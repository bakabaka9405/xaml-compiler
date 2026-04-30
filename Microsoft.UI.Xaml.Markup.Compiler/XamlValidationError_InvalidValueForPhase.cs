using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationError_InvalidValueForPhase : XamlCompileError
{
	public XamlValidationError_InvalidValueForPhase(XamlDomObject domObject)
		: base(ErrorCode.WMC0910, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidValueForPhase);
	}
}
