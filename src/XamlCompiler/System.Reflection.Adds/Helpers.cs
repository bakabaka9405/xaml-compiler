namespace System.Reflection.Adds;

internal static class Helpers
{
	public static ITypeUniverse Universe(Type type)
	{
		if (type is ITypeProxy typeProxy)
		{
			return typeProxy.TypeUniverse;
		}
		Assembly assembly = type.Assembly;
		if (!(assembly is IAssembly2 assembly2))
		{
			return null;
		}
		return assembly2.TypeUniverse;
	}

	public static Type EnsureResolve(Type type)
	{
		while (type is ITypeProxy typeProxy)
		{
			type = typeProxy.GetResolvedType();
		}
		return type;
	}
}
