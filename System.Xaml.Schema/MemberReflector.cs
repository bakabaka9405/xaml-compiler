using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Security;
using System.Windows.Markup;

namespace System.Xaml.Schema;

internal class MemberReflector : Reflector
{
	private const DesignerSerializationVisibility VisibilityInvalid = (DesignerSerializationVisibility)2147483647;

	private const DesignerSerializationVisibility VisibilityNone = (DesignerSerializationVisibility)2147483646;

	private static MemberReflector s_UnknownReflector;

	private NullableReference<string> _constructorArgument;

	private NullableReference<XamlValueConverter<XamlDeferringLoader>> _deferringLoader;

	private NullableReference<object> _defaultValue;

	[SecurityCritical]
	private NullableReference<MethodInfo> _getter;

	[SecurityCritical]
	private NullableReference<MethodInfo> _setter;

	private NullableReference<XamlValueConverter<TypeConverter>> _typeConverter;

	private NullableReference<XamlValueConverter<ValueSerializer>> _valueSerializer;

	private DesignerSerializationVisibility _designerSerializationVisibility;

	private int _memberBits;

	internal static MemberReflector UnknownReflector
	{
		[SecuritySafeCritical]
		get
		{
			if (s_UnknownReflector == null)
			{
				s_UnknownReflector = new MemberReflector();
				s_UnknownReflector._designerSerializationVisibility = DesignerSerializationVisibility.Visible;
				s_UnknownReflector._memberBits = -65432;
				s_UnknownReflector._deferringLoader.Value = null;
				s_UnknownReflector._getter.Value = null;
				s_UnknownReflector._setter.Value = null;
				s_UnknownReflector._typeConverter.Value = null;
				s_UnknownReflector._valueSerializer.Value = null;
				s_UnknownReflector.DependsOn = XamlType.EmptyList<XamlMember>.Value;
				s_UnknownReflector.Invoker = XamlMemberInvoker.UnknownInvoker;
				s_UnknownReflector.Type = XamlLanguage.Object;
			}
			return s_UnknownReflector;
		}
	}

	internal string ConstructorArgument
	{
		get
		{
			return _constructorArgument.Value;
		}
		set
		{
			_constructorArgument.Value = value;
		}
	}

	internal bool ConstructorArgumentIsSet => _constructorArgument.IsSet;

	internal IReadOnlyDictionary<char, char> MarkupExtensionBracketCharactersArgument { get; set; }

	internal bool MarkupExtensionBracketCharactersArgumentIsSet { get; set; }

	internal object DefaultValue
	{
		get
		{
			if (!_defaultValue.IsNotPresent)
			{
				return _defaultValue.Value;
			}
			return null;
		}
		set
		{
			_defaultValue.Value = value;
		}
	}

	internal bool DefaultValueIsNotPresent
	{
		get
		{
			return _defaultValue.IsNotPresent;
		}
		set
		{
			_defaultValue.IsNotPresent = value;
		}
	}

	internal bool DefaultValueIsSet => _defaultValue.IsSet;

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

	internal IList<XamlMember> DependsOn { get; set; }

	internal DesignerSerializationVisibility? SerializationVisibility
	{
		get
		{
			if (_designerSerializationVisibility == (DesignerSerializationVisibility)2147483646)
			{
				return null;
			}
			return _designerSerializationVisibility;
		}
		set
		{
			_designerSerializationVisibility = value ?? ((DesignerSerializationVisibility)2147483646);
		}
	}

	internal bool DesignerSerializationVisibilityIsSet => _designerSerializationVisibility != (DesignerSerializationVisibility)2147483647;

	internal MethodInfo Getter
	{
		[SecuritySafeCritical]
		get
		{
			return _getter.Value;
		}
		[SecuritySafeCritical]
		set
		{
			_getter.SetIfNull(value);
		}
	}

	internal bool GetterIsSet
	{
		[SecuritySafeCritical]
		get
		{
			return _getter.IsSet;
		}
	}

	internal XamlMemberInvoker Invoker { get; set; }

	internal bool IsUnknown => (_memberBits & 8) != 0;

	internal MethodInfo Setter
	{
		[SecuritySafeCritical]
		get
		{
			return _setter.Value;
		}
		[SecuritySafeCritical]
		set
		{
			_setter.SetIfNull(value);
		}
	}

	internal bool SetterIsSet
	{
		[SecuritySafeCritical]
		get
		{
			return _setter.IsSet;
		}
	}

	internal XamlType Type { get; set; }

	internal XamlType TargetType { get; set; }

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

	internal MemberInfo UnderlyingMember { get; set; }

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

	protected override MemberInfo Member => UnderlyingMember;

	internal MemberReflector()
	{
		_designerSerializationVisibility = (DesignerSerializationVisibility)2147483647;
	}

	internal MemberReflector(bool isEvent)
		: this()
	{
		if (isEvent)
		{
			_memberBits = 4;
		}
		_memberBits |= Reflector.GetValidMask(4);
	}

	[SecuritySafeCritical]
	internal MemberReflector(MethodInfo getter, MethodInfo setter, bool isEvent)
		: this(isEvent)
	{
		_getter.Value = getter;
		_setter.Value = setter;
	}

	[SecuritySafeCritical]
	internal MemberReflector(XamlType type, XamlValueConverter<TypeConverter> typeConverter)
	{
		Type = type;
		_typeConverter.Value = typeConverter;
		_designerSerializationVisibility = DesignerSerializationVisibility.Visible;
		_memberBits = -65440;
		_deferringLoader.Value = null;
		_getter.Value = null;
		_setter.Value = null;
		_valueSerializer.Value = null;
	}

	internal bool? GetFlag(BoolMemberBits flag)
	{
		return Reflector.GetFlag(_memberBits, (int)flag);
	}

	internal void SetFlag(BoolMemberBits flag, bool value)
	{
		Reflector.SetFlag(ref _memberBits, (int)flag, value);
	}

	internal static bool IsInternalVisibleTo(MethodInfo method, Assembly accessingAssembly, XamlSchemaContext schemaContext)
	{
		if (accessingAssembly == null)
		{
			return false;
		}
		if (method.IsAssembly || method.IsFamilyOrAssembly)
		{
			if (TypeReflector.IsInternal(method.DeclaringType))
			{
				return true;
			}
			return schemaContext.AreInternalsVisibleTo(method.DeclaringType.Assembly, accessingAssembly);
		}
		return false;
	}

	internal static bool IsProtectedVisibleTo(MethodInfo method, Type derivedType, XamlSchemaContext schemaContext)
	{
		if (derivedType == null)
		{
			return false;
		}
		if (!derivedType.Equals(method.DeclaringType) && !derivedType.IsSubclassOf(method.DeclaringType))
		{
			return false;
		}
		if (method.IsFamily || method.IsFamilyOrAssembly)
		{
			return true;
		}
		if (method.IsFamilyAndAssembly)
		{
			if (TypeReflector.IsInternal(method.DeclaringType))
			{
				return true;
			}
			return schemaContext.AreInternalsVisibleTo(method.DeclaringType.Assembly, derivedType.Assembly);
		}
		return false;
	}

	internal static bool GenericArgumentsAreVisibleTo(MethodInfo method, Assembly accessingAssembly, XamlSchemaContext schemaContext)
	{
		if (method.IsGenericMethod)
		{
			Type[] genericArguments = method.GetGenericArguments();
			foreach (Type type in genericArguments)
			{
				if (!TypeReflector.IsVisibleTo(type, accessingAssembly, schemaContext))
				{
					return false;
				}
			}
		}
		return true;
	}
}
