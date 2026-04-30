using System.ComponentModel;
using System.Globalization;
using System.Xaml;

namespace System.Windows.Markup;

public class NameReferenceConverter : TypeConverter
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
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		IXamlNameResolver xamlNameResolver = (IXamlNameResolver)context.GetService(typeof(IXamlNameResolver));
		if (xamlNameResolver == null)
		{
			throw new InvalidOperationException(SR.Get("MissingNameResolver"));
		}
		string text = value as string;
		if (string.IsNullOrEmpty(text))
		{
			throw new InvalidOperationException(SR.Get("MustHaveName"));
		}
		object obj = xamlNameResolver.Resolve(text);
		if (obj == null)
		{
			string[] names = new string[1] { text };
			obj = xamlNameResolver.GetFixupToken(names, canAssignDirectly: true);
		}
		return obj;
	}

	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (context == null || !(context.GetService(typeof(IXamlNameProvider)) is IXamlNameProvider))
		{
			return false;
		}
		if (destinationType == typeof(string))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		IXamlNameProvider xamlNameProvider = (IXamlNameProvider)context.GetService(typeof(IXamlNameProvider));
		if (xamlNameProvider == null)
		{
			throw new InvalidOperationException(SR.Get("MissingNameProvider"));
		}
		return xamlNameProvider.GetName(value);
	}
}
