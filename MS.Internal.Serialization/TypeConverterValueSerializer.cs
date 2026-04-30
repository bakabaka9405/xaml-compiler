using System.ComponentModel;
using System.Windows.Markup;
using System.Xaml;

namespace MS.Internal.Serialization;

internal sealed class TypeConverterValueSerializer : ValueSerializer
{
	private TypeConverter converter;

	public TypeConverterValueSerializer(TypeConverter converter)
	{
		this.converter = converter;
	}

	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		return converter.CanConvertTo(context, typeof(string));
	}

	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		return converter.ConvertToString(context, TypeConverterHelper.InvariantEnglishUS, value);
	}

	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		return converter.ConvertFrom(context, TypeConverterHelper.InvariantEnglishUS, value);
	}
}
