using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationError_InvalidValueForSuppressXamlTrimWarnings : XamlCompileError
{
	public XamlValidationError_InvalidValueForSuppressXamlTrimWarnings(XamlDomObject domObject)
		: base(ErrorCode.WMC0920, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidValueForSuppressXamlTrimWarnings);
	}
}
