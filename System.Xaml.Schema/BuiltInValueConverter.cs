using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;
using System.Xaml.Replacements;

namespace System.Xaml.Schema;

internal class BuiltInValueConverter<TConverterBase> : XamlValueConverter<TConverterBase> where TConverterBase : class
{
	private Func<TConverterBase> _factory;

	internal override bool IsPublic => true;

	internal BuiltInValueConverter(Type converterType, Func<TConverterBase> factory)
		: base(converterType, (XamlType)null)
	{
		_factory = factory;
	}

	protected override TConverterBase CreateInstance()
	{
		return _factory();
	}
}
internal static class BuiltInValueConverter
{
	private static XamlValueConverter<TypeConverter> s_String;

	private static XamlValueConverter<TypeConverter> s_Object;

	private static XamlValueConverter<TypeConverter> s_Int32;

	private static XamlValueConverter<TypeConverter> s_Int16;

	private static XamlValueConverter<TypeConverter> s_Int64;

	private static XamlValueConverter<TypeConverter> s_UInt32;

	private static XamlValueConverter<TypeConverter> s_UInt16;

	private static XamlValueConverter<TypeConverter> s_UInt64;

	private static XamlValueConverter<TypeConverter> s_Boolean;

	private static XamlValueConverter<TypeConverter> s_Double;

	private static XamlValueConverter<TypeConverter> s_Single;

	private static XamlValueConverter<TypeConverter> s_Byte;

	private static XamlValueConverter<TypeConverter> s_SByte;

	private static XamlValueConverter<TypeConverter> s_Char;

	private static XamlValueConverter<TypeConverter> s_Decimal;

	private static XamlValueConverter<TypeConverter> s_TimeSpan;

	private static XamlValueConverter<TypeConverter> s_Guid;

	private static XamlValueConverter<TypeConverter> s_Type;

	private static XamlValueConverter<TypeConverter> s_TypeList;

	private static XamlValueConverter<TypeConverter> s_DateTime;

	private static XamlValueConverter<TypeConverter> s_DateTimeOffset;

	private static XamlValueConverter<TypeConverter> s_CultureInfo;

	private static XamlValueConverter<ValueSerializer> s_StringSerializer;

	private static XamlValueConverter<TypeConverter> s_Delegate;

	internal static XamlValueConverter<TypeConverter> Int32
	{
		get
		{
			if ((object)s_Int32 == null)
			{
				s_Int32 = new BuiltInValueConverter<TypeConverter>(typeof(Int32Converter), () => new Int32Converter());
			}
			return s_Int32;
		}
	}

	internal static XamlValueConverter<TypeConverter> String
	{
		get
		{
			if ((object)s_String == null)
			{
				s_String = new BuiltInValueConverter<TypeConverter>(typeof(StringConverter), () => new StringConverter());
			}
			return s_String;
		}
	}

	internal static XamlValueConverter<TypeConverter> Object
	{
		get
		{
			if ((object)s_Object == null)
			{
				s_Object = new XamlValueConverter<TypeConverter>(null, XamlLanguage.Object);
			}
			return s_Object;
		}
	}

	internal static XamlValueConverter<TypeConverter> Event
	{
		get
		{
			if ((object)s_Delegate == null)
			{
				s_Delegate = new BuiltInValueConverter<TypeConverter>(typeof(EventConverter), () => new EventConverter());
			}
			return s_Delegate;
		}
	}

