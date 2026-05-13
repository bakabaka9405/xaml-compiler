using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class XamlTypeUniverse : SimpleUniverse
{
	private Dictionary<string, Assembly> _asmNameCache = new Dictionary<string, Assembly>();

	private Dictionary<string, Type> _resolvedTypes = new Dictionary<string, Type>();

	private Loader _loader;

	private Assembly _systemAssembly;

	private Assembly _systemRuntimeAssembly;

	private Assembly _xamlProxyAssembly;

	public string ProjectPath { get; set; }

	public string[] ReferenceAssemblyPaths { get; set; }

	public bool IsSystemAssemblyLoaded => _systemAssembly != null;

	private Loader Loader => _loader;

	private Assembly SystemAssembly => _systemAssembly;

	public XamlTypeUniverse(bool useManagedProjections)
	{
		_loader = new Loader(this);
		if (useManagedProjections)
		{
			_loader.OpenFlags = CorOpenFlags.ReadOnly;
		}
		_loader.Factory = new XamlReflectionFactory();
	}

	public override Module ResolveModule(Assembly containingAssembly, string moduleName)
	{
		return _loader.ResolveModule(containingAssembly, moduleName);
	}

    /// <summary>
    /// Load assembly metadata from a byte array without holding any file handle.
    /// The CLR metadata dispenser reads directly from pinned managed memory,
    /// so the WinMD source file can be overwritten immediately by build tools.
    /// </summary>
    /// <param name="data">Raw PE/COFF bytes of the WinMD file.</param>
    /// <param name="virtualPath">Original file path, used only for diagnostics
    /// and assembly identity; no file handle is opened.</param>
    internal Assembly LoadAssemblyFromByteArray(byte[] data, string virtualPath)
    {
        // Uses the fileName-aware overload so that:
        // 1. MetadataDispenser.OpenFromByteArray creates a FileMapping with the
        //    real path (enables MetadataFileAndRvaResolver for full RVA support).
        // 2. AssemblyFactory.CreateAssembly receives the real path as assembly
        //    Location (enables dedup and module resolution).
        Assembly assembly = Loader.LoadAssemblyFromByteArray(data, virtualPath);

        // Register in the name cache for ResolveAssembly lookups.
        // Loader.LoadAssemblyFromByteArray already called m_universe.AddAssembly,
        // so the assembly is in m_loadedAssemblies. We synchronize _asmNameCache
        // and system assembly tracking to match LoadAssemblyFromFile.
        string text = assembly.GetName().FullName;
        if (!_asmNameCache.ContainsKey(text))
        {
            _asmNameCache.Add(text, assembly);
        }

        if (assembly.GetName().Name.Equals("mscorlib"))
        {
            _systemAssembly = assembly;
            SetSystemAssembly(_systemAssembly);
        }
        if (assembly.GetName().Name.Equals("System.Runtime", StringComparison.OrdinalIgnoreCase))
        {
            _systemRuntimeAssembly = assembly;
        }

        return assembly;
    }

    public Assembly LoadAssemblyFromFile(string path)
    {
        string fullPath = Path.GetFullPath(path);
        Assembly assembly = Loader.ReadAssemblyFromFile(path);
		string text = assembly.GetName().FullName;
		Assembly value = null;
		while (_asmNameCache.TryGetValue(text, out value))
		{
			if (value.Location.Equals(assembly.Location))
			{
				return value;
			}
			text = "!" + text;
		}
		AddAssembly(assembly);
		_asmNameCache.Add(text, assembly);
		if (assembly.GetName().Name.Equals("mscorlib"))
		{
			_systemAssembly = assembly;
			SetSystemAssembly(_systemAssembly);
		}
		if (assembly.GetName().Name.Equals("System.Runtime"))
		{
			_systemRuntimeAssembly = assembly;
		}
		return assembly;
	}

	public override Assembly GetSystemRuntimeAssembly()
	{
		return _systemRuntimeAssembly;
	}

	public override Assembly ResolveAssembly(AssemblyName name, bool throwOnError)
	{
		string fullName = name.FullName;
		if (!_asmNameCache.TryGetValue(fullName, out var value))
		{
			// WinRT metadata (.winmd) files reference core framework assemblies
			// (mscorlib, System.Runtime) at the pseudo-version 255.255.255.255,
			// which does not exist in any .NET runtime. Redirect to the real
			// system assemblies loaded by GetSystemAssembly/GetSystemRuntimeAssembly.
			value = ResolveWinmdPseudoAssembly(name);
			if (value != null)
			{
				_asmNameCache.Add(fullName, value);
				return value;
			}

			value = base.ResolveAssembly(name, throwOnError);
			if (!_asmNameCache.TryGetValue(fullName, out var _))
			{
				_asmNameCache.Add(fullName, value);
			}
		}
		return value;
	}

	/// <summary>
	/// Attempt to resolve WinMD pseudo-references to core framework assemblies.
	///
	/// WinMD metadata files reference mscorlib and System.Runtime at version
	/// 255.255.255.255 — a placeholder that the CLR metadata loader expects to
	/// be redirected. This method maps those pseudo-references back to the
	/// real implementations loaded by this type universe.
	/// </summary>
	private Assembly ResolveWinmdPseudoAssembly(AssemblyName name)
	{
		if (name.Version == null
			|| name.Version.Major != 255
			|| name.Version.Minor != 255
			|| name.Version.Build != 255
			|| name.Version.Revision != 255)
		{
			return null;
		}

		if (string.Equals(name.Name, "mscorlib", StringComparison.OrdinalIgnoreCase))
		{
			return GetSystemAssembly();
		}

		if (string.Equals(name.Name, "System.Runtime", StringComparison.OrdinalIgnoreCase))
		{
			return GetSystemRuntimeAssembly() ?? GetSystemAssembly();
		}

		return null;
	}

	public override Assembly GetSystemAssembly()
	{
		if (_systemAssembly == null)
		{
			string text = string.Empty;
			if (ReferenceAssemblyPaths != null && ReferenceAssemblyPaths.Length != 0)
			{
				string[] referenceAssemblyPaths = ReferenceAssemblyPaths;
				foreach (string path in referenceAssemblyPaths)
				{
					text = Path.Combine(path, "mscorlib.dll");
					if (File.Exists(text))
					{
						break;
					}
				}
			}
			else
			{
				Assembly assembly = typeof(int).Assembly;
				text = assembly.Location;
			}
			try
			{
				LoadAssemblyFromFile(text);
			}
			catch (FileNotFoundException innerException)
			{
				throw new FileNotFoundException($"MsCorLib.dll not found at '{text}'", text, innerException);
			}
		}
		return base.GetSystemAssembly();
	}

	public Assembly GetXamlProxyAssembly()
	{
		if (_xamlProxyAssembly == null)
		{
			string location = Assembly.GetCallingAssembly().Location;
			_xamlProxyAssembly = LoadAssemblyFromFile(location);
		}
		return _xamlProxyAssembly;
	}

	public Type FindType(string typeName)
	{
		if (!_resolvedTypes.TryGetValue(typeName, out var value))
		{
			value = GetSystemAssembly().GetType(typeName);
			if (value == null)
			{
				foreach (Assembly assembly in base.Assemblies)
				{
					value = assembly.GetType(typeName);
					if (value != null)
					{
						break;
					}
				}
			}
			if (value != null)
			{
				_resolvedTypes.Add(typeName, value);
			}
		}
		return value;
	}
}
