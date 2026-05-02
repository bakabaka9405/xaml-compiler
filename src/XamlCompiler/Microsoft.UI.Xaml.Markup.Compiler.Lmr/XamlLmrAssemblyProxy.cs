using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class XamlLmrAssemblyProxy : AssemblyProxy
{
	private Assembly _assembly;

	private AssemblyName _asmName;

	public XamlLmrAssemblyProxy(ITypeUniverse universe, Assembly assembly)
		: base(universe)
	{
		_assembly = assembly;
	}

	protected override Assembly GetResolvedAssemblyWorker()
	{
		return _assembly;
	}

	protected override AssemblyName GetNameWithNoResolution()
	{
		if (_asmName == null)
		{
			_asmName = _assembly.GetName();
		}
		return _asmName;
	}

	public override int GetHashCode()
	{
		return GetResolvedAssembly().GetHashCode();
	}

	public override bool Equals(object obj)
	{
		AssemblyProxy assemblyProxy = obj as AssemblyProxy;
		if (assemblyProxy != null)
		{
			obj = assemblyProxy.GetResolvedAssembly();
		}
		return GetResolvedAssembly().Equals(obj);
	}
}
