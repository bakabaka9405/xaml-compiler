using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Context;
using System.Xaml.Schema;
using MS.Internal.Xaml.Runtime;

namespace MS.Internal.Xaml.Context;

internal class ObjectWriterContext : XamlContext
{
	internal class NameScopeInitializationCompleteSubscriber
	{
		private List<INameScopeDictionary> _nameScopeDictionaryList = new List<INameScopeDictionary>();

		public EventHandler Handler { get; set; }

		public List<INameScopeDictionary> NameScopeDictionaryList => _nameScopeDictionaryList;
	}

	private class StackWalkNameResolver : IXamlNameResolver
	{
		private List<INameScopeDictionary> _nameScopeDictionaryList;

		public bool IsFixupTokenAvailable => false;

		public event EventHandler OnNameScopeInitializationComplete
		{
			add
			{
			}
			remove
			{
			}
		}

		public StackWalkNameResolver(List<INameScopeDictionary> nameScopeDictionaryList)
		{
			_nameScopeDictionaryList = nameScopeDictionaryList;
		}

		public object GetFixupToken(IEnumerable<string> name)
		{
			return null;
		}

		public object GetFixupToken(IEnumerable<string> name, bool canAssignDirectly)
		{
			return null;
		}

		public object Resolve(string name)
		{
			object result = null;
			foreach (INameScopeDictionary nameScopeDictionary in _nameScopeDictionaryList)
			{
				object obj = nameScopeDictionary.FindName(name);
				if (obj != null)
				{
					result = obj;
					break;
				}
			}
			return result;
		}

		public object Resolve(string name, out bool isFullyInitialized)
		{
			object obj = Resolve(name);
			isFullyInitialized = obj != null;
			return obj;
		}

		public IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope()
		{
			List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();
			foreach (INameScopeDictionary nameScopeDictionary in _nameScopeDictionaryList)
			{
				foreach (KeyValuePair<string, object> nameValuePair in nameScopeDictionary)
				{
					if (!list.Exists((KeyValuePair<string, object> pair) => pair.Key == nameValuePair.Key))
					{
						list.Add(nameValuePair);
					}
				}
			}
			return list;
		}
	}

	private XamlContextStack<ObjectWriterFrame> _stack;

	private object _rootInstance;

	private ServiceProviderContext _serviceProviderContext;

	private XamlRuntime _runtime;

	private int _savedDepth;

	private bool _nameResolutionComplete;

	private XamlObjectWriterSettings _settings;

	private List<NameScopeInitializationCompleteSubscriber> _nameScopeInitializationCompleteSubscribers;

	public override Assembly LocalAssembly
	{
		get
		{
			Assembly assembly = base.LocalAssembly;
			if (assembly == null && _settings != null && _settings.AccessLevel != null)
			{
				assembly = (base.LocalAssembly = Assembly.Load(_settings.AccessLevel.AssemblyAccessToAssemblyName));
			}
			return assembly;
		}
		protected set
		{
			base.LocalAssembly = value;
		}
	}

	internal ICheckIfInitialized IsInitializedCallback { get; set; }

	internal bool NameResolutionComplete
	{
		get
		{
			return _nameResolutionComplete;
		}
		set
		{
			_nameResolutionComplete = value;
		}
	}

	internal XamlRuntime Runtime => _runtime;

	internal ServiceProviderContext ServiceProviderContext
	{
		get
		{
			if (_serviceProviderContext == null)
			{
				_serviceProviderContext = new ServiceProviderContext(this);
			}
			return _serviceProviderContext;
		}
	}

	public int Depth => _stack.Depth;

	public int SavedDepth => _savedDepth;

	public int LiveDepth => Depth - SavedDepth;

	public XamlType CurrentType
	{
		get
		{
			return _stack.CurrentFrame.XamlType;
		}
		set
		{
			_stack.CurrentFrame.XamlType = value;
		}
	}

	public XamlType ParentType => _stack.PreviousFrame.XamlType;

	public XamlType GrandParentType
	{
		get
		{
			if (_stack.PreviousPreviousFrame == null)
			{
				return null;
			}
			return _stack.PreviousPreviousFrame.XamlType;
		}
	}

	public XamlMember CurrentProperty
	{
		get
		{
			return _stack.CurrentFrame.Member;
		}
		set
		{
			_stack.CurrentFrame.Member = value;
		}
	}

