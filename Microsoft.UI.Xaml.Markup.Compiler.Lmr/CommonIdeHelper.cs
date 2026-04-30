using System;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal static class CommonIdeHelper
{
	private class EmptyUniverse : ITypeUniverse
	{
		public Type GetBuiltInType(CorElementType elementType)
		{
			throw new NotImplementedException();
		}

		public Type GetTypeXFromName(string fullName)
		{
			throw new NotImplementedException();
		}

		public Assembly GetSystemAssembly()
		{
			throw new NotImplementedException();
		}

		public Assembly GetSystemRuntimeAssembly()
		{
			throw new NotImplementedException();
		}

		public Assembly ResolveAssembly(AssemblyName name)
		{
			throw new NotImplementedException();
		}

		public Assembly ResolveAssembly(AssemblyName name, bool throwOnError)
		{
			throw new NotImplementedException();
		}

		public Assembly ResolveAssembly(Module scope, Token tokenAssemblyRef)
		{
			throw new NotImplementedException();
		}

		public bool WouldResolveToAssembly(AssemblyName name, Assembly assembly)
		{
			throw new NotImplementedException();
		}

		public Module ResolveModule(Assembly containingAssembly, string moduleName)
		{
			throw new NotImplementedException();
		}

		public Type ResolveWindowsRuntimeType(string typeName, bool throwOnError, bool ignoreCase)
		{
			throw new NotImplementedException();
		}
	}

	public static AssemblyName GetNameFromPath(string path)
	{
		EmptyUniverse typeUniverse = new EmptyUniverse();
		MetadataFile metadataImport = new MetadataDispenser().OpenFile(path);
		Assembly assembly = AssemblyFactory.CreateAssembly(typeUniverse, metadataImport, path);
		return assembly.GetName();
	}
}
