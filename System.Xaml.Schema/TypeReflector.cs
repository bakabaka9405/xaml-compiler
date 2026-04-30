using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows.Markup;
using System.Xaml.MS.Impl;

namespace System.Xaml.Schema;

internal class TypeReflector : Reflector
{
	private enum TypeVisibility
	{
		NotVisible,
		Internal,
		Public
	}

	internal class ThreadSafeDictionary<K, V> : Dictionary<K, V> where V : class
	{
		private bool _isComplete;

		public bool IsComplete
		{
			get
			{
				return _isComplete;
			}
			set
			{
				lock (this)
				{
					SetComplete();
				}
			}
		}

		internal ThreadSafeDictionary()
		{
		}

		public new bool TryGetValue(K name, out V member)
		{
			lock (this)
			{
				return base.TryGetValue(name, out member);
			}
		}

		public V TryAdd(K name, V member)
		{
			lock (this)
			{
				if (!base.TryGetValue(name, out V value))
				{
					if (!IsComplete)
					{
						Add(name, member);
					}
					return member;
				}
				return value;
			}
		}

		private void SetComplete()
		{
			List<K> list = null;
			using (Enumerator enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<K, V> current = enumerator.Current;
					if (current.Value == null)
					{
						if (list == null)
						{
							list = new List<K>();
						}
						list.Add(current.Key);
					}
				}
			}
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					Remove(list[i]);
				}
			}
			_isComplete = true;
		}
	}

	private const XamlCollectionKind XamlCollectionKindInvalid = (XamlCollectionKind)255;

	private const BindingFlags AllProperties_BF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private const BindingFlags AttachableProperties_BF = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

	private static TypeReflector s_UnknownReflector;

	private ThreadSafeDictionary<string, XamlMember> _nonAttachableMemberCache;

	private ThreadSafeDictionary<string, XamlMember> _attachableMemberCache;

	private int _boolTypeBits;

	private ThreadSafeDictionary<int, IList<XamlType>> _positionalParameterTypes;

	private ConcurrentDictionary<XamlDirective, XamlMember> _aliasedProperties;

	private XamlCollectionKind _collectionKind;

	private NullableReference<XamlMember> _contentProperty;

	private NullableReference<XamlMember> _runtimeNameProperty;

	private NullableReference<XamlMember> _xmlLangProperty;

	private NullableReference<XamlMember> _dictionaryKeyProperty;

	private NullableReference<XamlMember> _uidProperty;

	private NullableReference<MethodInfo> _isReadOnlyMethod;

	private NullableReference<XamlValueConverter<TypeConverter>> _typeConverter;

	private NullableReference<XamlValueConverter<ValueSerializer>> _valueSerializer;

	private NullableReference<XamlValueConverter<XamlDeferringLoader>> _deferringLoader;

	private NullableReference<EventHandler<XamlSetMarkupExtensionEventArgs>> _xamlSetMarkupExtensionHandler;

	private NullableReference<EventHandler<XamlSetTypeConverterEventArgs>> _xamlSetTypeConverterHandler;

	private NullableReference<MethodInfo> _addMethod;

	private NullableReference<XamlType> _baseType;

	private NullableReference<MethodInfo> _getEnumeratorMethod;

	internal static TypeReflector UnknownReflector
	{
		get
		{
			if (s_UnknownReflector == null)
			{
				s_UnknownReflector = new TypeReflector();
			}
			return s_UnknownReflector;
		}
	}

	internal IList<XamlType> AllowedContentTypes { get; set; }

	internal ThreadSafeDictionary<string, XamlMember> AttachableMembers
	{
		get
		{
			if (_attachableMemberCache == null)
			{
				Interlocked.CompareExchange(ref _attachableMemberCache, new ThreadSafeDictionary<string, XamlMember>(), null);
			}
			return _attachableMemberCache;
		}
	}

	internal XamlType BaseType
	{
		get
		{
			return _baseType.Value;
		}
		set
		{
			_baseType.Value = value;
		}
	}

	internal bool BaseTypeIsSet => _baseType.IsSet;

	internal XamlCollectionKind CollectionKind
	{
		get
		{
			return _collectionKind;
		}
		set
		{
			_collectionKind = value;
		}
	}

	internal bool CollectionKindIsSet => _collectionKind != (XamlCollectionKind)255;

	internal XamlMember ContentProperty
	{
		get
		{
			return _contentProperty.Value;
		}
		set
		{
			_contentProperty.Value = value;
		}
	}

	internal bool ContentPropertyIsSet => _contentProperty.IsSet;

	internal IList<XamlType> ContentWrappers { get; set; }

	internal XamlValueConverter<XamlDeferringLoader> DeferringLoader
	{
		get
		{
			return _deferringLoader.Value;
		}
		set
		{
			_deferringLoader.Value = value;
		}
	}

	internal bool DeferringLoaderIsSet => _deferringLoader.IsSet;

	internal ICollection<XamlMember> ExcludedReadOnlyMembers { get; set; }

	internal XamlType KeyType { get; set; }

	internal XamlTypeInvoker Invoker { get; set; }

	internal MethodInfo IsReadOnlyMethod
	{
		get
		{
			return _isReadOnlyMethod.Value;
		}
		set
		{
			_isReadOnlyMethod.Value = value;
		}
	}

	internal bool IsReadOnlyMethodIsSet => _isReadOnlyMethod.IsSet;

	internal bool IsUnknown => (_boolTypeBits & 0x100) != 0;

	internal XamlType ItemType { get; set; }

	internal XamlType MarkupExtensionReturnType { get; set; }

	internal ThreadSafeDictionary<string, XamlMember> Members
	{
		get
		{
			if (_nonAttachableMemberCache == null)
			{
				Interlocked.CompareExchange(ref _nonAttachableMemberCache, new ThreadSafeDictionary<string, XamlMember>(), null);
			}
			return _nonAttachableMemberCache;
		}
	}

	internal Dictionary<int, IList<XamlType>> ReflectedPositionalParameters { get; set; }

	internal XamlValueConverter<TypeConverter> TypeConverter
	{
		get
		{
			return _typeConverter.Value;
		}
		set
		{
			_typeConverter.Value = value;
		}
	}

	internal bool TypeConverterIsSet => _typeConverter.IsSet;

	internal Type UnderlyingType { get; set; }

	internal XamlValueConverter<ValueSerializer> ValueSerializer
	{
		get
		{
			return _valueSerializer.Value;
		}
		set
		{
			_valueSerializer.Value = value;
		}
	}

	internal bool ValueSerializerIsSet => _valueSerializer.IsSet;

	internal EventHandler<XamlSetMarkupExtensionEventArgs> XamlSetMarkupExtensionHandler
	{
		get
		{
			return _xamlSetMarkupExtensionHandler.Value;
		}
		set
		{
			_xamlSetMarkupExtensionHandler.Value = value;
		}
	}

	internal bool XamlSetMarkupExtensionHandlerIsSet => _xamlSetMarkupExtensionHandler.IsSet;

	internal EventHandler<XamlSetTypeConverterEventArgs> XamlSetTypeConverterHandler
	{
		get
		{
			return _xamlSetTypeConverterHandler.Value;
		}
		set
		{
			_xamlSetTypeConverterHandler.Value = value;
		}
	}

	internal bool XamlSetTypeConverterHandlerIsSet => _xamlSetTypeConverterHandler.IsSet;

	internal MethodInfo AddMethod
	{
		get
		{
			return _addMethod.Value;
		}
		set
		{
			_addMethod.Value = value;
		}
	}

	internal bool AddMethodIsSet => _addMethod.IsSet;

	internal MethodInfo GetEnumeratorMethod
	{
		get
		{
			return _getEnumeratorMethod.Value;
		}
		set
		{
			_getEnumeratorMethod.Value = value;
		}
	}

	internal bool GetEnumeratorMethodIsSet => _getEnumeratorMethod.IsSet;

	protected override MemberInfo Member => UnderlyingType;

	private TypeReflector()
	{
		_nonAttachableMemberCache = new ThreadSafeDictionary<string, XamlMember>();
		_nonAttachableMemberCache.IsComplete = true;
		_attachableMemberCache = new ThreadSafeDictionary<string, XamlMember>();
		_attachableMemberCache.IsComplete = true;
		_baseType.Value = XamlLanguage.Object;
		_boolTypeBits = -57015;
		_collectionKind = XamlCollectionKind.None;
		_addMethod.Value = null;
		_contentProperty.Value = null;
		_deferringLoader.Value = null;
		_dictionaryKeyProperty.Value = null;
		_getEnumeratorMethod.Value = null;
		_isReadOnlyMethod.Value = null;
		_runtimeNameProperty.Value = null;
		_typeConverter.Value = null;
		_uidProperty.Value = null;
		_valueSerializer.Value = null;
		_xamlSetMarkupExtensionHandler.Value = null;
		_xamlSetTypeConverterHandler.Value = null;
		_xmlLangProperty.Value = null;
		base.CustomAttributeProvider = null;
		Invoker = XamlTypeInvoker.UnknownInvoker;
	}

	public TypeReflector(Type underlyingType)
	{
		UnderlyingType = underlyingType;
		_collectionKind = (XamlCollectionKind)255;
	}

	internal static bool IsVisibleTo(Type type, Assembly accessingAssembly, XamlSchemaContext schemaContext)
	{
		switch (GetVisibility(type))
		{
		case TypeVisibility.NotVisible:
			return false;
		case TypeVisibility.Internal:
			if (!schemaContext.AreInternalsVisibleTo(type.Assembly, accessingAssembly))
			{
				return false;
			}
			break;
		}
		if (type.IsGenericType)
		{
			Type[] genericArguments = type.GetGenericArguments();
			foreach (Type type2 in genericArguments)
			{
				if (!IsVisibleTo(type2, accessingAssembly, schemaContext))
				{
					return false;
				}
			}
		}
		else if (type.HasElementType)
		{
			return IsVisibleTo(type.GetElementType(), accessingAssembly, schemaContext);
		}
		return true;
	}

	internal static bool IsInternal(Type type)
	{
		return GetVisibility(type) == TypeVisibility.Internal;
	}

	internal static bool IsPublicOrInternal(MethodBase method)
	{
		if (!method.IsPublic && !method.IsAssembly)
		{
			return method.IsFamilyOrAssembly;
		}
		return true;
	}

	internal bool TryGetPositionalParameters(int paramCount, out IList<XamlType> result)
	{
		result = null;
		if (_positionalParameterTypes == null)
		{
			if (IsUnknown)
			{
				return true;
			}
			Interlocked.CompareExchange(ref _positionalParameterTypes, new ThreadSafeDictionary<int, IList<XamlType>>(), null);
		}
		return _positionalParameterTypes.TryGetValue(paramCount, out result);
	}

	internal IList<XamlType> TryAddPositionalParameters(int paramCount, IList<XamlType> paramList)
	{
		return _positionalParameterTypes.TryAdd(paramCount, paramList);
	}

	internal bool TryGetAliasedProperty(XamlDirective directive, out XamlMember member)
	{
		member = null;
		if (IsUnknown)
		{
			return true;
		}
		bool result = false;
		if (directive == XamlLanguage.Key)
		{
			result = _dictionaryKeyProperty.IsSet;
			member = _dictionaryKeyProperty.Value;
		}
		else if (directive == XamlLanguage.Name)
		{
			result = _runtimeNameProperty.IsSet;
			member = _runtimeNameProperty.Value;
		}
		else if (directive == XamlLanguage.Uid)
		{
			result = _uidProperty.IsSet;
			member = _uidProperty.Value;
		}
		else if (directive == XamlLanguage.Lang)
		{
			result = _xmlLangProperty.IsSet;
			member = _xmlLangProperty.Value;
		}
		else if (_aliasedProperties != null)
		{
			result = _aliasedProperties.TryGetValue(directive, out member);
		}
		return result;
	}

	internal void TryAddAliasedProperty(XamlDirective directive, XamlMember member)
	{
		if (directive == XamlLanguage.Key)
		{
			_dictionaryKeyProperty.Value = member;
			return;
		}
		if (directive == XamlLanguage.Name)
		{
			_runtimeNameProperty.Value = member;
			return;
		}
		if (directive == XamlLanguage.Uid)
		{
			_uidProperty.Value = member;
			return;
		}
		if (directive == XamlLanguage.Lang)
		{
			_xmlLangProperty.Value = member;
			return;
		}
		if (_aliasedProperties == null)
		{
			ConcurrentDictionary<XamlDirective, XamlMember> value = XamlSchemaContext.CreateDictionary<XamlDirective, XamlMember>();
			Interlocked.CompareExchange(ref _aliasedProperties, value, null);
		}
		_aliasedProperties.TryAdd(directive, member);
	}

	internal static XamlMember LookupNameScopeProperty(XamlType xamlType)
	{
		if (xamlType.UnderlyingType == null)
		{
			return null;
		}
		object customAttribute = GetCustomAttribute(typeof(NameScopePropertyAttribute), xamlType.UnderlyingType);
		if (customAttribute is NameScopePropertyAttribute { Type: var type, Name: var name })
		{
			if (type != null)
			{
				XamlType xamlType2 = xamlType.SchemaContext.GetXamlType(type);
				return xamlType2.GetAttachableMember(name);
			}
			return xamlType.GetMember(name);
		}
		return null;
	}

	internal PropertyInfo LookupProperty(string name)
	{
		PropertyInfo propertyInfo = GetNonIndexerProperty(name);
		if (propertyInfo != null && IsPrivate(propertyInfo))
		{
			propertyInfo = null;
		}
		return propertyInfo;
	}

	internal EventInfo LookupEvent(string name)
	{
		EventInfo eventInfo = UnderlyingType.GetEvent(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (eventInfo != null && IsPrivate(eventInfo))
		{
			eventInfo = null;
		}
		return eventInfo;
	}

	internal void LookupAllMembers(out ICollection<PropertyInfo> newProperties, out ICollection<EventInfo> newEvents, out List<XamlMember> knownMembers)
	{
		PropertyInfo[] properties = UnderlyingType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		EventInfo[] events = UnderlyingType.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		knownMembers = new List<XamlMember>(properties.Length + events.Length);
		newProperties = FilterProperties(properties, knownMembers, skipKnownNegatives: true);
		newEvents = FilterEvents(events, knownMembers);
	}

	internal IList<PropertyInfo> LookupRemainingProperties()
	{
		PropertyInfo[] properties = UnderlyingType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		return FilterProperties(properties, null, skipKnownNegatives: false);
	}

	private IList<PropertyInfo> FilterProperties(PropertyInfo[] propList, List<XamlMember> knownMembers, bool skipKnownNegatives)
	{
		Dictionary<string, PropertyInfo> dictionary = new Dictionary<string, PropertyInfo>(propList.Length);
		foreach (PropertyInfo propertyInfo in propList)
		{
			if (propertyInfo.GetIndexParameters().Length != 0)
			{
				continue;
			}
			if (_nonAttachableMemberCache.TryGetValue(propertyInfo.Name, out var member))
			{
				if (member != null)
				{
					knownMembers?.Add(member);
					continue;
				}
				if (skipKnownNegatives)
				{
					continue;
				}
			}
			if (dictionary.TryGetValue(propertyInfo.Name, out var value))
			{
				if (value.DeclaringType.IsAssignableFrom(propertyInfo.DeclaringType))
				{
					dictionary[propertyInfo.Name] = propertyInfo;
				}
			}
			else
			{
				dictionary.Add(propertyInfo.Name, propertyInfo);
			}
		}
		if (dictionary.Count == 0)
		{
			return null;
		}
		List<PropertyInfo> list = new List<PropertyInfo>(dictionary.Count);
		foreach (PropertyInfo value2 in dictionary.Values)
		{
			if (!IsPrivate(value2))
			{
				list.Add(value2);
			}
		}
		return list;
	}

	private ICollection<EventInfo> FilterEvents(EventInfo[] eventList, List<XamlMember> knownMembers)
	{
		Dictionary<string, EventInfo> dictionary = new Dictionary<string, EventInfo>(eventList.Length);
		foreach (EventInfo eventInfo in eventList)
		{
			EventInfo value;
			if (_nonAttachableMemberCache.TryGetValue(eventInfo.Name, out var member))
			{
				if (member != null)
				{
					knownMembers.Add(member);
				}
			}
			else if (dictionary.TryGetValue(eventInfo.Name, out value))
			{
				if (value.DeclaringType.IsAssignableFrom(eventInfo.DeclaringType))
				{
					dictionary[eventInfo.Name] = eventInfo;
				}
			}
			else
			{
				dictionary.Add(eventInfo.Name, eventInfo);
			}
		}
		if (dictionary.Count == 0)
		{
			return null;
		}
		List<EventInfo> list = new List<EventInfo>(dictionary.Count);
		foreach (EventInfo value2 in dictionary.Values)
		{
			if (!IsPrivate(value2))
			{
				list.Add(value2);
			}
		}
		return list;
	}

	private PropertyInfo GetNonIndexerProperty(string name)
	{
		PropertyInfo propertyInfo = null;
		MemberInfo[] member = UnderlyingType.GetMember(name, MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		MemberInfo[] array = member;
		for (int i = 0; i < array.Length; i++)
		{
			PropertyInfo propertyInfo2 = (PropertyInfo)array[i];
			if (propertyInfo2.GetIndexParameters().Length == 0 && (propertyInfo == null || propertyInfo.DeclaringType.IsAssignableFrom(propertyInfo2.DeclaringType)))
			{
				propertyInfo = propertyInfo2;
			}
		}
		return propertyInfo;
	}

	private static bool IsPrivate(PropertyInfo pi)
	{
		if (IsPrivateOrNull(pi.GetGetMethod(nonPublic: true)))
		{
			return IsPrivateOrNull(pi.GetSetMethod(nonPublic: true));
		}
		return false;
	}

	private static bool IsPrivate(EventInfo ei)
	{
		return IsPrivateOrNull(ei.GetAddMethod(nonPublic: true));
	}

	private static bool IsPrivateOrNull(MethodInfo mi)
	{
		if (!(mi == null))
		{
			return mi.IsPrivate;
		}
		return true;
	}

	private void PickAttachablePropertyAccessors(List<MethodInfo> getters, List<MethodInfo> setters, out MethodInfo getter, out MethodInfo setter)
	{
		List<KeyValuePair<MethodInfo, MethodInfo>> list = new List<KeyValuePair<MethodInfo, MethodInfo>>();
		if (setters != null && getters != null)
		{
			foreach (MethodInfo setter2 in setters)
			{
				foreach (MethodInfo getter2 in getters)
				{
					ParameterInfo[] parameters = getter2.GetParameters();
					ParameterInfo[] parameters2 = setter2.GetParameters();
					if (parameters[0].ParameterType == parameters2[0].ParameterType && getter2.ReturnType == parameters2[1].ParameterType)
					{
						list.Add(new KeyValuePair<MethodInfo, MethodInfo>(getter2, setter2));
					}
				}
			}
		}
		if (list.Count > 0)
		{
			getter = list[0].Key;
			setter = list[0].Value;
		}
		else if (setters == null || setters.Count == 0 || (getters != null && getters.Count > 0 && UnderlyingType.IsVisible && getters[0].IsPublic && !setters[0].IsPublic))
		{
			getter = getters[0];
			setter = null;
		}
		else
		{
			getter = null;
			setter = setters[0];
		}
	}

	private MethodInfo PickAttachableEventAdder(IEnumerable<MethodInfo> adders)
	{
		if (adders != null)
		{
			foreach (MethodInfo adder in adders)
			{
				if (!adder.IsPrivate)
				{
					return adder;
				}
			}
		}
		return null;
	}

	internal bool LookupAttachableProperty(string name, out MethodInfo getter, out MethodInfo setter)
	{
		List<MethodInfo> list = LookupStaticSetters(name);
		List<MethodInfo> list2 = LookupStaticGetters(name);
		if ((list == null || list.Count == 0) && (list2 == null || list2.Count == 0))
		{
			getter = null;
			setter = null;
			return false;
		}
		PickAttachablePropertyAccessors(list2, list, out getter, out setter);
		return true;
	}

	internal MethodInfo LookupAttachableEvent(string name)
	{
		List<MethodInfo> list = LookupStaticAdders(name);
		if (list == null || list.Count == 0)
		{
			return null;
		}
		return PickAttachableEventAdder(list);
	}

	private void LookupAllStaticAccessors(out Dictionary<string, List<MethodInfo>> getters, out Dictionary<string, List<MethodInfo>> setters, out Dictionary<string, List<MethodInfo>> adders)
	{
		getters = new Dictionary<string, List<MethodInfo>>();
		setters = new Dictionary<string, List<MethodInfo>>();
		adders = new Dictionary<string, List<MethodInfo>>();
		MethodInfo[] methods = UnderlyingType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		if (UnderlyingType.IsVisible)
		{
			LookupAllStaticAccessorsHelper(methods, getters, setters, adders, isUnderlyingTypePublic: true);
		}
		else
		{
			LookupAllStaticAccessorsHelper(methods, getters, setters, adders, isUnderlyingTypePublic: false);
		}
	}

	private void LookupAllStaticAccessorsHelper(MethodInfo[] allMethods, Dictionary<string, List<MethodInfo>> getters, Dictionary<string, List<MethodInfo>> setters, Dictionary<string, List<MethodInfo>> adders, bool isUnderlyingTypePublic)
	{
		foreach (MethodInfo methodInfo in allMethods)
		{
			if (!methodInfo.IsPrivate)
			{
				if (IsAttachablePropertyGetter(methodInfo, out var name))
				{
					AddToMultiDict(getters, name, methodInfo, isUnderlyingTypePublic);
				}
				else if (IsAttachablePropertySetter(methodInfo, out name))
				{
					AddToMultiDict(setters, name, methodInfo, isUnderlyingTypePublic);
				}
				else if (IsAttachableEventAdder(methodInfo, out name))
				{
					AddToMultiDict(adders, name, methodInfo, isUnderlyingTypePublic);
				}
			}
		}
	}

	private List<MethodInfo> LookupStaticAdders(string name)
	{
		string name2 = "Add" + name + "Handler";
		MemberInfo[] member = UnderlyingType.GetMember(name2, MemberTypes.Method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		PrioritizeAccessors(member, isEvent: true, isGetter: false, out var preferredAccessors, out var otherAccessors);
		return preferredAccessors ?? otherAccessors;
	}

	private List<MethodInfo> LookupStaticGetters(string name)
	{
		MemberInfo[] member = UnderlyingType.GetMember("Get" + name, MemberTypes.Method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		PrioritizeAccessors(member, isEvent: false, isGetter: true, out var preferredAccessors, out var otherAccessors);
		return preferredAccessors ?? otherAccessors;
	}

	private List<MethodInfo> LookupStaticSetters(string name)
	{
		MemberInfo[] member = UnderlyingType.GetMember("Set" + name, MemberTypes.Method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		PrioritizeAccessors(member, isEvent: false, isGetter: false, out var preferredAccessors, out var otherAccessors);
		return preferredAccessors ?? otherAccessors;
	}

	private void PrioritizeAccessors(MemberInfo[] accessors, bool isEvent, bool isGetter, out List<MethodInfo> preferredAccessors, out List<MethodInfo> otherAccessors)
	{
		preferredAccessors = null;
		otherAccessors = null;
		if (UnderlyingType.IsVisible)
		{
			for (int i = 0; i < accessors.Length; i++)
			{
				MethodInfo methodInfo = (MethodInfo)accessors[i];
				if (methodInfo.IsPublic && IsAttachablePropertyAccessor(isEvent, isGetter, methodInfo))
				{
					if (preferredAccessors == null)
					{
						preferredAccessors = new List<MethodInfo>();
					}
					preferredAccessors.Add(methodInfo);
				}
				else if (!methodInfo.IsPrivate && IsAttachablePropertyAccessor(isEvent, isGetter, methodInfo))
				{
					if (otherAccessors == null)
					{
						otherAccessors = new List<MethodInfo>();
					}
					otherAccessors.Add(methodInfo);
				}
			}
			return;
		}
		for (int j = 0; j < accessors.Length; j++)
		{
			MethodInfo methodInfo2 = (MethodInfo)accessors[j];
			if (!methodInfo2.IsPrivate && IsAttachablePropertyAccessor(isEvent, isGetter, methodInfo2))
			{
				if (preferredAccessors == null)
				{
					preferredAccessors = new List<MethodInfo>();
				}
				preferredAccessors.Add(methodInfo2);
			}
		}
	}

	private bool IsAttachablePropertyAccessor(bool isEvent, bool isGetter, MethodInfo accessor)
	{
		if (isEvent)
		{
			return IsAttachableEventAdder(accessor);
		}
		if (isGetter)
		{
			return IsAttachablePropertyGetter(accessor);
		}
		return IsAttachablePropertySetter(accessor);
	}

	private static void AddToMultiDict(Dictionary<string, List<MethodInfo>> dict, string name, MethodInfo value, bool isUnderlyingTypePublic)
	{
		if (dict.TryGetValue(name, out var value2))
		{
			if (isUnderlyingTypePublic)
			{
				if (value.IsPublic)
				{
					if (!value2[0].IsPublic)
					{
						value2.Clear();
					}
					value2.Add(value);
				}
				else if (!value2[0].IsPublic)
				{
					value2.Add(value);
				}
			}
			else
			{
				value2.Add(value);
			}
		}
		else
		{
			value2 = new List<MethodInfo>();
			dict.Add(name, value2);
			value2.Add(value);
		}
	}

	private bool IsAttachablePropertyGetter(MethodInfo mi, out string name)
	{
		name = null;
		if (!KS.StartsWith(mi.Name, "Get"))
		{
			return false;
		}
		if (!IsAttachablePropertyGetter(mi))
		{
			return false;
		}
		name = mi.Name.Substring("Get".Length);
		return true;
	}

	private bool IsAttachablePropertyGetter(MethodInfo mi)
	{
		ParameterInfo[] parameters = mi.GetParameters();
		if (parameters.Length == 1)
		{
			return mi.ReturnType != typeof(void);
		}
		return false;
	}

	private bool IsAttachablePropertySetter(MethodInfo mi, out string name)
	{
		name = null;
		if (!KS.StartsWith(mi.Name, "Set"))
		{
			return false;
		}
		if (!IsAttachablePropertySetter(mi))
		{
			return false;
		}
		name = mi.Name.Substring("Set".Length);
		return true;
	}

	private bool IsAttachablePropertySetter(MethodInfo mi)
	{
		ParameterInfo[] parameters = mi.GetParameters();
		return parameters.Length == 2;
	}

	private bool IsAttachableEventAdder(MethodInfo mi, out string name)
	{
		name = null;
		if (!KS.StartsWith(mi.Name, "Add") || !KS.EndsWith(mi.Name, "Handler"))
		{
			return false;
		}
		if (!IsAttachableEventAdder(mi))
		{
			return false;
		}
		name = mi.Name.Substring("Add".Length, mi.Name.Length - "Add".Length - "Handler".Length);
		return true;
	}

	private bool IsAttachableEventAdder(MethodInfo mi)
	{
		ParameterInfo[] parameters = mi.GetParameters();
		if (parameters.Length == 2)
		{
			return typeof(Delegate).IsAssignableFrom(parameters[1].ParameterType);
		}
		return false;
	}

	internal IList<XamlMember> LookupAllAttachableMembers(XamlSchemaContext schemaContext)
	{
		List<XamlMember> result = new List<XamlMember>();
		LookupAllStaticAccessors(out var getters, out var setters, out var adders);
		GetOrCreateAttachableProperties(schemaContext, result, getters, setters);
		GetOrCreateAttachableEvents(schemaContext, result, adders);
		return result;
	}

	private void GetOrCreateAttachableProperties(XamlSchemaContext schemaContext, List<XamlMember> result, Dictionary<string, List<MethodInfo>> getters, Dictionary<string, List<MethodInfo>> setters)
	{
		foreach (KeyValuePair<string, List<MethodInfo>> setter2 in setters)
		{
			string key = setter2.Key;
			XamlMember member = null;
			if (!_attachableMemberCache.TryGetValue(key, out member))
			{
				getters.TryGetValue(key, out var value);
				getters.Remove(key);
				PickAttachablePropertyAccessors(value, setter2.Value, out var getter, out var setter);
				member = schemaContext.GetAttachableProperty(key, getter, setter);
				if (member.IsReadOnly && !member.Type.IsUsableAsReadOnly)
				{
					member = null;
				}
			}
			if (member != null)
			{
				result.Add(member);
			}
		}
		foreach (KeyValuePair<string, List<MethodInfo>> getter2 in getters)
		{
			string key2 = getter2.Key;
			XamlMember member2 = null;
			if (!_attachableMemberCache.TryGetValue(key2, out member2))
			{
				member2 = schemaContext.GetAttachableProperty(key2, getter2.Value[0], null);
			}
			result.Add(member2);
		}
	}

	private void GetOrCreateAttachableEvents(XamlSchemaContext schemaContext, List<XamlMember> result, Dictionary<string, List<MethodInfo>> adders)
	{
		foreach (KeyValuePair<string, List<MethodInfo>> adder2 in adders)
		{
			string key = adder2.Key;
			XamlMember member = null;
			if (!_attachableMemberCache.TryGetValue(key, out member))
			{
				MethodInfo adder = PickAttachableEventAdder(adder2.Value);
				member = schemaContext.GetAttachableEvent(key, adder);
			}
			if (member != null)
			{
				result.Add(member);
			}
		}
	}

	internal bool? GetFlag(BoolTypeBits typeBit)
	{
		return Reflector.GetFlag(_boolTypeBits, (int)typeBit);
	}

	internal void SetFlag(BoolTypeBits typeBit, bool value)
	{
		Reflector.SetFlag(ref _boolTypeBits, (int)typeBit, value);
	}

	private static object GetCustomAttribute(Type attrType, Type reflectedType)
	{
		object[] customAttributes = reflectedType.GetCustomAttributes(attrType, inherit: true);
		if (customAttributes.Length == 0)
		{
			return null;
		}
		if (customAttributes.Length > 1)
		{
			string message = SR.Get("TooManyAttributesOnType", reflectedType.Name, attrType.Name);
			throw new XamlSchemaException(message);
		}
		return customAttributes[0];
	}

	private static TypeVisibility GetVisibility(Type type)
	{
		bool flag = false;
		while (type.IsNested)
		{
			if (type.IsNestedAssembly || type.IsNestedFamORAssem)
			{
				flag = true;
			}
			else if (!type.IsNestedPublic)
			{
				return TypeVisibility.NotVisible;
			}
			type = type.DeclaringType;
		}
		bool isNotPublic = type.IsNotPublic;
		if (!(isNotPublic || flag))
		{
			return TypeVisibility.Public;
		}
		return TypeVisibility.Internal;
	}
}