	public XamlMember ParentProperty => _stack.PreviousFrame.Member;

	public XamlMember GrandParentProperty => _stack.PreviousPreviousFrame.Member;

	public object CurrentInstance
	{
		get
		{
			return _stack.CurrentFrame.Instance;
		}
		set
		{
			_stack.CurrentFrame.Instance = value;
		}
	}

	public object ParentInstance => _stack.PreviousFrame.Instance;

	public object GrandParentInstance
	{
		get
		{
			if (_stack.PreviousPreviousFrame == null)
			{
				return null;
			}
			return _stack.PreviousPreviousFrame.Instance;
		}
	}

	public object CurrentCollection
	{
		get
		{
			return _stack.CurrentFrame.Collection;
		}
		set
		{
			_stack.CurrentFrame.Collection = value;
		}
	}

	public object ParentCollection => _stack.PreviousFrame.Collection;

	public bool CurrentWasAssignedAtCreation
	{
		get
		{
			return _stack.CurrentFrame.WasAssignedAtCreation;
		}
		set
		{
			_stack.CurrentFrame.WasAssignedAtCreation = value;
		}
	}

	public bool CurrentIsObjectFromMember
	{
		get
		{
			return _stack.CurrentFrame.IsObjectFromMember;
		}
		set
		{
			_stack.CurrentFrame.IsObjectFromMember = value;
		}
	}

	public bool ParentIsObjectFromMember => _stack.PreviousFrame.IsObjectFromMember;

	public bool GrandParentIsObjectFromMember
	{
		get
		{
			if (_stack.PreviousPreviousFrame == null)
			{
				return false;
			}
			return _stack.PreviousPreviousFrame.IsObjectFromMember;
		}
	}

	public bool CurrentIsPropertyValueSet
	{
		set
		{
			_stack.CurrentFrame.IsPropertyValueSet = value;
		}
	}

	public bool ParentIsPropertyValueSet
	{
		get
		{
			return _stack.PreviousFrame.IsPropertyValueSet;
		}
		set
		{
			_stack.PreviousFrame.IsPropertyValueSet = value;
		}
	}

	public bool CurrentIsTypeConvertedObject
	{
		get
		{
			return _stack.CurrentFrame.IsTypeConvertedObject;
		}
		set
		{
			_stack.CurrentFrame.IsTypeConvertedObject = value;
		}
	}

	public Dictionary<XamlMember, object> CurrentPreconstructionPropertyValues => _stack.CurrentFrame.PreconstructionPropertyValues;

	public bool CurrentHasPreconstructionPropertyValuesDictionary => _stack.CurrentFrame.HasPreconstructionPropertyValuesDictionary;

	public Dictionary<XamlMember, object> ParentPreconstructionPropertyValues => _stack.PreviousFrame.PreconstructionPropertyValues;

	public System.Xaml.Context.HashSet<XamlMember> CurrentAssignedProperties => _stack.CurrentFrame.AssignedProperties;

	public System.Xaml.Context.HashSet<XamlMember> ParentAssignedProperties => _stack.PreviousFrame.AssignedProperties;

	public string CurrentInstanceRegisteredName
	{
		get
		{
			return _stack.CurrentFrame.InstanceRegisteredName;
		}
		set
		{
			_stack.CurrentFrame.InstanceRegisteredName = value;
		}
	}

	public string ParentInstanceRegisteredName
	{
		get
		{
			return _stack.PreviousFrame.InstanceRegisteredName;
		}
		set
		{
			_stack.PreviousFrame.InstanceRegisteredName = value;
		}
	}

	public Uri BaseUri { get; set; }

	public int LineNumber { get; set; }

	public int LinePosition { get; set; }

	public Uri SourceBamlUri
	{
		get
		{
			if (_settings == null)
			{
				return null;
			}
			return _settings.SourceBamlUri;
		}
	}

	public int LineNumber_StartObject { get; set; }

	public int LinePosition_StartObject { get; set; }

	public INameScopeDictionary CurrentNameScope => LookupNameScopeDictionary(_stack.CurrentFrame);

	public INameScopeDictionary ParentNameScope => LookupNameScopeDictionary(_stack.PreviousFrame);

	public INameScopeDictionary GrandParentNameScope => LookupNameScopeDictionary(_stack.PreviousPreviousFrame);

