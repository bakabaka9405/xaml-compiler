using System.Globalization;

namespace System.Reflection.Adds;

internal static class ElementTypeUtility
{
	public static string GetNameForPrimitive(CorElementType value)
	{
		return value switch
		{
			CorElementType.Object => "System.Object", 
			CorElementType.Void => "System.Void", 
			CorElementType.Int => "System.Int32", 
			CorElementType.UInt => "System.UInt32", 
			CorElementType.UShort => "System.UInt16", 
			CorElementType.ULong => "System.UInt64", 
			CorElementType.Char => "System.Char", 
			CorElementType.Byte => "System.Byte", 
			CorElementType.SByte => "System.SByte", 
			CorElementType.Short => "System.Int16", 
			CorElementType.Long => "System.Int64", 
			CorElementType.Float => "System.Single", 
			CorElementType.Double => "System.Double", 
			CorElementType.Bool => "System.Boolean", 
			CorElementType.IntPtr => "System.IntPtr", 
			CorElementType.UIntPtr => "System.UIntPtr", 
			CorElementType.String => "System.String", 
			CorElementType.Type => "System.Type", 
			_ => throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.IllegalElementType, value)), 
		};
	}
}
