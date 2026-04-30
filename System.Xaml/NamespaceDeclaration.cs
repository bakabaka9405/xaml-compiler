using System.Diagnostics;

namespace System.Xaml;

[DebuggerDisplay("Prefix={Prefix} Namespace={Namespace}")]
public class NamespaceDeclaration
{
	private string prefix;

	private string ns;

	public string Prefix => prefix;

	public string Namespace => ns;

	public NamespaceDeclaration(string ns, string prefix)
	{
		this.ns = ns;
		this.prefix = prefix;
	}
}
