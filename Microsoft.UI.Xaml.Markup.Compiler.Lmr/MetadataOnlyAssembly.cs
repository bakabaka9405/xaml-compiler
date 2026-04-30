using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyAssembly : Assembly, IAssembly2, IDisposable
{
	private readonly Module[] m_modules;

	private readonly MetadataOnlyModule m_manifestModule;

	private readonly string m_manifestFile;

	private readonly AssemblyName m_name;

	private string _assemblyFullName;

	private Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

	private IList<CustomAttributeData> _customAttributeDataCache;

	public override string FullName
	{
		get
		{
			if (_assemblyFullName == null)
			{
				_assemblyFullName = m_name.FullName;
			}
			return _assemblyFullName;
		}
	}

	public override string Location => m_manifestFile;

	public override bool ReflectionOnly => true;

	public override Module ManifestModule => m_modules[0];

	internal MetadataOnlyModule ManifestModuleInternal => m_manifestModule;

	public override string CodeBase => GetCodeBaseFromManifestModule(m_manifestModule);

	public override MethodInfo EntryPoint
	{
		get
		{
			MetadataFile rawMetadata = m_manifestModule.RawMetadata;
			Token token = rawMetadata.ReadEntryPointToken();
			if (token.IsNil)
			{
				return null;
			}
			switch (token.TokenType)
			{
			case TokenType.FieldDef:
				throw new NotImplementedException();
			case TokenType.MethodDef:
			{
				MethodBase methodBase = ManifestModule.ResolveMethod(token.Value);
				return (MethodInfo)methodBase;
			}
			default:
				throw new InvalidOperationException(Resources.InvalidMetadata);
			}
		}
	}

	public override string ImageRuntimeVersion => m_manifestModule.GetRuntimeVersion();

	public ITypeUniverse TypeUniverse => m_manifestModule.AssemblyResolver;

	internal MetadataOnlyAssembly(MetadataOnlyModule manifestModule, string manifestFile)
		: this(new MetadataOnlyModule[1] { manifestModule }, manifestFile)
	{
	}

	internal MetadataOnlyAssembly(MetadataOnlyModule[] modules, string manifestFile)
	{
		VerifyModules(modules);
		m_manifestModule = modules[0];
		m_name = AssemblyNameHelper.GetAssemblyName(m_manifestModule);
		m_manifestFile = manifestFile;
		foreach (MetadataOnlyModule metadataOnlyModule in modules)
		{
			metadataOnlyModule.SetContainingAssembly(this);
		}
		List<Module> list = new List<Module>(modules);
		bool getResources = false;
		List<string> fileNamesFromFilesTable = GetFileNamesFromFilesTable(m_manifestModule, getResources);
		foreach (string netModuleName in fileNamesFromFilesTable)
		{
			if (!(list.Find((Module module2) => module2.Name.Equals(netModuleName, StringComparison.OrdinalIgnoreCase)) != null))
			{
				Module module = m_manifestModule.AssemblyResolver.ResolveModule(this, netModuleName);
				if (module == null)
				{
					throw new InvalidOperationException(Resources.ResolverMustResolveToValidModule);
				}
				if (module.Assembly != this)
				{
					throw new InvalidOperationException(Resources.ResolverMustSetAssemblyProperty);
				}
				list.Add(module);
			}
		}
		m_modules = list.ToArray();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing || m_modules == null)
		{
			return;
		}
		Module[] modules = m_modules;
		foreach (Module module in modules)
		{
			if (module is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}

	private static void VerifyModules(MetadataOnlyModule[] modules)
	{
		if (modules == null || modules.Length < 1)
		{
			throw new ArgumentException(Resources.ManifestModuleMustBeProvided);
		}
		if (GetAssemblyToken(modules[0]) == Token.Nil)
		{
			throw new ArgumentException(Resources.NoAssemblyManifest);
		}
		for (int i = 1; i < modules.Length; i++)
		{
			if (GetAssemblyToken(modules[i]) != Token.Nil)
			{
				throw new ArgumentException(Resources.ExtraAssemblyManifest);
			}
		}
	}

	private static List<string> GetFileNamesFromFilesTable(MetadataOnlyModule manifestModule, bool getResources)
	{
		HCORENUM phEnum = default(HCORENUM);
		List<string> list = new List<string>();
		StringBuilder builder = StringBuilderPool.Get();
		IMetadataAssemblyImport metadataAssemblyImport = (IMetadataAssemblyImport)manifestModule.RawImport;
		try
		{
			while (true)
			{
				metadataAssemblyImport.EnumFiles(ref phEnum, out var files, 1, out var cTokens);
				if (cTokens == 0)
				{
					break;
				}
				metadataAssemblyImport.GetFileProps(files, null, 0, out var pchName, out var ppbHashValue, out var pcbHashValue, out var dwFileFlags);
				if (getResources || dwFileFlags != CorFileFlags.ContainsNoMetaData)
				{
					builder.Length = 0;
					builder.EnsureCapacity(pchName);
					metadataAssemblyImport.GetFileProps(files, builder, builder.Capacity, out pchName, out ppbHashValue, out pcbHashValue, out dwFileFlags);
					list.Add(builder.ToString());
				}
			}
		}
		finally
		{
			phEnum.Close(metadataAssemblyImport);
		}
		StringBuilderPool.Release(ref builder);
		return list;
	}

	public override int GetHashCode()
	{
		return m_modules[0].GetHashCode();
	}

	public override bool Equals(object obj)
	{
		Assembly assembly = obj as Assembly;
		if (assembly == null)
		{
			return false;
		}
		return ManifestModule.Equals(assembly.ManifestModule);
	}

	public override Stream GetManifestResourceStream(Type type, string name)
	{
		StringBuilder builder = StringBuilderPool.Get();
		if (type == null)
		{
			if (name == null)
			{
				throw new ArgumentNullException("type");
			}
		}
		else
		{
			string text = type.Namespace;
			if (text != null)
			{
				builder.Append(text);
				if (name != null)
				{
					builder.Append(Type.Delimiter);
				}
			}
		}
		if (name != null)
		{
			builder.Append(name);
		}
		string name2 = builder.ToString();
		StringBuilderPool.Release(ref builder);
		return GetManifestResourceStream(name2);
	}

	public override Stream GetManifestResourceStream(string name)
	{
		IMetadataAssemblyImport metadataAssemblyImport = (IMetadataAssemblyImport)m_manifestModule.RawImport;
		metadataAssemblyImport.FindManifestResourceByName(name, out var ptkManifestResource);
		if (new Token(ptkManifestResource).IsNil)
		{
			return null;
		}
		StringBuilder builder = StringBuilderPool.Get(name.Length + 1);
		metadataAssemblyImport.GetManifestResourceProps(ptkManifestResource, builder, builder.Capacity, out var _, out var ptkImplementation, out var pdwOffset, out var _);
		StringBuilderPool.Release(ref builder);
		Token token = new Token(ptkImplementation);
		if (token.TokenType == TokenType.File)
		{
			if (token.IsNil)
			{
				byte[] buffer = m_manifestModule.RawMetadata.ReadResource(pdwOffset);
				return new MemoryStream(buffer);
			}
			metadataAssemblyImport.GetFileProps(token.Value, null, 0, out var pchName2, out var ppbHashValue, out var pcbHashValue, out var dwFileFlags);
			StringBuilder builder2 = StringBuilderPool.Get(pchName2);
			metadataAssemblyImport.GetFileProps(token.Value, builder2, builder2.Capacity, out pchName2, out ppbHashValue, out pcbHashValue, out dwFileFlags);
			string directoryName = Path.GetDirectoryName(Location);
			string path = Path.Combine(directoryName, builder2.ToString());
			StringBuilderPool.Release(ref builder2);
			return new FileStream(path, FileMode.Open);
		}
		if (token.TokenType == TokenType.AssemblyRef)
		{
			throw new NotImplementedException();
		}
		throw new ArgumentException(Resources.InvalidMetadata);
	}

	public override string[] GetManifestResourceNames()
	{
		HCORENUM phEnum = default(HCORENUM);
		List<string> list = new List<string>();
		StringBuilder builder = StringBuilderPool.Get();
		IMetadataAssemblyImport metadataAssemblyImport = (IMetadataAssemblyImport)m_manifestModule.RawImport;
		try
		{
			while (true)
			{
				metadataAssemblyImport.EnumManifestResources(ref phEnum, out var rManifestResources, 1, out var cTokens);
				if (cTokens == 0)
				{
					break;
				}
				metadataAssemblyImport.GetManifestResourceProps(rManifestResources, null, 0, out var pchName, out var ptkImplementation, out var pdwOffset, out var pdwResourceFlags);
				builder.Length = 0;
				builder.EnsureCapacity(pchName);
				metadataAssemblyImport.GetManifestResourceProps(rManifestResources, builder, builder.Capacity, out pchName, out ptkImplementation, out pdwOffset, out pdwResourceFlags);
				list.Add(builder.ToString());
			}
		}
		finally
		{
			phEnum.Close(metadataAssemblyImport);
		}
		StringBuilderPool.Release(ref builder);
		return list.ToArray();
	}

	public override AssemblyName GetName()
	{
		return m_name;
	}

	public override AssemblyName GetName(bool copiedName)
	{
		throw new NotImplementedException();
	}

	public override Type[] GetExportedTypes()
	{
		Type[] types = GetTypes();
		List<Type> list = new List<Type>();
		Type[] array = types;
		foreach (Type type in array)
		{
			if (type.IsVisible)
			{
				list.Add(type);
			}
		}
		return list.ToArray();
	}

	public override Type GetType(string name)
	{
		return GetType(name, throwOnError: false, ignoreCase: false);
	}

	public override Type GetType(string name, bool throwOnError)
	{
		return GetType(name, throwOnError, ignoreCase: false);
	}

	public override Type GetType(string name, bool throwOnError, bool ignoreCase)
	{
		Type value = null;
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		string key = ((!ignoreCase) ? name : name.ToLower(CultureInfo.CurrentCulture));
		if (!_typeCache.TryGetValue(key, out value))
		{
			for (int i = 0; i < m_modules.Length; i++)
			{
				value = m_modules[i].GetType(name, throwOnError: false, ignoreCase);
				if (value != null)
				{
					break;
				}
			}
			if (value == null)
			{
				value = m_manifestModule.Policy.TryTypeForwardResolution(this, name, ignoreCase);
			}
			if (value != null)
			{
				_typeCache.Add(key, value);
			}
		}
		if (value != null)
		{
			return value;
		}
		if (throwOnError)
		{
			throw new TypeLoadException(string.Format(CultureInfo.InvariantCulture, Resources.CannotFindTypeInModule, name, m_modules[0].ScopeName));
		}
		return null;
	}

	public override Type[] GetTypes()
	{
		List<Type> list = new List<Type>();
		Module[] modules = m_modules;
		foreach (Module module in modules)
		{
			list.AddRange(module.GetTypes());
		}
		return list.ToArray();
	}

	public override Module GetModule(string name)
	{
		Module[] modules = m_modules;
		foreach (Module module in modules)
		{
			if (module.ScopeName.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				return module;
			}
		}
		return null;
	}

	public override Module[] GetModules(bool getResourceModules)
	{
		return m_modules;
	}

	public override Module[] GetLoadedModules(bool getResourceModules)
	{
		return m_modules;
	}

	internal static string GetCodeBaseFromManifestModule(MetadataOnlyModule manifestModule)
	{
		string fullyQualifiedName = manifestModule.FullyQualifiedName;
		if (!Utility.IsValidPath(fullyQualifiedName))
		{
			return string.Empty;
		}
		try
		{
			return new Uri(fullyQualifiedName).ToString();
		}
		catch (Exception)
		{
			throw;
		}
	}

	internal static Token GetAssemblyToken(MetadataOnlyModule module)
	{
		if (((IMetadataAssemblyImport)module.RawImport).GetAssemblyFromScope(out var assemblyToken) == 0)
		{
			return new Token(assemblyToken);
		}
		return Token.Nil;
	}

	public override FileStream[] GetFiles(bool getResourceModules)
	{
		List<string> list = new List<string>();
		Module[] modules = m_modules;
		foreach (Module module in modules)
		{
			list.Add(module.FullyQualifiedName);
		}
		if (getResourceModules)
		{
			string directoryName = Path.GetDirectoryName(m_manifestFile);
			foreach (string item in GetFileNamesFromFilesTable(m_manifestModule, getResources: true))
			{
				list.Add(Path.Combine(directoryName, item));
			}
		}
		return ConvertFileNamesToStreams(list.ToArray());
	}

	public override FileStream GetFile(string name)
	{
		Module module = GetModule(name);
		if (module == null)
		{
			return null;
		}
		return new FileStream(module.FullyQualifiedName, FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	private static FileStream[] ConvertFileNamesToStreams(string[] filenames)
	{
		return Array.ConvertAll(filenames, (string n) => new FileStream(n, FileMode.Open, FileAccess.Read, FileShare.Read));
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		if (_customAttributeDataCache == null)
		{
			_customAttributeDataCache = m_manifestModule.GetCustomAttributeData(GetAssemblyToken(m_manifestModule));
		}
		return _customAttributeDataCache;
	}

	public override AssemblyName[] GetReferencedAssemblies()
	{
		IMetadataAssemblyImport metadataAssemblyImport = (IMetadataAssemblyImport)m_manifestModule.RawImport;
		List<AssemblyName> list = new List<AssemblyName>();
		HCORENUM phEnum = default(HCORENUM);
		try
		{
			while (true)
			{
				Token assemblyRefs;
				int cTokens;
				int errorCode = metadataAssemblyImport.EnumAssemblyRefs(ref phEnum, out assemblyRefs, 1, out cTokens);
				Marshal.ThrowExceptionForHR(errorCode);
				if (cTokens == 0)
				{
					break;
				}
				AssemblyName assemblyNameFromRef = AssemblyNameHelper.GetAssemblyNameFromRef(assemblyRefs, m_manifestModule, metadataAssemblyImport);
				list.Add(assemblyNameFromRef);
			}
		}
		finally
		{
			phEnum.Close(metadataAssemblyImport);
		}
		return list.ToArray();
	}
}
