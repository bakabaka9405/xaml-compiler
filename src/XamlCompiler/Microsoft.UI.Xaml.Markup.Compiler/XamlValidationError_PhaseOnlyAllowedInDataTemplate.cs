using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationError_PhaseOnlyAllowedInDataTemplate : XamlCompileError
{
	public XamlValidationError_PhaseOnlyAllowedInDataTemplate(XamlDomObject domObject)
		: base(ErrorCode.WMC0912, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_PhaseMustBeUsedWithinADataTemplate);
	}
}
