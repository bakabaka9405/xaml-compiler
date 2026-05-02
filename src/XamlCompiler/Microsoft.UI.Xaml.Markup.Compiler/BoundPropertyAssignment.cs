using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class BoundPropertyAssignment : BindAssignment
{
	public BoundPropertyAssignment(XamlDomMember bindMember, BindUniverse bindUniverse, ConnectionIdElement connectionIdElement)
		: base(bindMember, bindUniverse, connectionIdElement)
	{
	}
}
