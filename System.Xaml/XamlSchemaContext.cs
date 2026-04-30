using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Xaml.MS.Impl;
using System.Xaml.Schema;
using MS.Internal.Xaml.Parser;

namespace System.Xaml;

public class XamlSchemaContext
{
	private class AssemblyLoadHandler
	{
		private WeakReference schemaContextRef;

		public AssemblyLoadHandler(XamlSchemaContext schemaContext)
		{
			schemaContextRef = new WeakReference(schemaContext);
		}

		private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			((XamlSchemaContext)schemaContextRef.Target)?.SchemaContextAssemblyLoadEventHandler(sender, args);
		}

		[SecuritySafeCritical]
		public void Hook()
		{
			AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
		}

		[SecuritySafeCritical]
		public void Unhook()
		{
			AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
		}
	}

	private class WeakReferenceList<T> : List<WeakReference>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T : class
	{
		T IList<T>.this[int index]
		{
			get
			{
				return (T)base[index].Target;
			}
			set
			{
				base[index] = new WeakReference(value);
			}
		}

		bool ICollection<T>.IsReadOnly => false;

		public WeakReferenceList(int capacity)
			: base(capacity)
		{
		}

		int IList<T>.IndexOf(T item)
		{
			throw new NotSupportedException();
		}

		void IList<T>.Insert(int index, T item)
		{
			Insert(index, new WeakReference(item));
		}

		void ICollection<T>.Add(T item)
		{
			Add(new WeakReference(item));
		}

		bool ICollection<T>.Contains(T item)
		{
			foreach (WeakReference item2 in (IEnumerable<WeakReference>)this)
			{
				if (item == item2.Target)
				{
					return true;
				}
			}
			return false;
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			for (int i = 0; i < base.Count; i++)
			{
				array[i + arrayIndex] = (T)base[i].Target;
			}
		}

		bool ICollection<T>.Remove(T item)
		{
			throw new NotSupportedException();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return Enumerate().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}

		private IEnumerable<T> Enumerate()
		{
			foreach (WeakReference item in (IEnumerable<WeakReference>)this)
			{
				yield return (T)item.Target;
			}
		}
	}

	private const int ConcurrencyLevel = 1;

	private const int DictionaryCapacity = 17;

	private readonly ReadOnlyCollection<Assembly> _referenceAssemblies;

	private object _syncExaminingAssemblies;

	private IList<string> _nonClrNamespaces;

	private ConcurrentDictionary<string, string> _preferredPrefixes;

	private ConcurrentDictionary<string, string> _xmlNsCompatDict;

	private ConcurrentDictionary<Type, XamlType> _masterTypeList;

	private ConcurrentDictionary<ReferenceEqualityTuple<Type, XamlType, Type>, object> _masterValueConverterList;

	private ConcurrentDictionary<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember> _masterMemberList;

	private ConcurrentDictionary<XamlType, Dictionary<string, SpecialBracketCharacters>> _masterBracketCharacterCache;

	private readonly XamlSchemaContextSettings _settings;

	private ConcurrentDictionary<string, XamlNamespace> _namespaceByUriList;

	private ConcurrentDictionary<Assembly, XmlNsInfo> _xmlnsInfo;

	private ConcurrentDictionary<WeakRefKey, XmlNsInfo> _xmlnsInfoForDynamicAssemblies;

	private ConcurrentDictionary<Assembly, XmlNsInfo> _xmlnsInfoForUnreferencedAssemblies;

	private AssemblyLoadHandler _assemblyLoadHandler;

	private IList<Assembly> _unexaminedAssemblies;

	private bool _isGCCallbackPending;

	private object _syncAccessingUnexaminedAssemblies;

	private AssemblyName[] _referenceAssemblyNames;

	private ConcurrentDictionary<string, string> XmlNsCompatDict
	{
		get
		{
			if (_xmlNsCompatDict == null)
			{
				Interlocked.CompareExchange(ref _xmlNsCompatDict, CreateDictionary<string, string>(), null);
			}
			return _xmlNsCompatDict;
		}
	}

	private ConcurrentDictionary<XamlType, Dictionary<string, SpecialBracketCharacters>> MasterBracketCharacterCache
	{
		get
		{
			if (_masterBracketCharacterCache == null)
			{
				Interlocked.CompareExchange(ref _masterBracketCharacterCache, CreateDictionary<XamlType, Dictionary<string, SpecialBracketCharacters>>(), null);
			}
			return _masterBracketCharacterCache;
		}
	}

	private ConcurrentDictionary<Type, XamlType> MasterTypeList
	{
		get
		{
			if (_masterTypeList == null)
			{
				Interlocked.CompareExchange(ref _masterTypeList, CreateDictionary<Type, XamlType>(ReferenceEqualityComparer<Type>.Singleton), null);
			}
			return _masterTypeList;
		}
	}

	private ConcurrentDictionary<ReferenceEqualityTuple<Type, XamlType, Type>, object> MasterValueConverterList
	{
		get
		{
			if (_masterValueConverterList == null)
			{
				Interlocked.CompareExchange(ref _masterValueConverterList, CreateDictionary<ReferenceEqualityTuple<Type, XamlType, Type>, object>(), null);
			}
			return _masterValueConverterList;
		}
	}

	private ConcurrentDictionary<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember> MasterMemberList
	{
		get
		{
			if (_masterMemberList == null)
			{
				Interlocked.CompareExchange(ref _masterMemberList, CreateDictionary<ReferenceEqualityTuple<MemberInfo, MemberInfo>, XamlMember>(), null);
			}
			return _masterMemberList;
		}
	}

	public bool SupportMarkupExtensionsWithDuplicateArity => _settings.SupportMarkupExtensionsWithDuplicateArity;

	public bool FullyQualifyAssemblyNamesInClrNamespaces => _settings.FullyQualifyAssemblyNamesInClrNamespaces;

	public IList<Assembly> ReferenceAssemblies => _referenceAssemblies;

	private ConcurrentDictionary<Assembly, XmlNsInfo> XmlnsInfo
	{
		get
		{
			if (_xmlnsInfo == null)
			{
				Interlocked.CompareExchange(ref _xmlnsInfo, CreateDictionary<Assembly, XmlNsInfo>(ReferenceEqualityComparer<Assembly>.Singleton), null);
			}
			return _xmlnsInfo;
		}
	}

	private ConcurrentDictionary<WeakRefKey, XmlNsInfo> XmlnsInfoForDynamicAssemblies
	{
		get
		{
			if (_xmlnsInfoForDynamicAssemblies == null)
			{
				Interlocked.CompareExchange(ref _xmlnsInfoForDynamicAssemblies, CreateDictionary<WeakRefKey, XmlNsInfo>(), null);
			}
			return _xmlnsInfoForDynamicAssemblies;
		}
	}

	private ConcurrentDictionary<string, XamlNamespace> NamespaceByUriList
	{
		get
		{
			if (_namespaceByUriList == null)
			{
				Interlocked.CompareExchange(ref _namespaceByUriList, CreateDictionary<string, XamlNamespace>(), null);
			}
			return _namespaceByUriList;
		}
	}

	private ConcurrentDictionary<Assembly, XmlNsInfo> XmlnsInfoForUnreferencedAssemblies
	{
		get
		{
			if (_xmlnsInfoForUnreferencedAssemblies == null)
			{
				Interlocked.CompareExchange(ref _xmlnsInfoForUnreferencedAssemblies, CreateDictionary<Assembly, XmlNsInfo>(ReferenceEqualityComparer<Assembly>.Singleton), null);
			}
			return _xmlnsInfoForUnreferencedAssemblies;
		}
	}

	public XamlSchemaContext()
		: this(null, null)
	{
	}

	public XamlSchemaContext(XamlSchemaContextSettings settings)
		: this(null, settings)
	{
	}

	public XamlSchemaContext(IEnumerable<Assembly> referenceAssemblies)
		: this(referenceAssemblies, null)
	{
	}

	public XamlSchemaContext(IEnumerable<Assembly> referenceAssemblies, XamlSchemaContextSettings settings)
	{
		if (referenceAssemblies != null)
		{
			List<Assembly> list = new List<Assembly>(referenceAssemblies);
			_referenceAssemblies = new ReadOnlyCollection<Assembly>(list);
		}
		_settings = ((settings != null) ? new XamlSchemaContextSettings(settings) : new XamlSchemaContextSettings());
		_syncExaminingAssemblies = new object();
		InitializeAssemblyLoadHook();
	}

	~XamlSchemaContext()
	{
		try
		{
			if (_assemblyLoadHandler != null && !Environment.HasShutdownStarted)
			{
				_assemblyLoadHandler.Unhook();
			}
		}
		catch
		{
		}
	}

	public virtual ICollection<XamlType> GetAllXamlTypes(string xamlNamespace)
	{
		UpdateXmlNsInfo();
		XamlNamespace xamlNamespace2 = GetXamlNamespace(xamlNamespace);
		return xamlNamespace2.GetAllXamlTypes();
	}

	public virtual IEnumerable<string> GetAllXamlNamespaces()
	{
		UpdateXmlNsInfo();
		IList<string> list = _nonClrNamespaces;
		if (list == null)
		{
			lock (_syncExaminingAssemblies)
			{
				list = new List<string>();
				foreach (KeyValuePair<string, XamlNamespace> namespaceByUri in NamespaceByUriList)
				{
					if (namespaceByUri.Value.IsResolved && !namespaceByUri.Value.IsClrNamespace)
					{
						list.Add(namespaceByUri.Key);
					}
				}
				list = (_nonClrNamespaces = new ReadOnlyCollection<string>(list));
			}
		}
		return list;
	}

	public virtual string GetPreferredPrefix(string xmlns)
	{
		if (xmlns == null)
		{
			throw new ArgumentNullException("xmlns");
		}
		UpdateXmlNsInfo();
		if (_preferredPrefixes == null)
		{
			InitializePreferredPrefixes();
		}
		string clrNs;
		string assemblyName;
		if (!_preferredPrefixes.TryGetValue(xmlns, out var value))
		{
			return TryAdd(value: XamlLanguage.XamlNamespaces.Contains(xmlns) ? "x" : ((!ClrNamespaceUriParser.TryParseUri(xmlns, out clrNs, out assemblyName)) ? "p" : GetPrefixForClrNs(clrNs, assemblyName)), dictionary: _preferredPrefixes, key: xmlns);
		}
		return value;
	}

	private string GetPrefixForClrNs(string clrNs, string assemblyName)
	{
		if (string.IsNullOrEmpty(assemblyName))
		{
			return "local";
		}
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = clrNs.Split('.');
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.Append(char.ToLower(text[0], TypeConverterHelper.InvariantEnglishUS));
			}
		}
		if (stringBuilder.Length > 0)
		{
			string text2 = stringBuilder.ToString();
			if (KS.Eq(text2, "x"))
			{
				return "p";
			}
			if (KS.Eq(text2, "xml"))
			{
				return "p";
			}
			return text2;
		}
		return "local";
	}

	private void InitializePreferredPrefixes()
	{
		lock (_syncExaminingAssemblies)
		{
			ConcurrentDictionary<string, string> concurrentDictionary = CreateDictionary<string, string>();
			foreach (XmlNsInfo item in EnumerateXmlnsInfos())
			{
				UpdatePreferredPrefixes(item, concurrentDictionary);
			}
			_preferredPrefixes = concurrentDictionary;
		}
	}

	private void UpdatePreferredPrefixes(XmlNsInfo newNamespaces, ConcurrentDictionary<string, string> prefixDict)
	{
		foreach (KeyValuePair<string, string> prefix in newNamespaces.Prefixes)
		{
			string text = prefix.Value;
			if (!prefixDict.TryGetValue(prefix.Key, out var value))
			{
				value = TryAdd(prefixDict, prefix.Key, text);
			}
			while (value != text)
			{
				text = XmlNsInfo.GetPreferredPrefix(value, text);
				if (!KS.Eq(text, value))
				{
					value = TryUpdate(prefixDict, prefix.Key, text, value);
				}
			}
		}
	}

	public virtual XamlDirective GetXamlDirective(string xamlNamespace, string name)
	{
		if (xamlNamespace == null)
		{
			throw new ArgumentNullException("xamlNamespace");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (XamlLanguage.XamlNamespaces.Contains(xamlNamespace))
		{
			return XamlLanguage.LookupXamlDirective(name);
		}
		if (XamlLanguage.XmlNamespaces.Contains(xamlNamespace))
		{
			return XamlLanguage.LookupXmlDirective(name);
		}
		return null;
	}

	public XamlType GetXamlType(XamlTypeName xamlTypeName)
	{
		if (xamlTypeName == null)
		{
			throw new ArgumentNullException("xamlTypeName");
		}
		if (xamlTypeName.Name == null)
		{
			throw new ArgumentException(SR.Get("ReferenceIsNull", "xamlTypeName.Name"), "xamlTypeName");
		}
		if (xamlTypeName.Namespace == null)
		{
			throw new ArgumentException(SR.Get("ReferenceIsNull", "xamlTypeName.Namespace"), "xamlTypeName");
		}
		XamlType[] array = null;
		if (xamlTypeName.HasTypeArgs)
		{
			array = new XamlType[xamlTypeName.TypeArguments.Count];
			for (int i = 0; i < xamlTypeName.TypeArguments.Count; i++)
			{
				if (xamlTypeName.TypeArguments[i] == null)
				{
					throw new ArgumentException(SR.Get("CollectionCannotContainNulls", "xamlTypeName.TypeArguments"));
				}
				array[i] = GetXamlType(xamlTypeName.TypeArguments[i]);
				if (array[i] == null)
				{
					return null;
				}
			}
		}
		return GetXamlType(xamlTypeName.Namespace, xamlTypeName.Name, array);
	}

	protected internal virtual XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
	{
		if (xamlNamespace == null)
		{
			throw new ArgumentNullException("xamlNamespace");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (typeArguments != null)
		{
			foreach (XamlType xamlType in typeArguments)
			{
				if (xamlType == null)
				{
					throw new ArgumentException(SR.Get("CollectionCannotContainNulls", "typeArguments"));
				}
				if (xamlType.UnderlyingType == null)
				{
					return null;
				}
			}
		}
		XamlType xamlType2 = null;
		if (typeArguments == null || typeArguments.Length == 0)
		{
			xamlType2 = XamlLanguage.LookupXamlType(xamlNamespace, name);
			if (xamlType2 != null)
			{
				if (FullyQualifyAssemblyNamesInClrNamespaces)
				{
					xamlType2 = GetXamlType(xamlType2.UnderlyingType);
				}
				return xamlType2;
			}
		}
		XamlNamespace xamlNamespace2 = GetXamlNamespace(xamlNamespace);
		int revisionNumber = xamlNamespace2.RevisionNumber;
		xamlType2 = xamlNamespace2.GetXamlType(name, typeArguments);
		if (xamlType2 == null && !xamlNamespace2.IsClrNamespace)
		{
			UpdateXmlNsInfo();
			if (xamlNamespace2.RevisionNumber > revisionNumber)
			{
				xamlType2 = xamlNamespace2.GetXamlType(name, typeArguments);
			}
		}
		return xamlType2;
	}

	public virtual bool TryGetCompatibleXamlNamespace(string xamlNamespace, out string compatibleNamespace)
	{
		if (xamlNamespace == null)
		{
			throw new ArgumentNullException("xamlNamespace");
		}
		if (XmlNsCompatDict.TryGetValue(xamlNamespace, out compatibleNamespace))
		{
			return true;
		}
		UpdateXmlNsInfo();
		compatibleNamespace = GetCompatibleNamespace(xamlNamespace);
		if (compatibleNamespace == null)
		{
			compatibleNamespace = xamlNamespace;
		}
		XamlNamespace xamlNamespace2 = GetXamlNamespace(compatibleNamespace);
		if (xamlNamespace2.IsResolved)
		{
			compatibleNamespace = TryAdd(XmlNsCompatDict, xamlNamespace, compatibleNamespace);
			return true;
		}
		compatibleNamespace = null;
		return false;
	}

	private string GetCompatibleNamespace(string oldNs)
	{
		string text = null;
		Assembly assembly = null;
		lock (_syncExaminingAssemblies)
		{
			foreach (XmlNsInfo item in EnumerateXmlnsInfos())
			{
				Assembly assembly2 = item.Assembly;
				if (assembly2 == null)
				{
					continue;
				}
				IDictionary<string, string> dictionary = null;
				if (ReferenceAssemblies == null)
				{
					try
					{
						dictionary = item.OldToNewNs;
					}
					catch (Exception ex)
					{
						if (CriticalExceptions.IsCriticalException(ex))
						{
							throw;
						}
						continue;
					}
				}
				else
				{
					dictionary = item.OldToNewNs;
				}
				if (dictionary.TryGetValue(oldNs, out var value))
				{
					if (text != null && text != value)
					{
						throw new XamlSchemaException(SR.Get("DuplicateXmlnsCompatAcrossAssemblies", assembly.FullName, assembly2.FullName, oldNs));
					}
					text = value;
					assembly = assembly2;
				}
			}
			return text;
		}
	}

	public virtual XamlType GetXamlType(Type type)
	{
		return GetXamlType(type, XamlLanguage.TypeAlias(type));
	}

	internal XamlType GetXamlType(Type type, string alias)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		XamlType value = null;
		if (!MasterTypeList.TryGetValue(type, out value))
		{
			value = new XamlType(alias, type, this, null, null);
			value = TryAdd(MasterTypeList, type, value);
		}
		return value;
	}

	internal Dictionary<string, SpecialBracketCharacters> InitBracketCharacterCacheForType(XamlType type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		Dictionary<string, SpecialBracketCharacters> value = null;
		if (type.IsMarkupExtension && !MasterBracketCharacterCache.TryGetValue(type, out value))
		{
			value = BuildBracketCharacterCacheForType(type);
			value = TryAdd(MasterBracketCharacterCache, type, value);
		}
		return value;
	}

	private Dictionary<string, SpecialBracketCharacters> BuildBracketCharacterCacheForType(XamlType type)
	{
		Dictionary<string, SpecialBracketCharacters> dictionary = new Dictionary<string, SpecialBracketCharacters>(StringComparer.OrdinalIgnoreCase);
		ICollection<XamlMember> allMembers = type.GetAllMembers();
		foreach (XamlMember item in allMembers)
		{
			string constructorArgument = item.ConstructorArgument;
			string name = item.Name;
			IReadOnlyDictionary<char, char> markupExtensionBracketCharacters = item.MarkupExtensionBracketCharacters;
			SpecialBracketCharacters specialBracketCharacters = ((markupExtensionBracketCharacters != null && markupExtensionBracketCharacters.Count > 0) ? new SpecialBracketCharacters(markupExtensionBracketCharacters) : null);
			if (specialBracketCharacters != null)
			{
				specialBracketCharacters.EndInit();
				dictionary.Add(name, specialBracketCharacters);
				if (!string.IsNullOrEmpty(constructorArgument))
				{
					dictionary.Add(constructorArgument, specialBracketCharacters);
				}
			}
		}
		if (dictionary.Count <= 0)
		{
			return null;
		}
		return dictionary;
	}

	protected internal XamlValueConverter<TConverterBase> GetValueConverter<TConverterBase>(Type converterType, XamlType targetType) where TConverterBase : class
	{
		ReferenceEqualityTuple<Type, XamlType, Type> key = new ReferenceEqualityTuple<Type, XamlType, Type>(converterType, targetType, typeof(TConverterBase));
		if (!MasterValueConverterList.TryGetValue(key, out var value))
		{
			value = new XamlValueConverter<TConverterBase>(converterType, targetType);
			value = TryAdd(MasterValueConverterList, key, value);
		}
		return (XamlValueConverter<TConverterBase>)value;
	}

	internal virtual XamlMember GetProperty(PropertyInfo pi)
	{
		ReferenceEqualityTuple<MemberInfo, MemberInfo> key = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(pi, null);
		if (!MasterMemberList.TryGetValue(key, out var value))
		{
			value = new XamlMember(pi, this);
			return TryAdd(MasterMemberList, key, value);
		}
		return value;
	}

	internal virtual XamlMember GetEvent(EventInfo ei)
	{
		ReferenceEqualityTuple<MemberInfo, MemberInfo> key = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(ei, null);
		if (!MasterMemberList.TryGetValue(key, out var value))
		{
			value = new XamlMember(ei, this);
			return TryAdd(MasterMemberList, key, value);
		}
		return value;
	}

	internal virtual XamlMember GetAttachableProperty(string name, MethodInfo getter, MethodInfo setter)
	{
		ReferenceEqualityTuple<MemberInfo, MemberInfo> key = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(getter, setter);
		if (!MasterMemberList.TryGetValue(key, out var value))
		{
			value = new XamlMember(name, getter, setter, this);
			return TryAdd(MasterMemberList, key, value);
		}
		return value;
	}

	internal virtual XamlMember GetAttachableEvent(string name, MethodInfo adder)
	{
		ReferenceEqualityTuple<MemberInfo, MemberInfo> key = new ReferenceEqualityTuple<MemberInfo, MemberInfo>(adder, null);
		if (!MasterMemberList.TryGetValue(key, out var value))
		{
			value = new XamlMember(name, adder, this);
			return TryAdd(MasterMemberList, key, value);
		}
		return value;
	}

	internal bool AreInternalsVisibleTo(Assembly fromAssembly, Assembly toAssembly)
	{
		if (fromAssembly.Equals(toAssembly))
		{
			return true;
		}
		XmlNsInfo xmlNsInfo = GetXmlNsInfo(fromAssembly);
		ICollection<AssemblyName> internalsVisibleTo = xmlNsInfo.InternalsVisibleTo;
		if (internalsVisibleTo.Count == 0)
		{
			return false;
		}
		AssemblyName assemblyName = new AssemblyName(toAssembly.FullName);
		foreach (AssemblyName item in internalsVisibleTo)
		{
			if (item.Name == assemblyName.Name)
			{
				byte[] publicKeyToken = item.GetPublicKeyToken();
				if (publicKeyToken == null)
				{
					return true;
				}
				byte[] publicKeyToken2 = assemblyName.GetPublicKeyToken();
				return SafeSecurityHelper.IsSameKeyToken(publicKeyToken, publicKeyToken2);
			}
		}
		return false;
	}

	private static void CleanupCollectedAssemblies(object schemaContextWeakRef)
	{
		WeakReference weakReference = (WeakReference)schemaContextWeakRef;
		if (weakReference.Target is XamlSchemaContext xamlSchemaContext)
		{
			xamlSchemaContext.CleanupCollectedAssemblies();
		}
	}

	private void CleanupCollectedAssemblies()
	{
		bool flag = false;
		lock (_syncAccessingUnexaminedAssemblies)
		{
			_isGCCallbackPending = false;
			if (_unexaminedAssemblies is WeakReferenceList<Assembly>)
			{
				for (int num = _unexaminedAssemblies.Count - 1; num >= 0; num--)
				{
					Assembly assembly = _unexaminedAssemblies[num];
					if (assembly == null)
					{
						_unexaminedAssemblies.RemoveAt(num);
					}
					else if (assembly.IsDynamic)
					{
						flag = true;
					}
				}
			}
		}
		lock (_syncExaminingAssemblies)
		{
			if (_xmlnsInfoForDynamicAssemblies != null)
			{
				foreach (WeakRefKey key in _xmlnsInfoForDynamicAssemblies.Keys)
				{
					if (key.IsAlive)
					{
						flag = true;
					}
					else
					{
						_xmlnsInfoForDynamicAssemblies.TryRemove(key, out var _);
					}
				}
			}
		}
		if (flag)
		{
			RegisterAssemblyCleanup();
		}
	}

	private void RegisterAssemblyCleanup()
	{
		lock (_syncAccessingUnexaminedAssemblies)
		{
			if (!_isGCCallbackPending)
			{
				GCNotificationToken.RegisterCallback(CleanupCollectedAssemblies, new WeakReference(this));
				_isGCCallbackPending = true;
			}
		}
	}

	private IEnumerable<XmlNsInfo> EnumerateXmlnsInfos()
	{
		if (_xmlnsInfoForDynamicAssemblies == null)
		{
			return XmlnsInfo.Values;
		}
		return EnumerateStaticAndDynamicXmlnsInfos();
	}

	private IEnumerable<XmlNsInfo> EnumerateStaticAndDynamicXmlnsInfos()
	{
		foreach (XmlNsInfo value in XmlnsInfo.Values)
		{
			yield return value;
		}
		foreach (XmlNsInfo value2 in XmlnsInfoForDynamicAssemblies.Values)
		{
			yield return value2;
		}
	}

	internal string GetRootNamespace(Assembly asm)
	{
		XmlNsInfo xmlNsInfo = GetXmlNsInfo(asm);
		return xmlNsInfo.RootNamespace;
	}

	internal ReadOnlyCollection<string> GetXamlNamespaces(XamlType type)
	{
		Type underlyingType = type.UnderlyingType;
		if (underlyingType == null || underlyingType.Assembly == null)
		{
			return null;
		}
		if (XamlLanguage.AllTypes.Contains(type))
		{
			IList<string> xmlNsMappings = GetXmlNsMappings(underlyingType.Assembly, underlyingType.Namespace);
			List<string> list = new List<string>();
			list.AddRange(XamlLanguage.XamlNamespaces);
			list.AddRange(xmlNsMappings);
			return list.AsReadOnly();
		}
		return GetXmlNsMappings(underlyingType.Assembly, underlyingType.Namespace);
	}

	private XamlNamespace GetXamlNamespace(string xmlns)
	{
		XamlNamespace value = null;
		if (NamespaceByUriList.TryGetValue(xmlns, out value))
		{
			return value;
		}
		string clrNs;
		string assemblyName;
		return TryAdd(value: (!ClrNamespaceUriParser.TryParseUri(xmlns, out clrNs, out assemblyName)) ? new XamlNamespace(this) : new XamlNamespace(this, clrNs, assemblyName), dictionary: NamespaceByUriList, key: xmlns);
	}

	private XmlNsInfo GetXmlNsInfo(Assembly assembly)
	{
		if (XmlnsInfo.TryGetValue(assembly, out var value) || (_xmlnsInfoForDynamicAssemblies != null && assembly.IsDynamic && _xmlnsInfoForDynamicAssemblies.TryGetValue(new WeakRefKey(assembly), out value)) || (_xmlnsInfoForUnreferencedAssemblies != null && _xmlnsInfoForUnreferencedAssemblies.TryGetValue(assembly, out value)))
		{
			return value;
		}
		bool flag = false;
		if (_referenceAssemblies != null)
		{
			foreach (Assembly referenceAssembly in _referenceAssemblies)
			{
				if ((object)referenceAssembly == assembly)
				{
					flag = true;
					break;
				}
			}
		}
		else
		{
			flag = !assembly.ReflectionOnly && typeof(object).Assembly.GetType().IsAssignableFrom(assembly.GetType());
		}
		value = new XmlNsInfo(assembly, FullyQualifyAssemblyNamesInClrNamespaces);
		if (flag)
		{
			if (assembly.IsDynamic && _referenceAssemblies == null)
			{
				value = TryAdd(XmlnsInfoForDynamicAssemblies, new WeakRefKey(assembly), value);
				RegisterAssemblyCleanup();
			}
			else
			{
				value = TryAdd(XmlnsInfo, assembly, value);
			}
		}
		else
		{
			value = TryAdd(XmlnsInfoForUnreferencedAssemblies, assembly, value);
		}
		return value;
	}

	private ReadOnlyCollection<string> GetXmlNsMappings(Assembly assembly, string clrNs)
	{
		XmlNsInfo xmlNsInfo = GetXmlNsInfo(assembly);
		ConcurrentDictionary<string, IList<string>> clrToXmlNs = xmlNsInfo.ClrToXmlNs;
		clrNs = clrNs ?? string.Empty;
		if (!clrToXmlNs.TryGetValue(clrNs, out var value))
		{
			string assemblyName = (FullyQualifyAssemblyNamesInClrNamespaces ? assembly.FullName : GetAssemblyShortName(assembly));
			string uri = ClrNamespaceUriParser.GetUri(clrNs, assemblyName);
			List<string> list = new List<string>();
			list.Add(uri);
			value = list.AsReadOnly();
			TryAdd(clrToXmlNs, clrNs, value);
		}
		return (ReadOnlyCollection<string>)value;
	}

	private void InitializeAssemblyLoadHook()
	{
		_syncAccessingUnexaminedAssemblies = new object();
		if (ReferenceAssemblies == null)
		{
			_assemblyLoadHandler = new AssemblyLoadHandler(this);
			_assemblyLoadHandler.Hook();
			lock (_syncAccessingUnexaminedAssemblies)
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				_unexaminedAssemblies = new WeakReferenceList<Assembly>(assemblies.Length);
				bool flag = false;
				Assembly[] array = assemblies;
				foreach (Assembly assembly in array)
				{
					_unexaminedAssemblies.Add(assembly);
					if (assembly.IsDynamic)
					{
						flag = true;
					}
				}
				if (flag)
				{
					RegisterAssemblyCleanup();
				}
				return;
			}
		}
		_unexaminedAssemblies = new List<Assembly>(ReferenceAssemblies);
	}

	private void SchemaContextAssemblyLoadEventHandler(object sender, AssemblyLoadEventArgs args)
	{
		lock (_syncAccessingUnexaminedAssemblies)
		{
			if (!args.LoadedAssembly.ReflectionOnly && !_unexaminedAssemblies.Contains(args.LoadedAssembly))
			{
				_unexaminedAssemblies.Add(args.LoadedAssembly);
				if (args.LoadedAssembly.IsDynamic)
				{
					RegisterAssemblyCleanup();
				}
			}
		}
	}

	private void UpdateXmlNsInfo()
	{
		bool flag = false;
		lock (_syncExaminingAssemblies)
		{
			IList<Assembly> unexaminedAssemblies;
			lock (_syncAccessingUnexaminedAssemblies)
			{
				unexaminedAssemblies = _unexaminedAssemblies;
				_unexaminedAssemblies = new WeakReferenceList<Assembly>(0);
			}
			bool flag2 = ReferenceAssemblies != null;
			for (int i = 0; i < unexaminedAssemblies.Count; i++)
			{
				Assembly assembly = unexaminedAssemblies[i];
				if (assembly == null)
				{
					continue;
				}
				XmlNsInfo xmlNsInfo = GetXmlNsInfo(assembly);
				try
				{
					if (UpdateXmlNsInfo(xmlNsInfo))
					{
						flag = true;
					}
				}
				catch (Exception ex)
				{
					if (!flag2 && !CriticalExceptions.IsCriticalException(ex))
					{
						continue;
					}
					lock (_syncAccessingUnexaminedAssemblies)
					{
						for (int j = i; j < unexaminedAssemblies.Count; j++)
						{
							_unexaminedAssemblies.Add(unexaminedAssemblies[j]);
						}
					}
					throw;
				}
			}
			if (flag && _nonClrNamespaces != null)
			{
				_nonClrNamespaces = null;
			}
		}
	}

	private bool UpdateXmlNsInfo(XmlNsInfo nsInfo)
	{
		bool result = UpdateNamespaceByUriList(nsInfo);
		if (_preferredPrefixes != null)
		{
			UpdatePreferredPrefixes(nsInfo, _preferredPrefixes);
		}
		return result;
	}

	private bool UpdateNamespaceByUriList(XmlNsInfo nsInfo)
	{
		bool result = false;
		IList<XmlNsInfo.XmlNsDefinition> nsDefs = nsInfo.NsDefs;
		foreach (XmlNsInfo.XmlNsDefinition item in nsDefs)
		{
			AssemblyNamespacePair pair = new AssemblyNamespacePair(nsInfo.Assembly, item.ClrNamespace);
			XamlNamespace xamlNamespace = GetXamlNamespace(item.XmlNamespace);
			xamlNamespace.AddAssemblyNamespacePair(pair);
			result = true;
		}
		return result;
	}

	internal static string GetAssemblyShortName(Assembly assembly)
	{
		string fullName = assembly.FullName;
		return fullName.Substring(0, fullName.IndexOf(','));
	}

	internal static ConcurrentDictionary<K, V> CreateDictionary<K, V>()
	{
		return new ConcurrentDictionary<K, V>(1, 17);
	}

	internal static ConcurrentDictionary<K, V> CreateDictionary<K, V>(IEqualityComparer<K> comparer)
	{
		return new ConcurrentDictionary<K, V>(1, 17, comparer);
	}

	internal static V TryAdd<K, V>(ConcurrentDictionary<K, V> dictionary, K key, V value)
	{
		if (dictionary.TryAdd(key, value))
		{
			return value;
		}
		return dictionary[key];
	}

	internal static V TryUpdate<K, V>(ConcurrentDictionary<K, V> dictionary, K key, V value, V comparand)
	{
		if (dictionary.TryUpdate(key, value, comparand))
		{
			return value;
		}
		return dictionary[key];
	}

	protected internal virtual Assembly OnAssemblyResolve(string assemblyName)
	{
		if (string.IsNullOrEmpty(assemblyName))
		{
			return null;
		}
		if (_referenceAssemblies != null)
		{
			return ResolveReferenceAssembly(assemblyName);
		}
		return ResolveAssembly(assemblyName);
	}

	private Assembly ResolveReferenceAssembly(string assemblyName)
	{
		AssemblyName reference = new AssemblyName(assemblyName);
		if (_referenceAssemblyNames == null)
		{
			AssemblyName[] value = new AssemblyName[_referenceAssemblies.Count];
			Interlocked.CompareExchange(ref _referenceAssemblyNames, value, null);
		}
		for (int i = 0; i < _referenceAssemblies.Count; i++)
		{
			AssemblyName assemblyName2 = _referenceAssemblyNames[i];
			if (_referenceAssemblyNames[i] == null)
			{
				_referenceAssemblyNames[i] = new AssemblyName(_referenceAssemblies[i].FullName);
			}
			if (AssemblySatisfiesReference(_referenceAssemblyNames[i], reference))
			{
				return _referenceAssemblies[i];
			}
		}
		return null;
	}

	private static bool AssemblySatisfiesReference(AssemblyName assemblyName, AssemblyName reference)
	{
		if (reference.Name != assemblyName.Name)
		{
			return false;
		}
		if (reference.Version != null && !reference.Version.Equals(assemblyName.Version))
		{
			return false;
		}
		if (reference.CultureInfo != null && !reference.CultureInfo.Equals(assemblyName.CultureInfo))
		{
			return false;
		}
		byte[] publicKeyToken = reference.GetPublicKeyToken();
		if (publicKeyToken != null)
		{
			byte[] publicKeyToken2 = assemblyName.GetPublicKeyToken();
			if (!SafeSecurityHelper.IsSameKeyToken(publicKeyToken, publicKeyToken2))
			{
				return false;
			}
		}
		return true;
	}

	private Assembly ResolveAssembly(string assemblyName)
	{
		AssemblyName assemblyName2 = new AssemblyName(assemblyName);
		Assembly loadedAssembly = SafeSecurityHelper.GetLoadedAssembly(assemblyName2);
		if (loadedAssembly != null)
		{
			return loadedAssembly;
		}
		try
		{
			byte[] publicKeyToken = assemblyName2.GetPublicKeyToken();
			if (assemblyName2.Version != null || assemblyName2.CultureInfo != null || publicKeyToken != null)
			{
				try
				{
					return Assembly.Load(assemblyName);
				}
				catch (Exception ex)
				{
					if (CriticalExceptions.IsCriticalException(ex))
					{
						throw;
					}
					AssemblyName assemblyName3 = new AssemblyName(assemblyName2.Name);
					if (publicKeyToken != null)
					{
						assemblyName3.SetPublicKeyToken(publicKeyToken);
					}
					return Assembly.Load(assemblyName3);
				}
			}
			return Assembly.Load(new AssemblyName(assemblyName));
		}
		catch (Exception ex2)
		{
			if (CriticalExceptions.IsCriticalException(ex2))
			{
				throw;
			}
			return null;
		}
	}
}
