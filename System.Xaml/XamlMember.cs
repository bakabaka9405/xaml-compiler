using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Windows.Markup;
using System.Xaml.Schema;
using MS.Internal.Xaml.Parser;

namespace System.Xaml;

public class XamlMember : IEquatable<XamlMember>
{
	private enum MemberType : byte
	{
		Instance,
		Attachable,
		Directive
	}

	private string _name;

	private XamlType _declaringType;

	private MemberType _memberType;

	private ThreeValuedBool _isNameValid;

	private MemberReflector _reflector;

	[SecurityCritical]
	private NullableReference<MemberInfo> _underlyingMember;

	public XamlType DeclaringType => _declaringType;

	public XamlMemberInvoker Invoker
	{
		get
		{
			EnsureReflector();
			if (_reflector.Invoker == null)
			{
				_reflector.Invoker = LookupInvoker() ?? XamlMemberInvoker.UnknownInvoker;
			}
			return _reflector.Invoker;
		}
	}

	public bool IsUnknown
	{
		get
		{
			EnsureReflector();
			return _reflector.IsUnknown;
		}
	}

	public bool IsReadPublic
	{
		get
		{
			if (IsReadPublicIgnoringType)
			{
				if (!(_declaringType == null))
				{
					return _declaringType.IsPublic;
				}
				return true;
			}
			return false;
		}
	}

	public bool IsWritePublic
	{
		get
		{
			if (IsWritePublicIgnoringType)
			{
				if (!(_declaringType == null))
				{
					return _declaringType.IsPublic;
				}
				return true;
			}
			return false;
		}
	}

	public string Name => _name;

	public bool IsNameValid
	{
		get
		{
			if (_isNameValid == ThreeValuedBool.NotSet)
			{
				_isNameValid = ((!XamlName.IsValidXamlName(_name)) ? ThreeValuedBool.False : ThreeValuedBool.True);
			}
			return _isNameValid == ThreeValuedBool.True;
		}
	}

	public string PreferredXamlNamespace
	{
		get
		{
			IList<string> xamlNamespaces = GetXamlNamespaces();
			if (xamlNamespaces.Count > 0)
			{
				return xamlNamespaces[0];
			}
			return null;
		}
	}

	public XamlType TargetType
	{
		get
		{
			if (!IsAttachable)
			{
				return _declaringType;
			}
			EnsureReflector();
			if (_reflector.TargetType == null)
			{
				if (_reflector.IsUnknown)
				{
					return XamlLanguage.Object;
				}
				_reflector.TargetType = LookupTargetType() ?? XamlLanguage.Object;
			}
			return _reflector.TargetType;
		}
	}

	public XamlType Type
	{
		get
		{
			EnsureReflector();
			if (_reflector.Type == null)
			{
				_reflector.Type = LookupType() ?? XamlLanguage.Object;
			}
			return _reflector.Type;
		}
	}

	public XamlValueConverter<TypeConverter> TypeConverter
	{
		get
		{
			EnsureReflector();
			if (!_reflector.TypeConverterIsSet)
			{
				_reflector.TypeConverter = LookupTypeConverter();
			}
			return _reflector.TypeConverter;
		}
	}

	public XamlValueConverter<ValueSerializer> ValueSerializer
	{
		get
		{
			EnsureReflector();
			if (!_reflector.ValueSerializerIsSet)
			{
				_reflector.ValueSerializer = LookupValueSerializer();
			}
			return _reflector.ValueSerializer;
		}
	}

	public XamlValueConverter<XamlDeferringLoader> DeferringLoader
	{
		get
		{
			EnsureReflector();
			if (!_reflector.DeferringLoaderIsSet)
			{
				_reflector.DeferringLoader = LookupDeferringLoader();
			}
			return _reflector.DeferringLoader;
		}
	}

	public MemberInfo UnderlyingMember
	{
		[SecuritySafeCritical]
		get
		{
			if (!_underlyingMember.IsSet)
			{
				_underlyingMember.SetIfNull(LookupUnderlyingMember());
			}
			return _underlyingMember.Value;
		}
	}

	internal NullableReference<MemberInfo> UnderlyingMemberInternal
	{
		[SecuritySafeCritical]
		get
		{
			return _underlyingMember;
		}
	}

	public bool IsReadOnly => GetFlag(BoolMemberBits.ReadOnly);

