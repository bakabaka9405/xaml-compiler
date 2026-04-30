using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml.MS.Impl;
using System.Xaml.Schema;
using System.Xml.Serialization;

namespace System.Xaml;

public static class XamlLanguage
{
	public const string Xaml2006Namespace = "http://schemas.microsoft.com/winfx/2006/xaml";

	public const string Xml1998Namespace = "http://www.w3.org/XML/1998/namespace";

	internal const string SWMNamespace = "System.Windows.Markup";

	internal const string PreferredPrefix = "x";

	private const string x_AsyncRecords = "AsyncRecords";

	private const string x_Arguments = "Arguments";

	private const string x_Class = "Class";

	private const string x_ClassModifier = "ClassModifier";

	private const string x_Code = "Code";

	private const string x_ConnectionId = "ConnectionId";

	private const string x_FactoryMethod = "FactoryMethod";

	private const string x_FieldModifier = "FieldModifier";

	private const string x_Initialization = "_Initialization";

	private const string x_Items = "_Items";

	private const string x_Key = "Key";

	private const string x_Members = "Members";

	private const string x_ClassAttributes = "ClassAttributes";

	private const string x_Name = "Name";

	private const string x_PositionalParameters = "_PositionalParameters";

	private const string x_Shared = "Shared";

	private const string x_Subclass = "Subclass";

	private const string x_SuppressXamlTrimWarnings = "SuppressXamlTrimWarnings";

	private const string x_SynchronousMode = "SynchronousMode";

	private const string x_TypeArguments = "TypeArguments";

	private const string x_Uid = "Uid";

	private const string x_UnknownContent = "_UnknownContent";

	private const string xml_Space = "space";

	private const string xml_Lang = "lang";

	private const string xml_Base = "base";

	private static ReadOnlyCollection<string> s_xamlNamespaces = new ReadOnlyCollection<string>(new string[1] { "http://schemas.microsoft.com/winfx/2006/xaml" });

	private static ReadOnlyCollection<string> s_xmlNamespaces = new ReadOnlyCollection<string>(new string[1] { "http://www.w3.org/XML/1998/namespace" });

	private static Lazy<XamlSchemaContext> s_schemaContext = new Lazy<XamlSchemaContext>(GetSchemaContext);

	private static Lazy<XamlType> s_array = new Lazy<XamlType>(() => GetXamlType(typeof(ArrayExtension)));

	private static Lazy<XamlType> s_null = new Lazy<XamlType>(() => GetXamlType(typeof(NullExtension)));

	private static Lazy<XamlType> s_reference = new Lazy<XamlType>(() => GetXamlType(typeof(Reference)));

	private static Lazy<XamlType> s_static = new Lazy<XamlType>(() => GetXamlType(typeof(StaticExtension)));

	private static Lazy<XamlType> s_type = new Lazy<XamlType>(() => GetXamlType(typeof(TypeExtension)));

	private static Lazy<XamlType> s_string = new Lazy<XamlType>(() => GetXamlType(typeof(string)));

	private static Lazy<XamlType> s_double = new Lazy<XamlType>(() => GetXamlType(typeof(double)));

	private static Lazy<XamlType> s_int32 = new Lazy<XamlType>(() => GetXamlType(typeof(int)));

	private static Lazy<XamlType> s_boolean = new Lazy<XamlType>(() => GetXamlType(typeof(bool)));

	private static Lazy<XamlType> s_member = new Lazy<XamlType>(() => GetXamlType(typeof(MemberDefinition)));

	private static Lazy<XamlType> s_property = new Lazy<XamlType>(() => GetXamlType(typeof(PropertyDefinition)));

	private static Lazy<XamlType> s_xDataHolder = new Lazy<XamlType>(() => GetXamlType(typeof(XData)));

	private static Lazy<XamlType> s_object = new Lazy<XamlType>(() => GetXamlType(typeof(object)));

	private static Lazy<XamlType> s_listOfObject = new Lazy<XamlType>(() => GetXamlType(typeof(List<object>)));

