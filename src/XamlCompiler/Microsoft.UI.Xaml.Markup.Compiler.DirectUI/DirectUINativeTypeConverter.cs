using System;
using System.ComponentModel;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

internal class DirectUINativeTypeConverter : TypeConverter
{
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(string);
	}
}
