using System.Collections.Generic;
using System.Xaml.Schema;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ProxyTypes;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

internal class TypeProxyMetadata
{
	public string Name = string.Empty;

	public XamlCollectionKind CollectionKind;

	public bool ConstructionRequiresArguments;

	public string UnderlyingTypeName;

	public string BaseTypeName;

	public bool IsConstructible = true;

	public bool IsMarkupExtension;

	public bool IsNameScope;

	public bool IsNullable = true;

	public bool IsUnknown;

	public bool IsWhitespaceSignificantCollection;

	public bool IsPublic = true;

	public bool IsXData;

	public bool IsAmbient;

	public Dictionary<string, XamlTypeName> MemberNamesAndMetadata;

	public static TypeProxyMetadata TemplateBindingExtension = new TypeProxyMetadata
	{
		Name = "TemplateBindingExtension",
		IsMarkupExtension = true,
		UnderlyingTypeName = typeof(TemplateBindingExtension).FullName,
		BaseTypeName = "System.Windows.Markup.MarkupExtension"
	};

	public static TypeProxyMetadata StaticResourceExtension = new TypeProxyMetadata
	{
		Name = "StaticResourceExtension",
		IsMarkupExtension = true,
		UnderlyingTypeName = typeof(StaticResourceExtension).FullName,
		BaseTypeName = "System.Windows.Markup.MarkupExtension",
		MemberNamesAndMetadata = new Dictionary<string, XamlTypeName> { 
		{
			"ResourceKey",
			new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "String")
		} }
	};

	public static TypeProxyMetadata ThemeResourceExtension = new TypeProxyMetadata
	{
		Name = "ThemeResourceExtension",
		IsMarkupExtension = true,
		UnderlyingTypeName = typeof(ThemeResourceExtension).FullName,
		BaseTypeName = "System.Windows.Markup.MarkupExtension",
		MemberNamesAndMetadata = new Dictionary<string, XamlTypeName> { 
		{
			"ResourceKey",
			new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "String")
		} }
	};

	public static TypeProxyMetadata NullExtension = new TypeProxyMetadata
	{
		Name = "NullExtension",
		IsMarkupExtension = true,
		UnderlyingTypeName = typeof(NullExtension).FullName,
		BaseTypeName = "System.Windows.Markup.MarkupExtension"
	};

	public static TypeProxyMetadata CustomResourceExtension = new TypeProxyMetadata
	{
		Name = "CustomResourceExtension",
		IsMarkupExtension = true,
		UnderlyingTypeName = typeof(CustomResourceExtension).FullName,
		BaseTypeName = "System.Windows.Markup.MarkupExtension",
		MemberNamesAndMetadata = new Dictionary<string, XamlTypeName> { 
		{
			"ResourceKey",
			new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "String")
		} }
	};

	public static TypeProxyMetadata BindExtension = new TypeProxyMetadata
	{
		Name = "BindExtension",
		IsMarkupExtension = true,
		UnderlyingTypeName = typeof(BindExtension).FullName,
		BaseTypeName = "System.Windows.Markup.MarkupExtension",
		MemberNamesAndMetadata = new Dictionary<string, XamlTypeName>
		{
			{
				"BindBack",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "String")
			},
			{
				"Path",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "String")
			},
			{
				"Mode",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "BindingMode")
			},
			{
				"Converter",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "IValueConverter")
			},
			{
				"ConverterParameter",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "Object")
			},
			{
				"ConverterLanguage",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "String")
			},
			{
				"FallbackValue",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "Object")
			},
			{
				"TargetNullValue",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "Object")
			},
			{
				"UpdateSourceTrigger",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "UpdateSourceTrigger")
			}
		}
	};

	public static TypeProxyMetadata Properties = new TypeProxyMetadata
	{
		Name = "Properties",
		IsMarkupExtension = false,
		UnderlyingTypeName = typeof(Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ProxyTypes.Properties).FullName,
		BaseTypeName = "System.Object",
		CollectionKind = XamlCollectionKind.Collection,
		MemberNamesAndMetadata = new Dictionary<string, XamlTypeName> { 
		{
			"DefaultValue",
			new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "Object")
		} }
	};

	public static TypeProxyMetadata Property = new TypeProxyMetadata
	{
		Name = "Property",
		IsMarkupExtension = false,
		UnderlyingTypeName = typeof(Property).FullName,
		BaseTypeName = "System.Object",
		MemberNamesAndMetadata = new Dictionary<string, XamlTypeName>
		{
			{
				"DefaultValue",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "Object")
			},
			{
				"Name",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "String")
			},
			{
				"Type",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "String")
			},
			{
				"ChangedHandler",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "String")
			},
			{
				"ReadOnly",
				new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "Boolean")
			}
		}
	};

	private TypeProxyMetadata()
	{
	}
}
