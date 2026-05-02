using System.Diagnostics;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[DebuggerDisplay("AssemblyRef: {m_name}")]
internal class AssemblyRef : AssemblyProxy
{
	private readonly AssemblyName m_name;

	public AssemblyRef(AssemblyName name, ITypeUniverse universe)
		: base(universe)
	{
		m_name = name;
	}

	protected override Assembly GetResolvedAssemblyWorker()
	{
		return base.TypeUniverse.ResolveAssembly(m_name);
	}

	protected override AssemblyName GetNameWithNoResolution()
	{
		return m_name;
	}

	public override AssemblyName GetName()
	{
		return m_name;
	}
}
