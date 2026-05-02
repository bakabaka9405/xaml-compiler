using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorBadName : XamlCompileError
{
	public XamlValidationErrorBadName(XamlDomMember domMember, string name)
		: base(ErrorCode.WMC0040, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_BadName, name, domMember.Member.Name, domMember.Parent.Type.Name);
	}

	public XamlValidationErrorBadName(XamlDomMember domMember, string name, char badChar)
		: base(ErrorCode.WMC0040, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_BadNameChar, name, domMember.Member.Name, domMember.Parent.Type.Name, badChar);
	}
}
