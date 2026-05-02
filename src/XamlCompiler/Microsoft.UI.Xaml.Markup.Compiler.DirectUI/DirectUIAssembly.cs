using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

public class DirectUIAssembly : Assembly
{
	private readonly Assembly _assembly;

	private AssemblyName _assemblyName;

	public Assembly WrappedAssembly => _assembly;

	public bool IsWinmd => _assemblyName.ContentType == AssemblyContentType.WindowsRuntime;

	public string BaseName => _assemblyName.Name;

	public override bool ReflectionOnly => _assembly.ReflectionOnly;

	public override string FullName => _assembly.FullName;

	public override string Location => _assembly.Location;

	private DirectUIAssembly(Assembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		_assembly = assembly;
		_assemblyName = assembly.GetName();
	}

	public static DirectUIAssembly Wrap(Assembly assembly)
	{
		if (assembly == null)
		{
			throw new ArgumentNullException("assembly");
		}
		return new DirectUIAssembly(assembly);
	}

	public static IEnumerable<DirectUIAssembly> Wrap(IEnumerable<Assembly> assemblies)
	{
		if (assemblies == null)
		{
			yield break;
		}
		foreach (Assembly assembly in assemblies)
		{
			yield return Wrap(assembly);
		}
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		if (!IsWinmd)
		{
			return _assembly.GetCustomAttributes(inherit);
		}
		return new Attribute[0];
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		if (!IsWinmd)
		{
			return _assembly.GetCustomAttributes(attributeType, inherit);
		}
		return new Attribute[0];
	}

	public override AssemblyName GetName()
	{
		return _assemblyName;
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		if (!IsWinmd)
		{
			try
			{
				return _assembly.GetCustomAttributesData();
			}
			catch (TypeLoadException)
			{
				return new List<CustomAttributeData>();
			}
		}
		return new List<CustomAttributeData>();
	}

	public override Type GetType(string name)
	{
		Type result = null;
		try
		{
			result = _assembly.GetType(name);
		}
		catch (TypeLoadException)
		{
		}
		return result;
	}

	public override Type[] GetTypes()
	{
		return _assembly.GetTypes();
	}
}
