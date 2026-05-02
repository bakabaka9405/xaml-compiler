using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class BindAssignmentValidationError : XamlCompileError
{
	public BindAssignmentValidationError(IXamlDomNode node, string message)
		: base(ErrorCode.WMC1121, node)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_XamlXBindAssignmentValidationError, message);
	}
}