	private static Lazy<XamlType> s_listOfMembers = new Lazy<XamlType>(() => GetXamlType(typeof(List<MemberDefinition>)));

	private static Lazy<XamlType> s_listOfAttributes = new Lazy<XamlType>(() => GetXamlType(typeof(List<Attribute>)));

	private static Lazy<XamlType> s_markupExtension = new Lazy<XamlType>(() => GetXamlType(typeof(MarkupExtension)));

	private static Lazy<XamlType> s_iNameScope = new Lazy<XamlType>(() => GetXamlType(typeof(INameScope)));

	private static Lazy<XamlType> s_iXmlSerializable = new Lazy<XamlType>(() => GetXamlType(typeof(IXmlSerializable)), isThreadSafe: true);

	private static Lazy<XamlType> s_positionalParameterDescriptor = new Lazy<XamlType>(() => GetXamlType(typeof(PositionalParameterDescriptor)), isThreadSafe: true);

	private static Lazy<XamlType> s_char = new Lazy<XamlType>(() => GetXamlType(typeof(char)), isThreadSafe: true);

	private static Lazy<XamlType> s_single = new Lazy<XamlType>(() => GetXamlType(typeof(float)), isThreadSafe: true);

	private static Lazy<XamlType> s_byte = new Lazy<XamlType>(() => GetXamlType(typeof(byte)), isThreadSafe: true);

	private static Lazy<XamlType> s_int16 = new Lazy<XamlType>(() => GetXamlType(typeof(short)), isThreadSafe: true);

	private static Lazy<XamlType> s_int64 = new Lazy<XamlType>(() => GetXamlType(typeof(long)), isThreadSafe: true);

	private static Lazy<XamlType> s_decimal = new Lazy<XamlType>(() => GetXamlType(typeof(decimal)), isThreadSafe: true);

	private static Lazy<XamlType> s_uri = new Lazy<XamlType>(() => GetXamlType(typeof(Uri)), isThreadSafe: true);

	private static Lazy<XamlType> s_timespan = new Lazy<XamlType>(() => GetXamlType(typeof(TimeSpan)), isThreadSafe: true);

	private static Lazy<ReadOnlyCollection<XamlType>> s_allTypes = new Lazy<ReadOnlyCollection<XamlType>>(GetAllTypes);

	private static Lazy<XamlDirective> s_asyncRecords = new Lazy<XamlDirective>(() => GetXamlDirective("AsyncRecords", String, BuiltInValueConverter.Int32, AllowedMemberLocations.Attribute), isThreadSafe: true);

	private static Lazy<XamlDirective> s_arguments = new Lazy<XamlDirective>(() => GetXamlDirective("Arguments", s_listOfObject.Value, null, AllowedMemberLocations.Any), isThreadSafe: true);

	private static Lazy<XamlDirective> s_class = new Lazy<XamlDirective>(() => GetXamlDirective("Class"));

	private static Lazy<XamlDirective> s_classModifier = new Lazy<XamlDirective>(() => GetXamlDirective("ClassModifier"));

	private static Lazy<XamlDirective> s_code = new Lazy<XamlDirective>(() => GetXamlDirective("Code"));

	private static Lazy<XamlDirective> s_connectionId = new Lazy<XamlDirective>(() => GetXamlDirective("ConnectionId", s_string.Value, BuiltInValueConverter.Int32, AllowedMemberLocations.Any), isThreadSafe: true);

	private static Lazy<XamlDirective> s_factoryMethod = new Lazy<XamlDirective>(() => GetXamlDirective("FactoryMethod", s_string.Value, BuiltInValueConverter.String, AllowedMemberLocations.Any), isThreadSafe: true);

	private static Lazy<XamlDirective> s_fieldModifier = new Lazy<XamlDirective>(() => GetXamlDirective("FieldModifier"));

	private static Lazy<XamlDirective> s_items = new Lazy<XamlDirective>(() => GetXamlDirective("_Items", s_listOfObject.Value, null, AllowedMemberLocations.Any), isThreadSafe: true);

