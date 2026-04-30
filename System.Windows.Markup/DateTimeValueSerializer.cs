using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xaml;

namespace System.Windows.Markup;

[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public class DateTimeValueSerializer : ValueSerializer
{
	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		if (!(value is DateTime))
		{
			return false;
		}
		return true;
	}

	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		if (value == null)
		{
			throw GetConvertFromException(value);
		}
		if (value.Length == 0)
		{
			return DateTime.MinValue;
		}
		DateTimeFormatInfo dateTimeFormatInfo = (DateTimeFormatInfo)TypeConverterHelper.InvariantEnglishUS.GetFormat(typeof(DateTimeFormatInfo));
		DateTimeStyles styles = DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.RoundtripKind;
		if (dateTimeFormatInfo != null)
		{
			return DateTime.Parse(value, dateTimeFormatInfo, styles);
		}
		return DateTime.Parse(value, TypeConverterHelper.InvariantEnglishUS, styles);
	}

	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		if (value == null || !(value is DateTime dateTime))
		{
			throw GetConvertToException(value, typeof(string));
		}
		StringBuilder stringBuilder = new StringBuilder("yyyy-MM-dd");
		if (dateTime.TimeOfDay.TotalSeconds == 0.0)
		{
			if (dateTime.Kind != DateTimeKind.Unspecified)
			{
				stringBuilder.Append("'T'HH':'mm");
			}
		}
		else
		{
			long num = dateTime.Ticks % 10000000;
			int second = dateTime.Second;
			stringBuilder.Append("'T'HH':'mm");
			if (second != 0 || num != 0L)
			{
				stringBuilder.Append("':'ss");
				if (num != 0L)
				{
					stringBuilder.Append("'.'FFFFFFF");
				}
			}
		}
		stringBuilder.Append("K");
		return dateTime.ToString(stringBuilder.ToString(), TypeConverterHelper.InvariantEnglishUS);
	}
}
