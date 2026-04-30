using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationWarningUsingCodeGenFlags : XamlCompileWarning
{
	public XamlValidationWarningUsingCodeGenFlags(CodeGenCtrlFlags flags)
		: base(ErrorCode.WMC1004)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CodeGenString_Using, flags.ToString());
	}
}
