using System;
using System.IO;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class Loader
{
	private readonly IMutableTypeUniverse m_universe;

	private readonly MetadataDispenser m_dispenser = new MetadataDispenser();

	private IReflectionFactory m_factory;

	public CorOpenFlags OpenFlags
	{
		get
		{
			return m_dispenser.OpenFlags;
		}
		set
		{
			m_dispenser.OpenFlags = value;
		}
	}

	public IReflectionFactory Factory
	{
		get
		{
			if (m_factory == null)
			{
				m_factory = new DefaultFactory();
			}
			return m_factory;
		}
		set
		{
			m_factory = value;
		}
	}

	public Loader(IMutableTypeUniverse universe)
	{
		m_universe = universe;
	}

	private MetadataFile OpenMetadataFile(string filename)
	{
		return m_dispenser.OpenFileAsFileMapping(filename);
	}

	public Assembly LoadAssemblyFromFile(string file)
	{
		Assembly assembly = ReadAssemblyFromFile(file);
		m_universe.AddAssembly(assembly);
		return assembly;
	}

	public Assembly ReadAssemblyFromFile(string file)
	{
		MetadataFile metadataFile = OpenMetadataFile(file);
		return AssemblyFactory.CreateAssembly(m_universe, metadataFile, Factory, metadataFile.FilePath);
	}

	public Assembly LoadAssemblyFromFile(string manifestFile, string[] netModuleFiles)
	{
		MetadataFile metadataFile = OpenMetadataFile(manifestFile);
		MetadataFile[] array = null;
		if (netModuleFiles != null && netModuleFiles.Length != 0)
		{
			array = new MetadataFile[netModuleFiles.Length];
			for (int i = 0; i < netModuleFiles.Length; i++)
			{
				array[i] = OpenMetadataFile(netModuleFiles[i]);
			}
		}
		Assembly assembly = AssemblyFactory.CreateAssembly(m_universe, metadataFile, array, Factory, metadataFile.FilePath, netModuleFiles);
		m_universe.AddAssembly(assembly);
		return assembly;
	}

	public Assembly LoadAssemblyFromByteArray(byte[] data)
	{
		MetadataFile metadataImport = m_dispenser.OpenFromByteArray(data);
		Assembly assembly = AssemblyFactory.CreateAssembly(m_universe, metadataImport, Factory, string.Empty);
		m_universe.AddAssembly(assembly);
		return assembly;
	}

	public MetadataOnlyModule LoadModuleFromFile(string moduleFileName)
	{
		MetadataFile import = m_dispenser.OpenFileAsFileMapping(moduleFileName);
		return new MetadataOnlyModule(m_universe, import, Factory, moduleFileName);
	}

	public Module ResolveModule(Assembly containingAssembly, string moduleName)
	{
		if (containingAssembly == null || string.IsNullOrEmpty(containingAssembly.Location))
		{
			throw new ArgumentException("manifestModule needs to be associated with an assembly with valid location");
		}
		string directoryName = Path.GetDirectoryName(containingAssembly.Location);
		string text = Path.Combine(directoryName, moduleName);
		MetadataFile import = m_dispenser.OpenFileAsFileMapping(text);
		MetadataOnlyModule metadataOnlyModule = new MetadataOnlyModule(m_universe, import, Factory, text);
		metadataOnlyModule.SetContainingAssembly(containingAssembly);
		return metadataOnlyModule;
	}
}
