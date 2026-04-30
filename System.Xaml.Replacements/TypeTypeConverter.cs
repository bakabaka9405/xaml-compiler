using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Xaml.Schema;

namespace System.Xaml.Replacements;

internal class TypeTypeConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(string);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		string text = value as string;
		if (context != null && text != null)
		{
			IXamlTypeResolver service = GetService<IXamlTypeResolver>(context);
			if (service != null)
			{
				return service.Resolve(text);
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType == typeof(string);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		Type type = value as Type;
		if (context != null && type != null && destinationType == typeof(string))
		{
			string text = ConvertTypeToString(context, type);
			if (text != null)
			{
				return text;
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	private static string ConvertTypeToString(ITypeDescriptorContext context, Type type)
	{
		IXamlSchemaContextProvider service = GetService<IXamlSchemaContextProvider>(context);
		if (service == null)
		{
			return null;
		}
		if (service.SchemaContext == null)
		{
			return null;
		}
		XamlType xamlType = service.SchemaContext.GetXamlType(type);
		if (xamlType == null)
		{
			return null;
		}
		return XamlTypeTypeConverter.ConvertXamlTypeToString(context, xamlType);
	}

	private static TService GetService<TService>(ITypeDescriptorContext context) where TService : class
	{
		return context.GetService(typeof(TService)) as TService;
	}
}
