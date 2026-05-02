using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationError_DeferLoadStrategyMissingXName : XamlCompileError
{
	public XamlValidationError_DeferLoadStrategyMissingXName(XamlDomObject domObject)
		: base(ErrorCode.WMC0907, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DeferLoadStrategyMissingXName);
	}
}
