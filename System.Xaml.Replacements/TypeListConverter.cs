using System.ComponentModel;
using System.Globalization;

namespace System.Xaml.Replacements;

internal class TypeListConverter : TypeConverter
{
	private static readonly TypeTypeConverter typeTypeConverter = new TypeTypeConverter();

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(string);
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		string typeList = (string)value;
		if (context != null)
		{
			string[] array = StringHelpers.SplitTypeList(typeList);
			Type[] array2 = new Type[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = (Type)typeTypeConverter.ConvertFrom(context, TypeConverterHelper.InvariantEnglishUS, array[i]);
			}
			return array2;
		}
		return base.ConvertFrom(context, culture, value);
	}
}
