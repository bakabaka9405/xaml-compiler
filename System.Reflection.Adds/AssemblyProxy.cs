using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace System.Reflection.Adds;

[DebuggerDisplay("AssemblyProxy")]
internal abstract class AssemblyProxy : Assembly, IAssembly2, IDisposable
{
	private readonly ITypeUniverse m_universe;

	private Assembly m_assembly;

	public ITypeUniverse TypeUniverse => m_universe;

	public override string FullName => GetResolvedAssembly().FullName;

	public override string Location => GetResolvedAssembly().Location;

	public override bool ReflectionOnly => true;

	public override string CodeBase => GetResolvedAssembly().CodeBase;

	public override string EscapedCodeBase => GetResolvedAssembly().EscapedCodeBase;

	public override MethodInfo EntryPoint => GetResolvedAssembly().EntryPoint;

	public override Module ManifestModule => GetResolvedAssembly().ManifestModule;

	public override bool GlobalAssemblyCache => GetResolvedAssembly().GlobalAssemblyCache;

	public override long HostContext => GetResolvedAssembly().HostContext;

	protected AssemblyProxy(ITypeUniverse universe)
	{
		m_universe = universe;
	}

	public Assembly GetResolvedAssembly()
	{
		if (m_assembly == null)
		{
			m_assembly = GetResolvedAssemblyWorker();
			if (m_assembly == null)
			{
				throw new UnresolvedAssemblyException(string.Format(CultureInfo.InvariantCulture, Resources.UniverseCannotResolveAssembly, GetNameWithNoResolution()));
			}
		}
		return m_assembly;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing && m_assembly != null && m_assembly is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	public override int GetHashCode()
	{
		return GetResolvedAssembly().GetHashCode();
	}

	public override string ToString()
	{
		return GetResolvedAssembly().ToString();
	}

	public override bool Equals(object obj)
	{
		return GetResolvedAssembly().Equals(obj);
	}

	protected abstract Assembly GetResolvedAssemblyWorker();

	protected abstract AssemblyName GetNameWithNoResolution();

	public override AssemblyName GetName()
	{
		return GetResolvedAssembly().GetName();
	}

	public override AssemblyName GetName(bool copiedName)
	{
		return GetResolvedAssembly().GetName(copiedName);
	}

	public override Type[] GetExportedTypes()
	{
		return GetResolvedAssembly().GetExportedTypes();
	}

	public override Type[] GetTypes()
	{
		return GetResolvedAssembly().GetTypes();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return GetResolvedAssembly().GetCustomAttributesData();
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		return GetResolvedAssembly().GetCustomAttributes(inherit);
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		return GetResolvedAssembly().GetCustomAttributes(attributeType, inherit);
	}

	public override Type GetType(string name, bool throwOnError, bool ignoreCase)
	{
		return GetResolvedAssembly().GetType(name, throwOnError, ignoreCase);
	}

	public override Module GetModule(string name)
	{
		return GetResolvedAssembly().GetModule(name);
	}

	public override Module[] GetLoadedModules(bool getResourceModules)
	{
		return GetResolvedAssembly().GetLoadedModules(getResourceModules);
	}

	public override Module[] GetModules(bool getResourceModules)
	{
		return GetResolvedAssembly().GetModules(getResourceModules);
	}

	public override AssemblyName[] GetReferencedAssemblies()
	{
		return GetResolvedAssembly().GetReferencedAssemblies();
	}

	public override Assembly GetSatelliteAssembly(CultureInfo culture)
	{
		return GetResolvedAssembly().GetSatelliteAssembly(culture);
	}

	public override Assembly GetSatelliteAssembly(CultureInfo culture, Version version)
	{
		return GetResolvedAssembly().GetSatelliteAssembly(culture, version);
	}

	public override Stream GetManifestResourceStream(Type type, string name)
	{
		return GetResolvedAssembly().GetManifestResourceStream(type, name);
	}

	public override Stream GetManifestResourceStream(string name)
	{
		return GetResolvedAssembly().GetManifestResourceStream(name);
	}

	public override string[] GetManifestResourceNames()
	{
		return GetResolvedAssembly().GetManifestResourceNames();
	}

	public override ManifestResourceInfo GetManifestResourceInfo(string resourceName)
	{
		return GetResolvedAssembly().GetManifestResourceInfo(resourceName);
	}

	public override FileStream[] GetFiles(bool getResourceModules)
	{
		return GetResolvedAssembly().GetFiles(getResourceModules);
	}

	public override FileStream[] GetFiles()
	{
		return GetResolvedAssembly().GetFiles();
	}

	public override FileStream GetFile(string name)
	{
		return GetResolvedAssembly().GetFile(name);
	}
}
