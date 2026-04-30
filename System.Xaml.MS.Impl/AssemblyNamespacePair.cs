using System.Diagnostics;
using System.Reflection;

namespace System.Xaml.MS.Impl;

[DebuggerDisplay("{ClrNamespace} {Assembly.FullName}")]
internal class AssemblyNamespacePair
{
	private WeakReference _assembly;

	private string _clrNamespace;

	public Assembly Assembly => (Assembly)_assembly.Target;

	public string ClrNamespace => _clrNamespace;

	public AssemblyNamespacePair(Assembly asm, string clrNamespace)
	{
		_assembly = new WeakReference(asm);
		_clrNamespace = clrNamespace;
	}
}
