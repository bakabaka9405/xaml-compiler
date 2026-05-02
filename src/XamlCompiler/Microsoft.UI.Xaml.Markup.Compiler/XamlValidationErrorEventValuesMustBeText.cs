using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorEventValuesMustBeText : XamlCompileError
{
	public XamlValidationErrorEventValuesMustBeText(XamlDomNode domNode, string eventName)
		: base(ErrorCode.WMC0125, domNode)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_EventValuesMustBeText, eventName);
	}
}
