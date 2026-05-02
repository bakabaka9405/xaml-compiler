using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorDeprecated : XamlCompileError
{
	public XamlValidationErrorDeprecated(XamlDomObject domObject, string name, string message)
		: base(ErrorCode.WMC0150, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Deprecated, name, message);
	}

	public XamlValidationErrorDeprecated(XamlDomMember domMember, string name, string message)
		: base(ErrorCode.WMC0150, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Deprecated, name, message);
	}
}
