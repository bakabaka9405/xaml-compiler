namespace System.Reflection.Adds;

internal interface ITypeUniverse
{
	Type GetBuiltInType(CorElementType elementType);

	Type GetTypeXFromName(string fullName);

	Assembly GetSystemAssembly();

	Assembly GetSystemRuntimeAssembly();

	Assembly ResolveAssembly(AssemblyName name);

	Assembly ResolveAssembly(AssemblyName name, bool throwOnError);

	Assembly ResolveAssembly(Module scope, Token tokenAssemblyRef);

	bool WouldResolveToAssembly(AssemblyName name, Assembly assembly);

	Module ResolveModule(Assembly containingAssembly, string moduleName);

	Type ResolveWindowsRuntimeType(string typeName, bool throwOnError, bool ignoreCase);
}
