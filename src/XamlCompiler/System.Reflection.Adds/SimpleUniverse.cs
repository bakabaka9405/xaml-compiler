using System.Collections.Generic;
using System.Globalization;

namespace System.Reflection.Adds;

internal class SimpleUniverse : IMutableTypeUniverse, ITypeUniverse, IDisposable
{
	private Dictionary<string, Type> m_hash = new Dictionary<string, Type>();

	private List<Assembly> m_loadedAssemblies = new List<Assembly>();

	private Assembly m_systemAssembly;

	public IEnumerable<Assembly> Assemblies => m_loadedAssemblies;

	public event EventHandler<ResolveAssemblyNameEventArgs> OnResolveEvent;

	private bool IsAssemblyInList(Assembly assembly)
	{
		foreach (Assembly loadedAssembly in m_loadedAssemblies)
		{
			if (loadedAssembly.Equals(assembly))
			{
				return true;
			}
		}
		return false;
	}

	public void AddAssembly(Assembly assembly)
	{
		IAssembly2 assembly2 = (IAssembly2)assembly;
		m_loadedAssemblies.Add(assembly);
	}

	public void SetSystemAssembly(Assembly systemAssembly)
	{
		if (systemAssembly == null)
		{
			throw new ArgumentNullException("systemAssembly");
		}
		m_systemAssembly = systemAssembly;
	}

	public virtual Type GetBuiltInType(CorElementType elementType)
	{
		string nameForPrimitive = ElementTypeUtility.GetNameForPrimitive(elementType);
		return GetTypeXFromName(nameForPrimitive);
	}

	public virtual Type GetTypeXFromName(string fullName)
	{
		if (!m_hash.TryGetValue(fullName, out var value))
		{
			value = GetSystemAssembly().GetType(fullName, throwOnError: true, ignoreCase: false);
			m_hash[fullName] = value;
		}
		return value;
	}

	public virtual Assembly GetSystemAssembly()
	{
		if (m_systemAssembly == null)
		{
			Assembly assembly = FindSystemAssembly();
			if (assembly != null)
			{
				SetSystemAssembly(assembly);
			}
		}
		if (m_systemAssembly == null)
		{
			throw new UnresolvedAssemblyException(string.Format(CultureInfo.InvariantCulture, Resources.CannotDetermineSystemAssembly));
		}
		return m_systemAssembly;
	}

	public virtual Assembly GetSystemRuntimeAssembly()
	{
		return null;
	}

	protected Assembly FindSystemAssembly()
	{
		foreach (Assembly loadedAssembly in m_loadedAssemblies)
		{
			if (loadedAssembly.GetReferencedAssemblies().Length == 0)
			{
				return loadedAssembly;
			}
		}
		return null;
	}

	public virtual Assembly ResolveAssembly(AssemblyName name)
	{
		return ResolveAssembly(name, throwOnError: true);
	}

	public virtual Assembly ResolveAssembly(AssemblyName name, bool throwOnError)
	{
		Assembly assembly = TryResolveAssembly(name);
		if (assembly != null)
		{
			return assembly;
		}
		if (this.OnResolveEvent != null)
		{
			ResolveAssemblyNameEventArgs e = new ResolveAssemblyNameEventArgs(name);
			this.OnResolveEvent(this, e);
			assembly = e.Target;
			_ = assembly != null;
		}
		if (assembly == null)
		{
			if (throwOnError)
			{
				throw new UnresolvedAssemblyException(string.Format(CultureInfo.InvariantCulture, Resources.UniverseCannotResolveAssembly, name));
			}
			return null;
		}
		if (!(assembly is IAssembly2 assembly2))
		{
			throw new InvalidOperationException(Resources.ResolverMustResolveToValidAssembly);
		}
		if (assembly2.TypeUniverse != this)
		{
			throw new InvalidOperationException(Resources.ResolvedAssemblyMustBeWithinSameUniverse);
		}
		return assembly;
	}

	public virtual Assembly ResolveAssembly(Module scope, Token tokenAssemblyRef)
	{
		IModule2 module = (IModule2)scope;
		AssemblyName assemblyNameFromAssemblyRef = module.GetAssemblyNameFromAssemblyRef(tokenAssemblyRef);
		return ResolveAssembly(assemblyNameFromAssemblyRef);
	}

	public virtual Module ResolveModule(Assembly containingAssembly, string moduleName)
	{
		throw new NotImplementedException();
	}

	public virtual bool WouldResolveToAssembly(AssemblyName name, Assembly assembly)
	{
		AssemblyName name2 = assembly.GetName();
		return AssemblyName.ReferenceMatchesDefinition(name, name2);
	}

	public Type ResolveWindowsRuntimeType(string typeName, bool throwOnError, bool ignoreCase)
	{
		string text = typeName;
		int num = text.LastIndexOf('.');
		while (0 <= num)
		{
			text = text.Remove(num);
			Assembly assembly = ResolveAssembly(new AssemblyName(text), throwOnError: false);
			if (null != assembly)
			{
				bool throwOnError2 = false;
				Type type = assembly.GetType(typeName, throwOnError2, ignoreCase);
				if (null != type)
				{
					return type;
				}
			}
			num = text.LastIndexOf('.');
		}
		if (throwOnError)
		{
			throw new TypeLoadException(string.Format(CultureInfo.InvariantCulture, Resources.WindowsRuntimeTypeNotFound, typeName));
		}
		return null;
	}

	protected Assembly TryResolveAssembly(AssemblyName name)
	{
		foreach (Assembly loadedAssembly in m_loadedAssemblies)
		{
			if (WouldResolveToAssembly(name, loadedAssembly))
			{
				return loadedAssembly;
			}
		}
		return null;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing || m_loadedAssemblies == null)
		{
			return;
		}
		foreach (Assembly loadedAssembly in m_loadedAssemblies)
		{
			if (loadedAssembly is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		m_loadedAssemblies = null;
	}
}
