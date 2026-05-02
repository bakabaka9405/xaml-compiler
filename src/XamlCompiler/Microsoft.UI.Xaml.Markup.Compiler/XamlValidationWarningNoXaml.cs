using Microsoft.UI.Xaml.Markup.Compiler.Properties;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationWarningNoXaml : XamlCompileWarning
{
	public XamlValidationWarningNoXaml()
		: base(ErrorCode.WMC1001)
	{
		base.Message = XamlCompilerResources.XamlCompiler_NoXamlGiven;
	}
}