	private static Lazy<XamlDirective> s_initialization = new Lazy<XamlDirective>(() => GetXamlDirective("_Initialization", s_object.Value, null, AllowedMemberLocations.Any), isThreadSafe: true);

	private static Lazy<XamlDirective> s_key = new Lazy<XamlDirective>(() => GetXamlDirective("Key", s_object.Value, BuiltInValueConverter.String, AllowedMemberLocations.Any), isThreadSafe: true);

	private static Lazy<XamlDirective> s_members = new Lazy<XamlDirective>(() => GetXamlDirective("Members", s_listOfMembers.Value, null, AllowedMemberLocations.MemberElement), isThreadSafe: true);

	private static Lazy<XamlDirective> s_classAttributes = new Lazy<XamlDirective>(() => GetXamlDirective("ClassAttributes", s_listOfAttributes.Value, null, AllowedMemberLocations.MemberElement), isThreadSafe: true);

	private static Lazy<XamlDirective> s_name = new Lazy<XamlDirective>(() => GetXamlDirective("Name"));

	private static Lazy<XamlDirective> s_positionalParameters = new Lazy<XamlDirective>(() => GetXamlDirective("_PositionalParameters", s_listOfObject.Value, null, AllowedMemberLocations.Any), isThreadSafe: true);

	private static Lazy<XamlDirective> s_shared = new Lazy<XamlDirective>(() => GetXamlDirective("Shared"), isThreadSafe: true);

	private static Lazy<XamlDirective> s_subclass = new Lazy<XamlDirective>(() => GetXamlDirective("Subclass"), isThreadSafe: true);

	private static Lazy<XamlDirective> s_suppressXamlTrimWarnings = new Lazy<XamlDirective>(() => GetXamlDirective("SuppressXamlTrimWarnings"));

	private static Lazy<XamlDirective> s_synchronousMode = new Lazy<XamlDirective>(() => GetXamlDirective("SynchronousMode"));

	private static Lazy<XamlDirective> s_typeArguments = new Lazy<XamlDirective>(() => GetXamlDirective("TypeArguments"));

	private static Lazy<XamlDirective> s_uid = new Lazy<XamlDirective>(() => GetXamlDirective("Uid"));

	private static Lazy<XamlDirective> s_unknownContent = new Lazy<XamlDirective>(() => GetXamlDirective("_UnknownContent", AllowedMemberLocations.MemberElement, MemberReflector.UnknownReflector), isThreadSafe: true);

	private static Lazy<XamlDirective> s_base = new Lazy<XamlDirective>(() => GetXmlDirective("base"));

	private static Lazy<XamlDirective> s_lang = new Lazy<XamlDirective>(() => GetXmlDirective("lang"));

	private static Lazy<XamlDirective> s_space = new Lazy<XamlDirective>(() => GetXmlDirective("space"));

	private static Lazy<ReadOnlyCollection<XamlDirective>> s_allDirectives = new Lazy<ReadOnlyCollection<XamlDirective>>(GetAllDirectives);

	public static IList<string> XamlNamespaces => s_xamlNamespaces;

	public static IList<string> XmlNamespaces => s_xmlNamespaces;

	public static XamlType Array => s_array.Value;

	public static XamlType Member => s_member.Value;

	public static XamlType Null => s_null.Value;

	public static XamlType Property => s_property.Value;

	public static XamlType Reference => s_reference.Value;

	public static XamlType Static => s_static.Value;

	public static XamlType Type => s_type.Value;

	public static XamlType String => s_string.Value;

	public static XamlType Double => s_double.Value;

	public static XamlType Int32 => s_int32.Value;

	public static XamlType Boolean => s_boolean.Value;

	public static XamlType XData => s_xDataHolder.Value;

	public static XamlType Object => s_object.Value;

	public static XamlType Char => s_char.Value;

	public static XamlType Single => s_single.Value;

