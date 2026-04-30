using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class BindAssignmentValidationWarning : XamlCompileWarning
{
	public BindAssignmentValidationWarning(IXamlDomNode node, ErrorCode errorCode, string message)
		: base(errorCode, node)
	{
		base.Message = message;
	}
}