	internal static XamlValueConverter<TypeConverter> GetTypeConverter(Type targetType)
	{
		if (typeof(string) == targetType)
		{
			return String;
		}
		if (typeof(object) == targetType)
		{
			return Object;
		}
		if (typeof(int) == targetType)
		{
			return Int32;
		}
		if (typeof(short) == targetType)
		{
			if ((object)s_Int16 == null)
			{
				s_Int16 = new BuiltInValueConverter<TypeConverter>(typeof(Int16Converter), () => new Int16Converter());
			}
			return s_Int16;
		}
		if (typeof(long) == targetType)
		{
			if ((object)s_Int64 == null)
			{
				s_Int64 = new BuiltInValueConverter<TypeConverter>(typeof(Int64Converter), () => new Int64Converter());
			}
			return s_Int64;
		}
		if (typeof(uint) == targetType)
		{
			if ((object)s_UInt32 == null)
			{
				s_UInt32 = new BuiltInValueConverter<TypeConverter>(typeof(UInt32Converter), () => new UInt32Converter());
			}
			return s_UInt32;
		}
		if (typeof(ushort) == targetType)
		{
			if ((object)s_UInt16 == null)
			{
				s_UInt16 = new BuiltInValueConverter<TypeConverter>(typeof(UInt16Converter), () => new UInt16Converter());
			}
			return s_UInt16;
		}
		if (typeof(ulong) == targetType)
		{
			if ((object)s_UInt64 == null)
			{
				s_UInt64 = new BuiltInValueConverter<TypeConverter>(typeof(UInt64Converter), () => new UInt64Converter());
			}
			return s_UInt64;
		}
		if (typeof(bool) == targetType)
		{
			if ((object)s_Boolean == null)
			{
				s_Boolean = new BuiltInValueConverter<TypeConverter>(typeof(BooleanConverter), () => new BooleanConverter());
			}
			return s_Boolean;
		}
		if (typeof(double) == targetType)
		{
			if ((object)s_Double == null)
			{
				s_Double = new BuiltInValueConverter<TypeConverter>(typeof(DoubleConverter), () => new DoubleConverter());
			}
			return s_Double;
		}
		if (typeof(float) == targetType)
		{
			if ((object)s_Single == null)
			{
				s_Single = new BuiltInValueConverter<TypeConverter>(typeof(SingleConverter), () => new SingleConverter());
			}
			return s_Single;
		}
		if (typeof(byte) == targetType)
		{
			if ((object)s_Byte == null)
			{
				s_Byte = new BuiltInValueConverter<TypeConverter>(typeof(ByteConverter), () => new ByteConverter());
			}
			return s_Byte;
		}
		if (typeof(sbyte) == targetType)
		{
			if ((object)s_SByte == null)
			{
				s_SByte = new BuiltInValueConverter<TypeConverter>(typeof(SByteConverter), () => new SByteConverter());
			}
			return s_SByte;
		}
		if (typeof(char) == targetType)
		{
			if ((object)s_Char == null)
			{
				s_Char = new BuiltInValueConverter<TypeConverter>(typeof(CharConverter), () => new CharConverter());
			}
			return s_Char;
		}
		if (typeof(decimal) == targetType)
		{
			if ((object)s_Decimal == null)
			{
				s_Decimal = new BuiltInValueConverter<TypeConverter>(typeof(DecimalConverter), () => new DecimalConverter());
			}
			return s_Decimal;
		}
		if (typeof(TimeSpan) == targetType)
		{
			if ((object)s_TimeSpan == null)
			{
				s_TimeSpan = new BuiltInValueConverter<TypeConverter>(typeof(TimeSpanConverter), () => new TimeSpanConverter());
			}
			return s_TimeSpan;
		}
		if (typeof(Guid) == targetType)
		{
			if ((object)s_Guid == null)
			{
				s_Guid = new BuiltInValueConverter<TypeConverter>(typeof(GuidConverter), () => new GuidConverter());
			}
			return s_Guid;
		}
		if (typeof(Type).IsAssignableFrom(targetType))
		{
			if ((object)s_Type == null)
			{
				s_Type = new BuiltInValueConverter<TypeConverter>(typeof(TypeTypeConverter), () => new TypeTypeConverter());
			}
			return s_Type;
		}
		if (typeof(Type[]).IsAssignableFrom(targetType))
		{
			if ((object)s_TypeList == null)
			{
				s_TypeList = new BuiltInValueConverter<TypeConverter>(typeof(System.Xaml.Replacements.TypeListConverter), () => new System.Xaml.Replacements.TypeListConverter());
			}
			return s_TypeList;
		}
		if (typeof(DateTime) == targetType)
		{
			if ((object)s_DateTime == null)
			{
				s_DateTime = new BuiltInValueConverter<TypeConverter>(typeof(DateTimeConverter2), () => new DateTimeConverter2());
			}
			return s_DateTime;
		}
		if (typeof(DateTimeOffset) == targetType)
		{
			if ((object)s_DateTimeOffset == null)
			{
				s_DateTimeOffset = new BuiltInValueConverter<TypeConverter>(typeof(DateTimeOffsetConverter2), () => new DateTimeOffsetConverter2());
			}
			return s_DateTimeOffset;
		}
		if (typeof(CultureInfo).IsAssignableFrom(targetType))
		{
			if ((object)s_CultureInfo == null)
			{
				s_CultureInfo = new BuiltInValueConverter<TypeConverter>(typeof(CultureInfoConverter), () => new CultureInfoConverter());
			}
			return s_CultureInfo;
		}
		if (typeof(Delegate).IsAssignableFrom(targetType))
		{
			if ((object)s_Delegate == null)
			{
				s_Delegate = new BuiltInValueConverter<TypeConverter>(typeof(EventConverter), () => new EventConverter());
			}
			return s_Delegate;
		}
		return null;
	}

	internal static XamlValueConverter<ValueSerializer> GetValueSerializer(Type targetType)
	{
		if (typeof(string) == targetType)
		{
			if ((object)s_StringSerializer == null)
			{
				ValueSerializer stringSerializer = ValueSerializer.GetSerializerFor(typeof(string));
				s_StringSerializer = new BuiltInValueConverter<ValueSerializer>(stringSerializer.GetType(), () => stringSerializer);
			}
			return s_StringSerializer;
		}
		return null;
	}
}