	public static XamlType Byte => s_byte.Value;

	public static XamlType Int16 => s_int16.Value;

	public static XamlType Int64 => s_int64.Value;

	public static XamlType Decimal => s_decimal.Value;

	public static XamlType Uri => s_uri.Value;

	public static XamlType TimeSpan => s_timespan.Value;

	public static ReadOnlyCollection<XamlType> AllTypes => s_allTypes.Value;

	public static XamlDirective Arguments => s_arguments.Value;

	public static XamlDirective AsyncRecords => s_asyncRecords.Value;

	public static XamlDirective Class => s_class.Value;

	public static XamlDirective ClassModifier => s_classModifier.Value;

	public static XamlDirective Code => s_code.Value;

	public static XamlDirective ConnectionId => s_connectionId.Value;

	public static XamlDirective FactoryMethod => s_factoryMethod.Value;

	public static XamlDirective FieldModifier => s_fieldModifier.Value;

	public static XamlDirective Items => s_items.Value;

	public static XamlDirective Initialization => s_initialization.Value;

	public static XamlDirective Key => s_key.Value;

	public static XamlDirective Members => s_members.Value;

	public static XamlDirective ClassAttributes => s_classAttributes.Value;

	public static XamlDirective Name => s_name.Value;

	public static XamlDirective PositionalParameters => s_positionalParameters.Value;

	public static XamlDirective Shared => s_shared.Value;

	public static XamlDirective Subclass => s_subclass.Value;

	public static XamlDirective SuppressXamlTrimWarnings => s_suppressXamlTrimWarnings.Value;

	public static XamlDirective SynchronousMode => s_synchronousMode.Value;

	public static XamlDirective TypeArguments => s_typeArguments.Value;

	public static XamlDirective Uid => s_uid.Value;

	public static XamlDirective UnknownContent => s_unknownContent.Value;

	public static XamlDirective Base => s_base.Value;

	public static XamlDirective Lang => s_lang.Value;

	public static XamlDirective Space => s_space.Value;

	public static ReadOnlyCollection<XamlDirective> AllDirectives => s_allDirectives.Value;

	internal static XamlType MarkupExtension => s_markupExtension.Value;

	internal static XamlType INameScope => s_iNameScope.Value;

	internal static XamlType PositionalParameterDescriptor => s_positionalParameterDescriptor.Value;

	internal static XamlType IXmlSerializable => s_iXmlSerializable.Value;

	internal static string TypeAlias(Type type)
	{
		if (type.Equals(typeof(MemberDefinition)))
		{
			return "Member";
		}
		if (type.Equals(typeof(PropertyDefinition)))
		{
			return "Property";
		}
		return null;
	}

	internal static XamlDirective LookupXamlDirective(string name)
	{
		return name switch
		{
			"AsyncRecords" => AsyncRecords, 
			"Arguments" => Arguments, 
			"Class" => Class, 
			"ClassModifier" => ClassModifier, 
			"Code" => Code, 
			"ConnectionId" => ConnectionId, 
			"FactoryMethod" => FactoryMethod, 
			"FieldModifier" => FieldModifier, 
			"_Initialization" => Initialization, 
			"_Items" => Items, 
			"Key" => Key, 
			"Members" => Members, 
			"ClassAttributes" => ClassAttributes, 
			"Name" => Name, 
			"_PositionalParameters" => PositionalParameters, 
			"Shared" => Shared, 
			"Subclass" => Subclass, 
			"SynchronousMode" => SynchronousMode, 
			"TypeArguments" => TypeArguments, 
			"Uid" => Uid, 
			"_UnknownContent" => UnknownContent, 
			_ => null, 
		};
	}