	public INameScopeDictionary RootNameScope
	{
		get
		{
			ObjectWriterFrame frame = _stack.GetFrame(SavedDepth + 1);
			return LookupNameScopeDictionary(frame);
		}
	}

	public object[] CurrentCtorArgs
	{
		get
		{
			return _stack.CurrentFrame.PositionalCtorArgs;
		}
		set
		{
			_stack.CurrentFrame.PositionalCtorArgs = value;
		}
	}

	public object CurrentKey => _stack.CurrentFrame.Key;

	public bool CurrentIsKeySet => _stack.CurrentFrame.IsKeySet;

	public object ParentKey
	{
		get
		{
			return _stack.PreviousFrame.Key;
		}
		set
		{
			_stack.PreviousFrame.Key = value;
		}
	}

	public bool CurrentKeyIsUnconverted
	{
		get
		{
			return _stack.CurrentFrame.KeyIsUnconverted;
		}
		set
		{
			_stack.CurrentFrame.KeyIsUnconverted = value;
		}
	}

	public bool ParentKeyIsUnconverted
	{
		set
		{
			_stack.PreviousFrame.KeyIsUnconverted = value;
		}
	}

	public bool ParentShouldConvertChildKeys
	{
		get
		{
			return _stack.PreviousFrame.ShouldConvertChildKeys;
		}
		set
		{
			_stack.PreviousPreviousFrame.ShouldConvertChildKeys = value;
		}
	}

	public bool GrandParentShouldConvertChildKeys
	{
		get
		{
			return _stack.PreviousPreviousFrame.ShouldConvertChildKeys;
		}
		set
		{
			_stack.PreviousPreviousFrame.ShouldConvertChildKeys = value;
		}
	}

	public bool ParentShouldNotConvertChildKeys
	{
		get
		{
			return _stack.PreviousFrame.ShouldNotConvertChildKeys;
		}
		set
		{
			_stack.PreviousPreviousFrame.ShouldNotConvertChildKeys = value;
		}
	}

	public bool GrandParentShouldNotConvertChildKeys => _stack.PreviousPreviousFrame.ShouldNotConvertChildKeys;

	public object RootInstance
	{
		get
		{
			if (_rootInstance == null)
			{
				ObjectWriterFrame topFrame = GetTopFrame();
				_rootInstance = topFrame.Instance;
			}
			return _rootInstance;
		}
	}

	public IEnumerable<INameScopeDictionary> StackWalkOfNameScopes
	{
		get
		{
			ObjectWriterFrame frame = _stack.CurrentFrame;
			INameScopeDictionary previousNameScopeDictionary = null;
			while (frame.Depth > 0)
			{
				INameScopeDictionary nameScopeDictionary = LookupNameScopeDictionary(frame);
				if (frame.NameScopeDictionary != previousNameScopeDictionary)
				{
					previousNameScopeDictionary = nameScopeDictionary;
					yield return nameScopeDictionary;
				}
				frame = (ObjectWriterFrame)frame.Previous;
			}
			if (frame.NameScopeDictionary != null && frame.NameScopeDictionary != previousNameScopeDictionary)
			{
				yield return frame.NameScopeDictionary;
			}
		}
	}

	public ObjectWriterContext(XamlSavedContext savedContext, XamlObjectWriterSettings settings, INameScope rootNameScope, XamlRuntime runtime)
		: base(savedContext.SchemaContext)
	{
		_stack = new XamlContextStack<ObjectWriterFrame>(savedContext.Stack, copy: false);
		if (settings != null)
		{
			_settings = settings.StripDelegates();
		}
		_runtime = runtime;
		BaseUri = savedContext.BaseUri;
		switch (savedContext.SaveContextType)
		{
		case SavedContextType.Template:
		{
			INameScopeDictionary nameScopeDictionary = null;
			if (rootNameScope == null)
			{
				nameScopeDictionary = new NameScope();
			}
			else
			{
				nameScopeDictionary = rootNameScope as INameScopeDictionary;
				if (nameScopeDictionary == null)
				{
					nameScopeDictionary = new NameScopeDictionary(rootNameScope);
				}
			}
			_stack.PushScope();
			_savedDepth = _stack.Depth;
			_stack.CurrentFrame.NameScopeDictionary = nameScopeDictionary;
			_stack.PushScope();
			break;
		}
		case SavedContextType.ReparseValue:
		case SavedContextType.ReparseMarkupExtension:
			_savedDepth = _stack.Depth - 1;
			break;
		}
	}

