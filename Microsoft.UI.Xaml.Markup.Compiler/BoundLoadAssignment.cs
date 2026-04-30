using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class BoundLoadAssignment : BindAssignment
{
	public override XamlType MemberType => bindMember.SchemaContext.GetXamlType(typeof(bool));

	public override XamlType MemberDeclaringType => base.ConnectionIdElement.Type;

	public override bool IsAttachable => false;

	public override bool HasSetValueHelper => false;

	public override bool HasDeferredValueProxy => false;

	public BoundLoadAssignment(XamlDomMember bindMember, BindUniverse bindUniverse, ConnectionIdElement connectionIdElement)
		: base(bindMember, bindUniverse, connectionIdElement)
	{
	}
}