	internal static XamlType LookupXamlType(string typeNamespace, string typeName)
	{
		if (XamlNamespaces.Contains(typeNamespace))
		{
			switch (typeName)
			{
			case "Array":
			case "ArrayExtension":
				return Array;
			case "Member":
				return Member;
			case "Null":
			case "NullExtension":
				return Null;
			case "Property":
				return Property;
			case "Reference":
			case "ReferenceExtension":
				return Reference;
			case "Static":
			case "StaticExtension":
				return Static;
			case "Type":
			case "TypeExtension":
				return Type;
			case "String":
				return String;
			case "Double":
				return Double;
			case "Int16":
				return Int16;
			case "Int32":
				return Int32;
			case "Int64":
				return Int64;
			case "Boolean":
				return Boolean;
			case "XData":
				return XData;
			case "Object":
				return Object;
			case "Char":
				return Char;
			case "Single":
				return Single;
			case "Byte":
				return Byte;
			case "Decimal":
				return Decimal;
			case "Uri":
				return Uri;
			case "TimeSpan":
				return TimeSpan;
			default:
				return null;
			}
		}
		return null;
	}

	internal static Type LookupClrNamespaceType(AssemblyNamespacePair nsPair, string typeName)
	{
		if (nsPair.ClrNamespace == "System.Windows.Markup" && nsPair.Assembly == typeof(XamlLanguage).Assembly)
		{
			if (!(typeName == "Member"))
			{
				if (typeName == "Property")
				{
					return typeof(PropertyDefinition);
				}
				return null;
			}
			return typeof(MemberDefinition);
		}
		return null;
	}

	internal static XamlDirective LookupXmlDirective(string name)
	{
		return name switch
		{
			"base" => Base, 
			"lang" => Lang, 
			"space" => Space, 
			_ => null, 
		};
	}

	private static ReadOnlyCollection<XamlType> GetAllTypes()
	{
		XamlType[] list = new XamlType[21]
		{
			Array, Member, Null, Property, Reference, Static, Type, String, Double, Int16,
			Int32, Int64, Boolean, XData, Object, Char, Single, Byte, Decimal, Uri,
			TimeSpan
		};
		return new ReadOnlyCollection<XamlType>(list);
	}

	private static ReadOnlyCollection<XamlDirective> GetAllDirectives()
	{
		XamlDirective[] list = new XamlDirective[24]
		{
			Arguments, AsyncRecords, Class, Code, ClassModifier, ConnectionId, FactoryMethod, FieldModifier, Key, Initialization,
			Items, Members, ClassAttributes, Name, PositionalParameters, Shared, Subclass, SynchronousMode, TypeArguments, Uid,
			UnknownContent, Base, Lang, Space
		};
		return new ReadOnlyCollection<XamlDirective>(list);
	}

	private static XamlSchemaContext GetSchemaContext()
	{
		Assembly[] referenceAssemblies = new Assembly[2]
		{
			typeof(XamlLanguage).Assembly,
			typeof(MarkupExtension).Assembly
		};
		XamlSchemaContextSettings settings = new XamlSchemaContextSettings
		{
			SupportMarkupExtensionsWithDuplicateArity = true
		};
		return new XamlSchemaContext(referenceAssemblies, settings);
	}

	private static XamlDirective GetXamlDirective(string name)
	{
		return GetXamlDirective(name, String, BuiltInValueConverter.String, AllowedMemberLocations.Attribute);
	}

	private static XamlDirective GetXamlDirective(string name, AllowedMemberLocations allowedLocation, MemberReflector reflector)
	{
		return new XamlDirective(s_xamlNamespaces, name, allowedLocation, reflector);
	}

	private static XamlDirective GetXamlDirective(string name, XamlType xamlType, XamlValueConverter<TypeConverter> typeConverter, AllowedMemberLocations allowedLocation)
	{
		return new XamlDirective(s_xamlNamespaces, name, xamlType, typeConverter, allowedLocation);
	}

	private static XamlDirective GetXmlDirective(string name)
	{
		return new XamlDirective(s_xmlNamespaces, name, String, BuiltInValueConverter.String, AllowedMemberLocations.Attribute);
	}

	private static XamlType GetXamlType(Type type)
	{
		return s_schemaContext.Value.GetXamlType(type);
	}
}
