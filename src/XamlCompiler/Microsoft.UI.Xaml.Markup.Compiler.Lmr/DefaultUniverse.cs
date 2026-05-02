using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class DefaultUniverse : SimpleUniverse
{
	private Loader m_loader;

	public Loader Loader => m_loader;

	public DefaultUniverse()
	{
		m_loader = new Loader(this);
	}

	public override Module ResolveModule(Assembly containingAssembly, string moduleName)
	{
		return Loader.ResolveModule(containingAssembly, moduleName);
	}

	internal Assembly LoadAssemblyFromFile(string manifestFileName, string[] netModuleFileNames)
	{
		return Loader.LoadAssemblyFromFile(manifestFileName, netModuleFileNames);
	}

	internal Assembly LoadAssemblyFromFile(string manifestFileName)
	{
		return Loader.LoadAssemblyFromFile(manifestFileName);
	}

	internal MetadataOnlyModule LoadModuleFromFile(string netModulePath)
	{
		return Loader.LoadModuleFromFile(netModulePath);
	}

	internal Assembly LoadAssemblyFromByteArray(byte[] data)
	{
		return Loader.LoadAssemblyFromByteArray(data);
	}
}
