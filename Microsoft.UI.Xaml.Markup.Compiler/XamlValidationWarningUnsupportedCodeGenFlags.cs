using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationWarningUnsupportedCodeGenFlags : XamlCompileWarning
{
	public XamlValidationWarningUnsupportedCodeGenFlags(CodeGenCtrlFlags flags)
		: base(ErrorCode.WMC1014)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CodeGenString_NotSupported, flags.ToString());
	}
}
