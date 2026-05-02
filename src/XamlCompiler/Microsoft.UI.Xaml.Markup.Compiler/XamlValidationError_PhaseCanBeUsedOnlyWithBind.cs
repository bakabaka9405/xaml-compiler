using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationError_PhaseCanBeUsedOnlyWithBind : XamlCompileError
{
	public XamlValidationError_PhaseCanBeUsedOnlyWithBind(XamlDomObject domObject)
		: base(ErrorCode.WMC0911, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_PhaseMustHaveAssociatedBind);
	}
}
