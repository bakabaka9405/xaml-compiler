using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationIdPropertiesMustBeText : XamlCompileError
{
	public XamlValidationIdPropertiesMustBeText(XamlDomMember domMember)
		: base(ErrorCode.WMC0030, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_IdPropertiesMustBeText, domMember.Member.Name);
	}
}
