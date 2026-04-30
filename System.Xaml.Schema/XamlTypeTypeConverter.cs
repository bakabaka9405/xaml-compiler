using System.ComponentModel;
using System.Globalization;

namespace System.Xaml.Schema;

public class XamlTypeTypeConverter : TypeConverter
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
			XamlType xamlType = ConvertStringToXamlType(context, text);
			if (xamlType != null)
			{
				return xamlType;
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
		XamlType xamlType = value as XamlType;
		if (context != null && xamlType != null && destinationType == typeof(string))
		{
			string text = ConvertXamlTypeToString(context, xamlType);
			if (text != null)
			{
				return text;
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	internal static string ConvertXamlTypeToString(ITypeDescriptorContext context, XamlType xamlType)
	{
		INamespacePrefixLookup service = GetService<INamespacePrefixLookup>(context);
		if (service == null)
		{
			return null;
		}
		XamlTypeName xamlTypeName = new XamlTypeName(xamlType);
		return xamlTypeName.ToString(service);
	}

	private static XamlType ConvertStringToXamlType(ITypeDescriptorContext context, string typeName)
	{
		IXamlNamespaceResolver service = GetService<IXamlNamespaceResolver>(context);
		if (service == null)
		{
			return null;
		}
		XamlTypeName typeName2 = XamlTypeName.Parse(typeName, service);
		IXamlSchemaContextProvider service2 = GetService<IXamlSchemaContextProvider>(context);
		if (service2 == null)
		{
			return null;
		}
		if (service2.SchemaContext == null)
		{
			return null;
		}
		return GetXamlTypeOrUnknown(service2.SchemaContext, typeName2);
	}

	private static TService GetService<TService>(ITypeDescriptorContext context) where TService : class
	{
		return context.GetService(typeof(TService)) as TService;
	}

	private static XamlType GetXamlTypeOrUnknown(XamlSchemaContext schemaContext, XamlTypeName typeName)
	{
		XamlType xamlType = schemaContext.GetXamlType(typeName);
		if (xamlType != null)
		{
			return xamlType;
		}
		XamlType[] array = null;
		if (typeName.HasTypeArgs)
		{
			array = new XamlType[typeName.TypeArguments.Count];
			for (int i = 0; i < typeName.TypeArguments.Count; i++)
			{
				array[i] = GetXamlTypeOrUnknown(schemaContext, typeName.TypeArguments[i]);
			}
		}
		return new XamlType(typeName.Namespace, typeName.Name, array, schemaContext);
	}
}
