using System;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

internal class DirectUIXamlLanguage : IDirectUIXamlLanguage
{
	private DirectUISchemaContext schema;

	private Lazy<XamlType> objectXamlType;

	private Lazy<XamlType> stringXamlType;

	private Lazy<XamlType> doubleXamlType;

	private Lazy<XamlType> int32XamlType;

	private Lazy<XamlType> booleanXamlType;

	private Lazy<XamlType> uiElementXamlType;

	private Lazy<XamlType> nullExtensionXamlType;

	private Lazy<XamlType> staticResourceExtensionXamlType;

	private Lazy<XamlType> customResourceExtensionXamlType;

	private Lazy<XamlType> bindExtensionXamlType;

	private Lazy<XamlType> propertiesXamlType;

	private Lazy<XamlType> propertyXamlType;

	public XamlType Object => objectXamlType.Value;

	public XamlType String => stringXamlType.Value;

	public XamlType Double => doubleXamlType.Value;

	public XamlType Int32 => int32XamlType.Value;

	public XamlType Boolean => booleanXamlType.Value;

	public XamlType NullExtension => nullExtensionXamlType.Value;

	public XamlType StaticResourceExtension => staticResourceExtensionXamlType.Value;

	public XamlType CustomResourceExtension => customResourceExtensionXamlType.Value;

	public XamlType BindExtension => bindExtensionXamlType.Value;

	public XamlType UIElement => uiElementXamlType.Value;

	public XamlType Properties => propertiesXamlType.Value;

	public XamlType Property => propertyXamlType.Value;

	public bool IsStringNullable { get; }

	public DirectUIXamlLanguage(DirectUISchemaContext schema, bool isStringNullable)
	{
		this.schema = schema;
		objectXamlType = new Lazy<XamlType>(() => GetDirectUIXamlType(this.schema.DirectUISystem.Object));
		stringXamlType = new Lazy<XamlType>(() => GetDirectUIXamlType(this.schema.DirectUISystem.String));
		doubleXamlType = new Lazy<XamlType>(() => GetDirectUIXamlType(this.schema.DirectUISystem.Double));
		int32XamlType = new Lazy<XamlType>(() => GetDirectUIXamlType(this.schema.DirectUISystem.Int32));
		booleanXamlType = new Lazy<XamlType>(() => GetDirectUIXamlType(this.schema.DirectUISystem.Boolean));
		uiElementXamlType = new Lazy<XamlType>(() => GetDirectUIXamlType(this.schema.DirectUISystem.UIElement));
		nullExtensionXamlType = new Lazy<XamlType>(() => GetDirectUIProxyXamlType(TypeProxyMetadata.NullExtension.Name));
		staticResourceExtensionXamlType = new Lazy<XamlType>(() => GetDirectUIProxyXamlType(TypeProxyMetadata.StaticResourceExtension.Name));
		customResourceExtensionXamlType = new Lazy<XamlType>(() => GetDirectUIProxyXamlType(TypeProxyMetadata.CustomResourceExtension.Name));
		bindExtensionXamlType = new Lazy<XamlType>(() => GetDirectUIProxyXamlType(TypeProxyMetadata.BindExtension.Name));
		propertiesXamlType = new Lazy<XamlType>(() => GetDirectUIProxyXamlType(TypeProxyMetadata.Properties.Name));
		propertyXamlType = new Lazy<XamlType>(() => GetDirectUIProxyXamlType(TypeProxyMetadata.Property.Name));
		IsStringNullable = isStringNullable;
	}

	public XamlType LookupXamlObjects(string name)
	{
		switch (name)
		{
		case "Null":
		case "NullExtension":
			return NullExtension;
		case "String":
			return String;
		case "Double":
			return Double;
		case "Int32":
			return Int32;
		case "Boolean":
			return Boolean;
		case "Bind":
			return BindExtension;
		case "Object":
			return Object;
		case "Properties":
			return Properties;
		case "Property":
			return Property;
		default:
			return null;
		}
	}

	private XamlType GetDirectUIXamlType(Type type)
	{
		return schema.GetXamlType(type);
	}

	private XamlType GetDirectUIProxyXamlType(string name)
	{
		return schema.GetProxyType("http://schemas.microsoft.com/winfx/2006/xaml/presentation", name);
	}
}