	public bool IsWriteOnly => GetFlag(BoolMemberBits.WriteOnly);

	public bool IsAttachable => _memberType == MemberType.Attachable;

	public bool IsEvent => GetFlag(BoolMemberBits.Event);

	public bool IsDirective => _memberType == MemberType.Directive;

	public IList<XamlMember> DependsOn
	{
		get
		{
			EnsureReflector();
			if (_reflector.DependsOn == null)
			{
				_reflector.DependsOn = LookupDependsOn() ?? XamlType.EmptyList<XamlMember>.Value;
			}
			return _reflector.DependsOn;
		}
	}

	public bool IsAmbient => GetFlag(BoolMemberBits.Ambient);

	public DesignerSerializationVisibility SerializationVisibility
	{
		get
		{
			EnsureReflector();
			if (!_reflector.DesignerSerializationVisibilityIsSet)
			{
				_reflector.SerializationVisibility = LookupSerializationVisibility();
			}
			return _reflector.SerializationVisibility ?? DesignerSerializationVisibility.Visible;
		}
	}

	public IReadOnlyDictionary<char, char> MarkupExtensionBracketCharacters
	{
		get
		{
			EnsureReflector();
			if (!_reflector.MarkupExtensionBracketCharactersArgumentIsSet)
			{
				_reflector.MarkupExtensionBracketCharactersArgument = LookupMarkupExtensionBracketCharacters();
				_reflector.MarkupExtensionBracketCharactersArgumentIsSet = true;
			}
			return _reflector.MarkupExtensionBracketCharactersArgument;
		}
	}

	internal string ConstructorArgument
	{
		get
		{
			EnsureReflector();
			if (!_reflector.ConstructorArgumentIsSet)
			{
				_reflector.ConstructorArgument = LookupConstructorArgument();
			}
			return _reflector.ConstructorArgument;
		}
	}

	internal object DefaultValue
	{
		get
		{
			EnsureDefaultValue();
			return _reflector.DefaultValue;
		}
	}

	internal MethodInfo Getter
	{
		get
		{
			EnsureReflector();
			if (!_reflector.GetterIsSet)
			{
				_reflector.Getter = LookupUnderlyingGetter();
			}
			return _reflector.Getter;
		}
	}

	internal bool HasDefaultValue
	{
		get
		{
			EnsureDefaultValue();
			return !_reflector.DefaultValueIsNotPresent;
		}
	}

	internal bool HasSerializationVisibility
	{
		get
		{
			EnsureReflector();
			if (!_reflector.DesignerSerializationVisibilityIsSet)
			{
				_reflector.SerializationVisibility = LookupSerializationVisibility();
			}
			return _reflector.SerializationVisibility.HasValue;
		}
	}

	internal MethodInfo Setter
	{
		get
		{
			EnsureReflector();
			if (!_reflector.SetterIsSet)
			{
				_reflector.Setter = LookupUnderlyingSetter();
			}
			return _reflector.Setter;
		}
	}

	private bool IsReadPublicIgnoringType
	{
		get
		{
			EnsureReflector();
			bool? flag = _reflector.GetFlag(BoolMemberBits.ReadPublic);
			if (!flag.HasValue)
			{
				flag = LookupIsReadPublic();
				_reflector.SetFlag(BoolMemberBits.ReadPublic, flag.Value);
			}
			return flag.Value;
		}
	}

	private bool IsWritePublicIgnoringType
	{
		get
		{
			EnsureReflector();
			bool? flag = _reflector.GetFlag(BoolMemberBits.WritePublic);
			if (!flag.HasValue)
			{
				flag = LookupIsWritePublic();
				_reflector.SetFlag(BoolMemberBits.WritePublic, flag.Value);
			}
			return flag.Value;
		}
	}

	private bool AreAttributesAvailable
	{
		get
		{
			EnsureReflector();
			if (!_reflector.CustomAttributeProviderIsSetVolatile)
			{
				ICustomAttributeProvider customAttributeProvider = LookupCustomAttributeProvider();
				if (customAttributeProvider == null)
				{
					_reflector.UnderlyingMember = UnderlyingMember;
				}
				_reflector.SetCustomAttributeProviderVolatile(customAttributeProvider);
			}
			if (_reflector.CustomAttributeProvider == null)
			{
				return UnderlyingMemberInternal.Value != null;
			}
			return true;
		}
	}

