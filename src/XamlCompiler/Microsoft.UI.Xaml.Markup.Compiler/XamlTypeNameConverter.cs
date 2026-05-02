using System;
using System.ComponentModel;
using System.Globalization;
using System.Xaml;
using System.Xaml.Schema;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlTypeNameConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string typeName && context.GetService(typeof(IXamlNamespaceResolver)) is IXamlNamespaceResolver namespaceResolver)
		{
			return XamlTypeName.Parse(typeName, namespaceResolver);
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (!(value is XamlTypeName xamlTypeName) || destinationType != typeof(string))
		{
			throw new InvalidOperationException("Bad argument to XamlTypeNameConverter");
		}
		if (context.GetService(typeof(INamespacePrefixLookup)) is INamespacePrefixLookup namespacePrefixLookup)
		{
			return xamlTypeName.ToString(namespacePrefixLookup);
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
