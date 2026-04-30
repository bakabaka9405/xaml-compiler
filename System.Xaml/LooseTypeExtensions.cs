using System.Reflection;
using System.Windows.Markup;

namespace System.Xaml;

internal static class LooseTypeExtensions
{
	private const string WindowsBase = "WindowsBase";

	private static readonly byte[] WindowsBaseToken = new byte[8] { 49, 191, 56, 86, 173, 54, 78, 53 };

	internal static bool AssemblyQualifiedNameEquals(Type t1, Type t2)
	{
		if ((object)t1 == null)
		{
			return (object)t2 == null;
		}
		if ((object)t2 == null)
		{
			return false;
		}
		if (t1.FullName != t2.FullName)
		{
			return false;
		}
		if (t1.Assembly.FullName == t2.Assembly.FullName)
		{
			return true;
		}
		AssemblyName assemblyName = new AssemblyName(t1.Assembly.FullName);
		AssemblyName assemblyName2 = new AssemblyName(t2.Assembly.FullName);
		if (assemblyName.Name == assemblyName2.Name)
		{
			if (assemblyName.CultureInfo.Equals(assemblyName2.CultureInfo))
			{
				return SafeSecurityHelper.IsSameKeyToken(assemblyName.GetPublicKeyToken(), assemblyName2.GetPublicKeyToken());
			}
			return false;
		}
		return IsWindowsBaseToSystemXamlComparison(t1.Assembly, t2.Assembly, assemblyName, assemblyName2);
	}

	private static bool IsWindowsBaseToSystemXamlComparison(Assembly a1, Assembly a2, AssemblyName name1, AssemblyName name2)
	{
		AssemblyName assemblyName = null;
		if (name1.Name == "WindowsBase" && a2 == typeof(MarkupExtension).Assembly)
		{
			assemblyName = name1;
		}
		else if (name2.Name == "WindowsBase" && a1 == typeof(MarkupExtension).Assembly)
		{
			assemblyName = name2;
		}
		if (assemblyName != null)
		{
			return SafeSecurityHelper.IsSameKeyToken(assemblyName.GetPublicKeyToken(), WindowsBaseToken);
		}
		return false;
	}

	internal static bool IsAssemblyQualifiedNameAssignableFrom(Type t1, Type t2)
	{
		if (t1 == null || t2 == null)
		{
			return false;
		}
		if (AssemblyQualifiedNameEquals(t1, t2))
		{
			return true;
		}
		if (IsLooseSubClassOf(t2, t1))
		{
			return true;
		}
		if (t1.IsInterface)
		{
			return LooselyImplementInterface(t2, t1);
		}
		if (!t1.IsGenericParameter)
		{
			return false;
		}
		Type[] genericParameterConstraints = t1.GetGenericParameterConstraints();
		for (int i = 0; i < genericParameterConstraints.Length; i++)
		{
			if (!IsAssemblyQualifiedNameAssignableFrom(genericParameterConstraints[i], t2))
			{
				return false;
			}
		}
		return true;
	}

	private static bool LooselyImplementInterface(Type t, Type interfaceType)
	{
		Type type = t;
		while (type != null)
		{
			Type[] interfaces = type.GetInterfaces();
			for (int i = 0; i < interfaces.Length; i++)
			{
				if (AssemblyQualifiedNameEquals(interfaces[i], interfaceType) || LooselyImplementInterface(interfaces[i], interfaceType))
				{
					return true;
				}
			}
			type = type.BaseType;
		}
		return false;
	}

	private static bool IsLooseSubClassOf(Type t1, Type t2)
	{
		if (t1 == null || t2 == null)
		{
			return false;
		}
		if (AssemblyQualifiedNameEquals(t1, t2))
		{
			return false;
		}
		Type baseType = t1.BaseType;
		while (baseType != null)
		{
			if (AssemblyQualifiedNameEquals(baseType, t2))
			{
				return true;
			}
			baseType = baseType.BaseType;
		}
		return false;
	}
}
