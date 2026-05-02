using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationConditionalNamespaceError : XamlCompileError
{
	public XamlValidationConditionalNamespaceError(string expressionBeingParsed, string message, XamlDomNode domNode)
		: base(ErrorCode.WMC0916, domNode)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.ConditionalNamespace_FailedToParse, expressionBeingParsed, message);
	}
}
