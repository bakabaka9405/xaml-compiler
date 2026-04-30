using System.ComponentModel;
using System.Globalization;
using System.Xaml.Schema;

namespace System.Xaml;

internal class EventConverter : TypeConverter
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
		if (value is string methodName)
		{
			object rootObject = null;
			Type delegateType = null;
			GetRootObjectAndDelegateType(context, out rootObject, out delegateType);
			if (rootObject != null && delegateType != null)
			{
				return SafeReflectionInvoker.CreateDelegate(delegateType, rootObject, methodName);
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	internal static void GetRootObjectAndDelegateType(ITypeDescriptorContext context, out object rootObject, out Type delegateType)
	{
		rootObject = null;
		delegateType = null;
		if (context != null && context.GetService(typeof(IRootObjectProvider)) is IRootObjectProvider rootObjectProvider)
		{
			rootObject = rootObjectProvider.RootObject;
			if (context.GetService(typeof(IDestinationTypeProvider)) is IDestinationTypeProvider destinationTypeProvider)
			{
				delegateType = destinationTypeProvider.GetDestinationType();
			}
		}
	}
}
