using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationWarningDeprecated : XamlCompileWarning
{
	public XamlValidationWarningDeprecated(IXamlDomNode domObject, string name, string message)
		: base(ErrorCode.WMC1500, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Deprecated, name, message);
	}
}
