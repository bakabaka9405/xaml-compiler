using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Security;

namespace System.Windows.Markup;

internal class DateTimeOffsetConverter2 : TypeConverter
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string) || destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	[SecuritySafeCritical]
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is DateTimeOffset)
		{
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}
			return ((DateTimeOffset)value).ToString("O", culture);
		}
		if (destinationType == typeof(InstanceDescriptor) && value is DateTimeOffset dateTimeOffset)
		{
			Type typeFromHandle = typeof(int);
			ConstructorInfo constructor = typeof(DateTimeOffset).GetConstructor(new Type[8]
			{
				typeFromHandle,
				typeFromHandle,
				typeFromHandle,
				typeFromHandle,
				typeFromHandle,
				typeFromHandle,
				typeFromHandle,
				typeof(TimeSpan)
			});
			if (constructor != null)
			{
				return new InstanceDescriptor(constructor, new object[8] { dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, dateTimeOffset.Hour, dateTimeOffset.Minute, dateTimeOffset.Second, dateTimeOffset.Millisecond, dateTimeOffset.Offset }, isComplete: true);
			}
			return null;
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string)
		{
			string input = ((string)value).Trim();
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}
			return DateTimeOffset.Parse(input, culture, DateTimeStyles.None);
		}
		return base.ConvertFrom(context, culture, value);
	}
}
