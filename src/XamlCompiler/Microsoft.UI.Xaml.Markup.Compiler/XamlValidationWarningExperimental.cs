using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationWarningExperimental : XamlCompileWarning
{
	public XamlValidationWarningExperimental(ErrorCode warningCode, string name)
		: base(warningCode)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Experimental, name);
	}

	public XamlValidationWarningExperimental(ErrorCode warningCode, IXamlDomNode domNode, string name)
		: base(warningCode, domNode)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Experimental, name);
	}
}