	public ObjectWriterContext(XamlSchemaContext schemaContext, XamlObjectWriterSettings settings, INameScope rootNameScope, XamlRuntime runtime)
		: base(schemaContext)
	{
		_stack = new XamlContextStack<ObjectWriterFrame>(() => new ObjectWriterFrame());
		INameScopeDictionary nameScopeDictionary = null;
		if (rootNameScope == null)
		{
			nameScopeDictionary = new NameScope();
		}
		else
		{
			nameScopeDictionary = rootNameScope as INameScopeDictionary;
			if (nameScopeDictionary == null)
			{
				nameScopeDictionary = new NameScopeDictionary(rootNameScope);
			}
		}
		_stack.CurrentFrame.NameScopeDictionary = nameScopeDictionary;
		_stack.PushScope();
		if (settings != null)
		{
			_settings = settings.StripDelegates();
		}
		_runtime = runtime;
		_savedDepth = 0;
	}

	internal Type ServiceProvider_Resolve(string qName)
	{
		XamlType xamlType = ServiceProvider_ResolveXamlType(qName);
		if (xamlType == null || xamlType.UnderlyingType == null)
		{
			XamlTypeName typeName = XamlTypeName.Parse(qName, _serviceProviderContext);
			xamlType = GetXamlType(typeName, returnUnknownTypesOnFailure: true, skipVisibilityCheck: true);
			throw new XamlParseException(SR.Get("TypeNotFound", xamlType.GetQualifiedName()));
		}
		return xamlType.UnderlyingType;
	}

	internal XamlType ServiceProvider_ResolveXamlType(string qName)
	{
		return ResolveXamlType(qName, skipVisibilityCheck: true);
	}

	internal AmbientPropertyValue ServiceProvider_GetFirstAmbientValue(IEnumerable<XamlType> ceilingTypes, XamlMember[] properties)
	{
		List<AmbientPropertyValue> list = FindAmbientValues(ceilingTypes, searchLiveStackOnly: false, null, properties, stopAfterFirst: true);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}

	internal object ServiceProvider_GetFirstAmbientValue(XamlType[] types)
	{
		List<object> list = FindAmbientValues(types, stopAfterFirst: true);
		if (list.Count != 0)
		{
			return list[0];
		}
		return null;
	}

