using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Security;
using System.Xaml;

namespace System.Windows.Markup;

internal class TypeExtensionConverter : TypeConverter
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	[SecuritySafeCritical]
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			if (!(value is TypeExtension typeExtension))
			{
				throw new ArgumentException(SR.Get("MustBeOfType", "value", "TypeExtension"));
			}
			return new InstanceDescriptor(typeof(TypeExtension).GetConstructor(new Type[1] { typeof(Type) }), new object[1] { typeExtension.Type });
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