	private XamlSchemaContext SchemaContext => _declaringType.SchemaContext;

	public XamlMember(string name, XamlType declaringType, bool isAttachable)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (declaringType == null)
		{
			throw new ArgumentNullException("declaringType");
		}
		_name = name;
		_declaringType = declaringType;
		_memberType = (isAttachable ? MemberType.Attachable : MemberType.Instance);
	}

	public XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext)
		: this(propertyInfo, schemaContext, null)
	{
	}

	public XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
		: this(propertyInfo, schemaContext, invoker, new MemberReflector(isEvent: false))
	{
	}

	[SecuritySafeCritical]
	internal XamlMember(PropertyInfo propertyInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
	{
		if (propertyInfo == null)
		{
			throw new ArgumentNullException("propertyInfo");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		_name = propertyInfo.Name;
		_declaringType = schemaContext.GetXamlType(propertyInfo.DeclaringType);
		_memberType = MemberType.Instance;
		_reflector = reflector;
		_reflector.Invoker = invoker;
		_underlyingMember.Value = propertyInfo;
	}

	public XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext)
		: this(eventInfo, schemaContext, null)
	{
	}

	public XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
		: this(eventInfo, schemaContext, invoker, new MemberReflector(isEvent: true))
	{
	}

	[SecuritySafeCritical]
	internal XamlMember(EventInfo eventInfo, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
	{
		if (eventInfo == null)
		{
			throw new ArgumentNullException("eventInfo");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		_name = eventInfo.Name;
		_declaringType = schemaContext.GetXamlType(eventInfo.DeclaringType);
		_memberType = MemberType.Instance;
		_reflector = reflector;
		_reflector.Invoker = invoker;
		_underlyingMember.Value = eventInfo;
	}

	public XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext)
		: this(attachablePropertyName, getter, setter, schemaContext, null)
	{
	}

	public XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
		: this(attachablePropertyName, getter, setter, schemaContext, invoker, new MemberReflector(getter, setter, isEvent: false))
	{
	}

	[SecuritySafeCritical]
	internal XamlMember(string attachablePropertyName, MethodInfo getter, MethodInfo setter, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
	{
		if (attachablePropertyName == null)
		{
			throw new ArgumentNullException("attachablePropertyName");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		MethodInfo methodInfo = getter ?? setter;
		if (methodInfo == null)
		{
			throw new ArgumentNullException(SR.Get("GetterOrSetterRequired"), (Exception)null);
		}
		ValidateGetter(getter, "getter");
		ValidateSetter(setter, "setter");
		_name = attachablePropertyName;
		_declaringType = schemaContext.GetXamlType(methodInfo.DeclaringType);
		_reflector = reflector;
		_memberType = MemberType.Attachable;
		_reflector.Invoker = invoker;
		_underlyingMember.Value = getter ?? setter;
	}

	public XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext)
		: this(attachableEventName, adder, schemaContext, null)
	{
	}

	public XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext, XamlMemberInvoker invoker)
		: this(attachableEventName, adder, schemaContext, invoker, new MemberReflector(null, adder, isEvent: true))
	{
	}

	[SecuritySafeCritical]
	internal XamlMember(string attachableEventName, MethodInfo adder, XamlSchemaContext schemaContext, XamlMemberInvoker invoker, MemberReflector reflector)
	{
		if (attachableEventName == null)
		{
			throw new ArgumentNullException("attachableEventName");
		}
		if (adder == null)
		{
			throw new ArgumentNullException("adder");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		ValidateSetter(adder, "adder");
		_name = attachableEventName;
		_declaringType = schemaContext.GetXamlType(adder.DeclaringType);
		_reflector = reflector;
		_memberType = MemberType.Attachable;
		_reflector.Invoker = invoker;
		_underlyingMember.Value = adder;
	}

	internal XamlMember(string name, MemberReflector reflector)
	{
		_name = name;
		_declaringType = null;
		_reflector = reflector ?? MemberReflector.UnknownReflector;
		_memberType = MemberType.Directive;
	}

	public virtual IList<string> GetXamlNamespaces()
	{
		return DeclaringType.GetXamlNamespaces();
	}

	public override string ToString()
	{
		return _declaringType.ToString() + "." + Name;
	}

	internal bool IsReadVisibleTo(Assembly accessingAssembly, Type accessingType)
	{
		if (IsReadPublicIgnoringType)
		{
			return true;
		}
		MethodInfo getter = Getter;
		if (getter != null)
		{
			if (MemberReflector.GenericArgumentsAreVisibleTo(getter, accessingAssembly, SchemaContext))
			{
				if (!MemberReflector.IsInternalVisibleTo(getter, accessingAssembly, SchemaContext))
				{
					return MemberReflector.IsProtectedVisibleTo(getter, accessingType, SchemaContext);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	internal bool IsWriteVisibleTo(Assembly accessingAssembly, Type accessingType)
	{
		if (IsWritePublicIgnoringType)
		{
			return true;
		}
		MethodInfo setter = Setter;
		if (setter != null)
		{
			if (MemberReflector.GenericArgumentsAreVisibleTo(setter, accessingAssembly, SchemaContext))
			{
				if (!MemberReflector.IsInternalVisibleTo(setter, accessingAssembly, SchemaContext))
				{
					return MemberReflector.IsProtectedVisibleTo(setter, accessingType, SchemaContext);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	protected virtual XamlMemberInvoker LookupInvoker()
	{
		if (UnderlyingMember != null)
		{
			return new XamlMemberInvoker(this);
		}
		return null;
	}

	protected virtual ICustomAttributeProvider LookupCustomAttributeProvider()
	{
		return null;
	}

	protected virtual XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
	{
		if (AreAttributesAvailable)
		{
			Type[] attributeTypes = _reflector.GetAttributeTypes(typeof(XamlDeferLoadAttribute), 2);
			if (attributeTypes != null)
			{
				return SchemaContext.GetValueConverter<XamlDeferringLoader>(attributeTypes[0], null);
			}
		}
		if (Type != null)
		{
			return Type.DeferringLoader;
		}
		return null;
	}

	protected virtual IList<XamlMember> LookupDependsOn()
	{
		if (!AreAttributesAvailable)
		{
			return null;
		}
		List<string> allAttributeContents = _reflector.GetAllAttributeContents<string>(typeof(DependsOnAttribute));
		if (allAttributeContents == null || allAttributeContents.Count == 0)
		{
			return null;
		}
		List<XamlMember> list = new List<XamlMember>();
		foreach (string item in allAttributeContents)
		{
			XamlMember member = _declaringType.GetMember(item);
			if (member != null)
			{
				list.Add(member);
			}
		}
		return XamlType.GetReadOnly(list);
	}

	private DesignerSerializationVisibility? LookupSerializationVisibility()
	{
		DesignerSerializationVisibility? result = null;
		if (AreAttributesAvailable)
		{
			return _reflector.GetAttributeValue<DesignerSerializationVisibility>(typeof(DesignerSerializationVisibilityAttribute));
		}
		return result;
	}

	protected virtual bool LookupIsAmbient()
	{
		if (AreAttributesAvailable)
		{
			return _reflector.IsAttributePresent(typeof(AmbientAttribute));
		}
		return GetDefaultFlag(BoolMemberBits.Ambient);
	}

	protected virtual bool LookupIsEvent()
	{
		return UnderlyingMember is EventInfo;
	}

	protected virtual bool LookupIsReadPublic()
	{
		MethodInfo getter = Getter;
		if (getter != null && !getter.IsPublic)
		{
			return false;
		}
		return !IsWriteOnly;
	}

	protected virtual bool LookupIsReadOnly()
	{
		if (UnderlyingMember != null)
		{
			return Setter == null;
		}
		return GetDefaultFlag(BoolMemberBits.ReadOnly);
	}

	protected virtual bool LookupIsUnknown()
	{
		if (_reflector != null)
		{
			return _reflector.IsUnknown;
		}
		return UnderlyingMember == null;
	}

	protected virtual bool LookupIsWriteOnly()
	{
		if (UnderlyingMember != null)
		{
			return Getter == null;
		}
		return GetDefaultFlag(BoolMemberBits.WriteOnly);
	}

	protected virtual bool LookupIsWritePublic()
	{
		MethodInfo setter = Setter;
		if (setter != null && !setter.IsPublic)
		{
			return false;
		}
		return !IsReadOnly;
	}

	protected virtual XamlType LookupTargetType()
	{
		if (IsAttachable)
		{
			MethodInfo methodInfo = UnderlyingMember as MethodInfo;
			if (methodInfo != null)
			{
				ParameterInfo[] parameters = methodInfo.GetParameters();
				if (parameters.Length != 0)
				{
					Type parameterType = parameters[0].ParameterType;
					return SchemaContext.GetXamlType(parameterType);
				}
			}
			return XamlLanguage.Object;
		}
		return _declaringType;
	}

	protected virtual XamlValueConverter<TypeConverter> LookupTypeConverter()
	{
		XamlValueConverter<TypeConverter> xamlValueConverter = null;
		if (AreAttributesAvailable)
		{
			Type attributeType = _reflector.GetAttributeType(typeof(TypeConverterAttribute));
			if (attributeType != null)
			{
				xamlValueConverter = SchemaContext.GetValueConverter<TypeConverter>(attributeType, null);
			}
		}
		if (xamlValueConverter == null && Type != null)
		{
			xamlValueConverter = Type.TypeConverter;
		}
		return xamlValueConverter;
	}

	protected virtual XamlValueConverter<ValueSerializer> LookupValueSerializer()
	{
		XamlValueConverter<ValueSerializer> xamlValueConverter = null;
		if (AreAttributesAvailable)
		{
			Type attributeType = _reflector.GetAttributeType(typeof(ValueSerializerAttribute));
			if (attributeType != null)
			{
				xamlValueConverter = SchemaContext.GetValueConverter<ValueSerializer>(attributeType, null);
			}
		}
		if (xamlValueConverter == null && Type != null)
		{
			xamlValueConverter = Type.ValueSerializer;
		}
		return xamlValueConverter;
	}

	protected virtual IReadOnlyDictionary<char, char> LookupMarkupExtensionBracketCharacters()
	{
		if (AreAttributesAvailable)
		{
			IReadOnlyDictionary<char, char> bracketCharacterAttributes = _reflector.GetBracketCharacterAttributes(typeof(MarkupExtensionBracketCharactersAttribute));
			if (bracketCharacterAttributes != null)
			{
				_reflector.MarkupExtensionBracketCharactersArgument = bracketCharacterAttributes;
			}
		}
		return _reflector.MarkupExtensionBracketCharactersArgument;
	}

	protected virtual XamlType LookupType()
	{
		Type type = LookupSystemType();
		if (!(type != null))
		{
			return null;
		}
		return SchemaContext.GetXamlType(type);
	}

	protected virtual MethodInfo LookupUnderlyingGetter()
	{
		EnsureReflector();
		if (_reflector.Getter != null)
		{
			return _reflector.Getter;
		}
		PropertyInfo propertyInfo = UnderlyingMember as PropertyInfo;
		if (!(propertyInfo != null))
		{
			return null;
		}
		return propertyInfo.GetGetMethod(nonPublic: true);
	}

	protected virtual MethodInfo LookupUnderlyingSetter()
	{
		EnsureReflector();
		if (_reflector.Setter != null)
		{
			return _reflector.Setter;
		}
		PropertyInfo propertyInfo = UnderlyingMember as PropertyInfo;
		if (propertyInfo != null)
		{
			return propertyInfo.GetSetMethod(nonPublic: true);
		}
		EventInfo eventInfo = UnderlyingMember as EventInfo;
		if (!(eventInfo != null))
		{
			return null;
		}
		return eventInfo.GetAddMethod(nonPublic: true);
	}

	protected virtual MemberInfo LookupUnderlyingMember()
	{
		return UnderlyingMemberInternal.Value;
	}

	private static void ValidateGetter(MethodInfo method, string argumentName)
	{
		if (method == null || (method.GetParameters().Length == 1 && !(method.ReturnType == typeof(void))))
		{
			return;
		}
		throw new ArgumentException(SR.Get("IncorrectGetterParamNum"), argumentName);
	}

	private static void ValidateSetter(MethodInfo method, string argumentName)
	{
		if (method != null && method.GetParameters().Length != 2)
		{
			throw new ArgumentException(SR.Get("IncorrectSetterParamNum"), argumentName);
		}
	}

	private static bool GetDefaultFlag(BoolMemberBits flagBit)
	{
		return (BoolMemberBits.Default & flagBit) == flagBit;
	}

	private void CreateReflector()
	{
		MemberReflector value = (LookupIsUnknown() ? MemberReflector.UnknownReflector : new MemberReflector());
		Interlocked.CompareExchange(ref _reflector, value, null);
	}

	private void EnsureDefaultValue()
	{
		EnsureReflector();
		if (_reflector.DefaultValueIsSet)
		{
			return;
		}
		DefaultValueAttribute defaultValueAttribute = null;
		if (AreAttributesAvailable)
		{
			ICustomAttributeProvider customAttributeProvider = _reflector.CustomAttributeProvider ?? UnderlyingMember;
			object[] customAttributes = customAttributeProvider.GetCustomAttributes(typeof(DefaultValueAttribute), inherit: true);
			if (customAttributes.Length != 0)
			{
				defaultValueAttribute = (DefaultValueAttribute)customAttributes[0];
			}
		}
		if (defaultValueAttribute != null)
		{
			_reflector.DefaultValue = defaultValueAttribute.Value;
		}
		else
		{
			_reflector.DefaultValueIsNotPresent = true;
		}
	}

	private void EnsureReflector()
	{
		if (_reflector == null)
		{
			CreateReflector();
		}
	}

	private bool GetFlag(BoolMemberBits flagBit)
	{
		EnsureReflector();
		bool? flag = _reflector.GetFlag(flagBit);
		if (!flag.HasValue)
		{
			flag = LookupBooleanValue(flagBit);
			_reflector.SetFlag(flagBit, flag.Value);
		}
		return flag.Value;
	}

	private bool LookupBooleanValue(BoolMemberBits flag)
	{
		return flag switch
		{
			BoolMemberBits.Ambient => LookupIsAmbient(), 
			BoolMemberBits.Event => LookupIsEvent(), 
			BoolMemberBits.ReadOnly => LookupIsReadOnly(), 
			BoolMemberBits.ReadPublic => LookupIsReadPublic(), 
			BoolMemberBits.WriteOnly => LookupIsWriteOnly(), 
			BoolMemberBits.WritePublic => LookupIsWritePublic(), 
			_ => GetDefaultFlag(flag), 
		};
	}

	private string LookupConstructorArgument()
	{
		string result = null;
		if (AreAttributesAvailable)
		{
			result = _reflector.GetAttributeString(typeof(ConstructorArgumentAttribute), out var _);
		}
		return result;
	}

	private Type LookupSystemType()
	{
		MemberInfo underlyingMember = UnderlyingMember;
		PropertyInfo propertyInfo = underlyingMember as PropertyInfo;
		if (propertyInfo != null)
		{
			return propertyInfo.PropertyType;
		}
		EventInfo eventInfo = underlyingMember as EventInfo;
		if (eventInfo != null)
		{
			return eventInfo.EventHandlerType;
		}
		MethodInfo methodInfo = underlyingMember as MethodInfo;
		if (methodInfo != null)
		{
			if (methodInfo.ReturnType != null && methodInfo.ReturnType != typeof(void))
			{
				return methodInfo.ReturnType;
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			if (parameters.Length == 2)
			{
				return parameters[1].ParameterType;
			}
		}
		return null;
	}

	public override bool Equals(object obj)
	{
		XamlMember xamlMember = obj as XamlMember;
		return this == xamlMember;
	}

	public override int GetHashCode()
	{
		return (int)((uint)((Name != null) ? Name.GetHashCode() : 0) ^ (uint)_memberType) ^ DeclaringType.GetHashCode();
	}

	public bool Equals(XamlMember other)
	{
		return this == other;
	}

	public static bool operator ==(XamlMember xamlMember1, XamlMember xamlMember2)
	{
		if ((object)xamlMember1 == xamlMember2)
		{
			return true;
		}
		if ((object)xamlMember1 == null || (object)xamlMember2 == null)
		{
			return false;
		}
		if (xamlMember1._memberType != xamlMember2._memberType || xamlMember1.Name != xamlMember2.Name)
		{
			return false;
		}
		if (xamlMember1.IsDirective)
		{
			return XamlDirective.NamespacesAreEqual((XamlDirective)xamlMember1, (XamlDirective)xamlMember2);
		}
		if (xamlMember1.DeclaringType == xamlMember2.DeclaringType)
		{
			return xamlMember1.IsUnknown == xamlMember2.IsUnknown;
		}
		return false;
	}

	public static bool operator !=(XamlMember xamlMember1, XamlMember xamlMember2)
	{
		return !(xamlMember1 == xamlMember2);
	}
}
