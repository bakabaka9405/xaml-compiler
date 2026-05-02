using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlCompileErrorInvalidPropertyType_SignedChar : XamlCompileError
{
	public XamlCompileErrorInvalidPropertyType_SignedChar(XamlDomMember domMember)
		: base(ErrorCode.WMC0121, domMember)
	{
		string name = domMember.Member.Type.Name;
		string name2 = domMember.Member.Name;
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidSignedChar, name, name2);
	}
}
