using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal static class AssemblyFactory
{
	public static Assembly CreateAssembly(MetadataOnlyModule manifestModule, string manifestFile)
	{
		return new MetadataOnlyAssembly(manifestModule, manifestFile);
	}

	public static Assembly CreateAssembly(ITypeUniverse typeUniverse, MetadataFile metadataImport, string manifestFile)
	{
		return CreateAssembly(typeUniverse, metadataImport, new DefaultFactory(), manifestFile);
	}

	public static Assembly CreateAssembly(ITypeUniverse typeUniverse, MetadataFile metadataImport, IReflectionFactory factory, string manifestFile)
	{
		return CreateAssembly(typeUniverse, metadataImport, null, factory, manifestFile, null);
	}

	public static Assembly CreateAssembly(ITypeUniverse typeUniverse, MetadataFile manifestModuleImport, MetadataFile[] netModuleImports, string manifestFile, string[] netModuleFiles)
	{
		return CreateAssembly(typeUniverse, manifestModuleImport, netModuleImports, new DefaultFactory(), manifestFile, netModuleFiles);
	}

	public static Assembly CreateAssembly(ITypeUniverse typeUniverse, MetadataFile manifestModuleImport, MetadataFile[] netModuleImports, IReflectionFactory factory, string manifestFile, string[] netModuleFiles)
	{
		int num = 1;
		if (netModuleImports != null)
		{
			num += netModuleImports.Length;
		}
		MetadataOnlyModule[] array = new MetadataOnlyModule[num];
		MetadataOnlyModule metadataOnlyModule = new MetadataOnlyModule(typeUniverse, manifestModuleImport, factory, manifestFile);
		array[0] = metadataOnlyModule;
		if (num > 1)
		{
			for (int i = 0; i < netModuleImports.Length; i++)
			{
				array[i + 1] = new MetadataOnlyModule(typeUniverse, netModuleImports[i], factory, netModuleFiles[i]);
			}
		}
		return new MetadataOnlyAssembly(array, manifestFile);
	}
}