	internal IEnumerable<AmbientPropertyValue> ServiceProvider_GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, XamlMember[] properties)
	{
		return FindAmbientValues(ceilingTypes, searchLiveStackOnly: false, null, properties, stopAfterFirst: false);
	}

	internal IEnumerable<object> ServiceProvider_GetAllAmbientValues(XamlType[] types)
	{
		return FindAmbientValues(types, stopAfterFirst: false);
	}

	internal IEnumerable<AmbientPropertyValue> ServiceProvider_GetAllAmbientValues(IEnumerable<XamlType> ceilingTypes, bool searchLiveStackOnly, IEnumerable<XamlType> types, XamlMember[] properties)
	{
		return FindAmbientValues(ceilingTypes, searchLiveStackOnly, types, properties, stopAfterFirst: false);
	}

	private static void CheckAmbient(XamlMember xamlMember)
	{
		if (!xamlMember.IsAmbient)
		{
			throw new ArgumentException(SR.Get("NotAmbientProperty", xamlMember.DeclaringType.Name, xamlMember.Name), "xamlMember");
		}
	}

	private static void CheckAmbient(XamlType xamlType)
	{
		if (!xamlType.IsAmbient)
		{
			throw new ArgumentException(SR.Get("NotAmbientType", xamlType.Name), "xamlType");
		}
	}

	internal XamlObjectWriterSettings ServiceProvider_GetSettings()
	{
		if (_settings == null)
		{
			_settings = new XamlObjectWriterSettings();
		}
		return _settings;
	}

	public override void AddNamespacePrefix(string prefix, string xamlNS)
	{
		_stack.CurrentFrame.AddNamespace(prefix, xamlNS);
	}

	public override string FindNamespaceByPrefix(string prefix)
	{
		ObjectWriterFrame objectWriterFrame = _stack.CurrentFrame;
		while (objectWriterFrame.Depth > 0)
		{
			if (objectWriterFrame.TryGetNamespaceByPrefix(prefix, out var xamlNs))
			{
				return xamlNs;
			}
			objectWriterFrame = (ObjectWriterFrame)objectWriterFrame.Previous;
		}
		return null;
	}

	public override IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
	{
		ObjectWriterFrame frame = _stack.CurrentFrame;
		Dictionary<string, string> keys = new Dictionary<string, string>();
		while (frame.Depth > 0)
		{
			if (frame._namespaces != null)
			{
				foreach (NamespaceDeclaration namespacePrefix in frame.GetNamespacePrefixes())
				{
					if (!keys.ContainsKey(namespacePrefix.Prefix))
					{
						keys.Add(namespacePrefix.Prefix, null);
						yield return namespacePrefix;
					}
				}
			}
			frame = (ObjectWriterFrame)frame.Previous;
		}
	}

	public XamlException WithLineInfo(XamlException ex)
	{
		ex.SetLineInfo(LineNumber, LinePosition);
		return ex;
	}

	internal XamlType GetDestinationType()
	{
		ObjectWriterFrame objectWriterFrame = _stack.CurrentFrame;
		if (objectWriterFrame == null)
		{
			return null;
		}
		if (objectWriterFrame.Instance != null && objectWriterFrame.XamlType == null)
		{
			objectWriterFrame = objectWriterFrame.Previous as ObjectWriterFrame;
		}
		if (objectWriterFrame.Member == XamlLanguage.Initialization)
		{
			return objectWriterFrame.XamlType;
		}
		return objectWriterFrame.Member.Type;
	}

	private List<AmbientPropertyValue> FindAmbientValues(IEnumerable<XamlType> ceilingTypesEnumerable, bool searchLiveStackOnly, IEnumerable<XamlType> types, XamlMember[] properties, bool stopAfterFirst)
	{
		ArrayHelper.ForAll(properties, CheckAmbient);
		List<XamlType> list = ArrayHelper.ToList(ceilingTypesEnumerable);
		List<AmbientPropertyValue> list2 = new List<AmbientPropertyValue>();
		ObjectWriterFrame objectWriterFrame = _stack.PreviousFrame;
		ObjectWriterFrame objectWriterFrame2 = _stack.CurrentFrame;
		while (objectWriterFrame.Depth >= 1 && (!searchLiveStackOnly || objectWriterFrame.Depth > SavedDepth))
		{
			object instance = objectWriterFrame.Instance;
			if (types != null)
			{
				foreach (XamlType type in types)
				{
					if (objectWriterFrame.XamlType != null && objectWriterFrame.XamlType.CanAssignTo(type) && instance != null)
					{
						AmbientPropertyValue item = new AmbientPropertyValue(null, instance);
						list2.Add(item);
					}
				}
			}
			if (properties != null)
			{
				foreach (XamlMember xamlMember in properties)
				{
					bool flag = false;
					object value = null;
					if (!(objectWriterFrame.XamlType != null) || !objectWriterFrame.XamlType.CanAssignTo(xamlMember.DeclaringType))
					{
						continue;
					}
					if (instance != null)
					{
						if (xamlMember == objectWriterFrame.Member && objectWriterFrame2.Instance != null && objectWriterFrame2.XamlType != null && !objectWriterFrame2.XamlType.IsUsableDuringInitialization)
						{
							if (!typeof(MarkupExtension).IsAssignableFrom(objectWriterFrame2.Instance.GetType()))
							{
								flag = true;
								value = objectWriterFrame2.Instance;
							}
						}
						else if (!(instance is IQueryAmbient queryAmbient) || queryAmbient.IsAmbientPropertyAvailable(xamlMember.Name))
						{
							flag = true;
							value = _runtime.GetValue(instance, xamlMember);
						}
					}
					if (flag)
					{
						AmbientPropertyValue item2 = new AmbientPropertyValue(xamlMember, value);
						list2.Add(item2);
					}
				}
			}
			if ((stopAfterFirst && list2.Count > 0) || (list != null && list.Contains(objectWriterFrame.XamlType)))
			{
				break;
			}
			objectWriterFrame2 = objectWriterFrame;
			objectWriterFrame = (ObjectWriterFrame)objectWriterFrame.Previous;
		}
		return list2;
	}

	private List<object> FindAmbientValues(XamlType[] types, bool stopAfterFirst)
	{
		ArrayHelper.ForAll(types, CheckAmbient);
		List<object> list = new List<object>();
		ObjectWriterFrame objectWriterFrame = _stack.PreviousFrame;
		ObjectWriterFrame currentFrame = _stack.CurrentFrame;
		while (objectWriterFrame.Depth >= 1)
		{
			foreach (XamlType xamlType in types)
			{
				object instance = objectWriterFrame.Instance;
				if (objectWriterFrame.XamlType != null && objectWriterFrame.XamlType.CanAssignTo(xamlType) && instance != null)
				{
					list.Add(instance);
					if (stopAfterFirst)
					{
						return list;
					}
				}
			}
			currentFrame = objectWriterFrame;
			objectWriterFrame = (ObjectWriterFrame)objectWriterFrame.Previous;
		}
		return list;
	}

	public void PushScope()
	{
		_stack.PushScope();
	}

	public void LiftScope()
	{
		_stack.Depth--;
	}

	public void UnLiftScope()
	{
		_stack.Depth++;
	}

	public void PopScope()
	{
		_stack.PopScope();
	}

	private ObjectWriterFrame GetTopFrame()
	{
		if (_stack.Depth == 0)
		{
			return null;
		}
		XamlFrame xamlFrame = _stack.CurrentFrame;
		while (xamlFrame.Depth > 1)
		{
			xamlFrame = xamlFrame.Previous;
		}
		return (ObjectWriterFrame)xamlFrame;
	}

	private INameScopeDictionary LookupNameScopeDictionary(ObjectWriterFrame frame)
	{
		if (frame.NameScopeDictionary == null)
		{
			if (frame.XamlType != null && frame.XamlType.IsNameScope)
			{
				frame.NameScopeDictionary = (frame.Instance as INameScopeDictionary) ?? new NameScopeDictionary(frame.Instance as INameScope);
			}
			if (frame.NameScopeDictionary == null)
			{
				if (frame.Depth == 1)
				{
					frame.NameScopeDictionary = HuntAroundForARootNameScope(frame);
				}
				else if (frame.Depth > 1)
				{
					if (frame.Depth == SavedDepth + 1 && _settings != null && !_settings.RegisterNamesOnExternalNamescope)
					{
						frame.NameScopeDictionary = new NameScope();
					}
					else
					{
						ObjectWriterFrame frame2 = (ObjectWriterFrame)frame.Previous;
						frame.NameScopeDictionary = LookupNameScopeDictionary(frame2);
					}
				}
			}
		}
		return frame.NameScopeDictionary;
	}

	public bool IsOnTheLiveStack(object instance)
	{
		ObjectWriterFrame objectWriterFrame = _stack.CurrentFrame;
		while (objectWriterFrame.Depth > SavedDepth)
		{
			if (instance == objectWriterFrame.Instance)
			{
				return true;
			}
			objectWriterFrame = (ObjectWriterFrame)objectWriterFrame.Previous;
		}
		return false;
	}

	private INameScopeDictionary HuntAroundForARootNameScope(ObjectWriterFrame rootFrame)
	{
		object instance = rootFrame.Instance;
		if (instance == null && rootFrame.XamlType.IsNameScope)
		{
			throw new InvalidOperationException(SR.Get("NameScopeOnRootInstance"));
		}
		INameScopeDictionary nameScopeDictionary = null;
		nameScopeDictionary = instance as INameScopeDictionary;
		if (nameScopeDictionary == null && instance is INameScope underlyingNameScope)
		{
			nameScopeDictionary = new NameScopeDictionary(underlyingNameScope);
		}
		if (nameScopeDictionary == null)
		{
			XamlType xamlType = rootFrame.XamlType;
			if (xamlType.UnderlyingType != null)
			{
				XamlMember xamlMember = TypeReflector.LookupNameScopeProperty(xamlType);
				if (xamlMember != null)
				{
					INameScope nameScope = (INameScope)_runtime.GetValue(instance, xamlMember, failIfWriteOnly: false);
					if (nameScope == null)
					{
						nameScopeDictionary = new NameScope();
						_runtime.SetValue(instance, xamlMember, nameScopeDictionary);
					}
					else
					{
						nameScopeDictionary = nameScope as INameScopeDictionary;
						if (nameScopeDictionary == null)
						{
							nameScopeDictionary = new NameScopeDictionary(nameScope);
						}
					}
				}
			}
		}
		if (nameScopeDictionary == null && _settings != null && _settings.RegisterNamesOnExternalNamescope)
		{
			ObjectWriterFrame objectWriterFrame = (ObjectWriterFrame)rootFrame.Previous;
			nameScopeDictionary = objectWriterFrame.NameScopeDictionary;
		}
		if (nameScopeDictionary == null)
		{
			nameScopeDictionary = new NameScope();
		}
		rootFrame.NameScopeDictionary = nameScopeDictionary;
		return nameScopeDictionary;
	}

	public XamlSavedContext GetSavedContext(SavedContextType savedContextType)
	{
		ObjectWriterFrame topFrame = GetTopFrame();
		if (topFrame.NameScopeDictionary == null)
		{
			topFrame.NameScopeDictionary = LookupNameScopeDictionary(topFrame);
		}
		XamlContextStack<ObjectWriterFrame> stack = new XamlContextStack<ObjectWriterFrame>(_stack, copy: true);
		return new XamlSavedContext(savedContextType, this, stack);
	}

	public object ResolveName(string name, out bool isFullyInitialized)
	{
		isFullyInitialized = false;
		object result = null;
		foreach (INameScopeDictionary stackWalkOfNameScope in StackWalkOfNameScopes)
		{
			object obj = stackWalkOfNameScope.FindName(name);
			if (obj != null)
			{
				if (IsInitializedCallback != null)
				{
					isFullyInitialized = IsInitializedCallback.IsFullyInitialized(obj);
				}
				if ((NameResolutionComplete | isFullyInitialized) || IsInitializedCallback == null)
				{
					result = obj;
				}
				break;
			}
		}
		return result;
	}

	public IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope()
	{
		List<KeyValuePair<string, object>> list = new List<KeyValuePair<string, object>>();
		foreach (INameScopeDictionary stackWalkOfNameScope in StackWalkOfNameScopes)
		{
			foreach (KeyValuePair<string, object> nameValuePair in stackWalkOfNameScope)
			{
				if (!list.Exists((KeyValuePair<string, object> pair) => pair.Key == nameValuePair.Key))
				{
					list.Add(nameValuePair);
				}
			}
		}
		return list;
	}

	internal void AddNameScopeInitializationCompleteSubscriber(EventHandler handler)
	{
		if (_nameScopeInitializationCompleteSubscribers == null)
		{
			_nameScopeInitializationCompleteSubscribers = new List<NameScopeInitializationCompleteSubscriber>();
		}
		NameScopeInitializationCompleteSubscriber nameScopeInitializationCompleteSubscriber = new NameScopeInitializationCompleteSubscriber
		{
			Handler = handler
		};
		nameScopeInitializationCompleteSubscriber.NameScopeDictionaryList.AddRange(StackWalkOfNameScopes);
		_nameScopeInitializationCompleteSubscribers.Add(nameScopeInitializationCompleteSubscriber);
	}

	internal void RemoveNameScopeInitializationCompleteSubscriber(EventHandler handler)
	{
		NameScopeInitializationCompleteSubscriber nameScopeInitializationCompleteSubscriber = _nameScopeInitializationCompleteSubscribers.Find((NameScopeInitializationCompleteSubscriber o) => o.Handler == handler);
		if (nameScopeInitializationCompleteSubscriber != null)
		{
			_nameScopeInitializationCompleteSubscribers.Remove(nameScopeInitializationCompleteSubscriber);
		}
	}

	internal void RaiseNameScopeInitializationCompleteEvent()
	{
		if (_nameScopeInitializationCompleteSubscribers == null)
		{
			return;
		}
		EventArgs e = new EventArgs();
		foreach (NameScopeInitializationCompleteSubscriber nameScopeInitializationCompleteSubscriber in _nameScopeInitializationCompleteSubscribers)
		{
			StackWalkNameResolver sender = new StackWalkNameResolver(nameScopeInitializationCompleteSubscriber.NameScopeDictionaryList);
			nameScopeInitializationCompleteSubscriber.Handler(sender, e);
		}
	}
}
