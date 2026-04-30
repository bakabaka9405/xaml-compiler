using System.Windows.Markup;

namespace MS.Internal.Serialization;

internal sealed class StringValueSerializer : ValueSerializer
{
	public override bool CanConvertToString(object value, IValueSerializerContext context)
	{
		return true;
	}

	public override bool CanConvertFromString(string value, IValueSerializerContext context)
	{
		return true;
	}

	public override object ConvertFromString(string value, IValueSerializerContext context)
	{
		return value;
	}

	public override string ConvertToString(object value, IValueSerializerContext context)
	{
		return (string)value;
	}
}
