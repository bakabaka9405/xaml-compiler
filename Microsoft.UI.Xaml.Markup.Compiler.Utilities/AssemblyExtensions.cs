using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler.Utilities;

internal static class AssemblyExtensions
{
	public static bool IsClsCompliant(this Assembly instance)
	{
		foreach (CustomAttributeData item in from a in instance.GetCustomAttributesData()
			where a.AttributeType.FullName == typeof(CLSCompliantAttribute).FullName && a.ConstructorArguments.Any()
			select a)
		{
			CustomAttributeTypedArgument customAttributeTypedArgument = item.ConstructorArguments[0];
			if (customAttributeTypedArgument.ArgumentType.FullName == typeof(bool).FullName)
			{
				return (bool)customAttributeTypedArgument.Value;
			}
		}
		return false;
	}
}
