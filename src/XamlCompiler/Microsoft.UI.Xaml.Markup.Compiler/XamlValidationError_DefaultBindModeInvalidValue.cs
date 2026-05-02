using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationError_DefaultBindModeInvalidValue : XamlCompileError
{
	public XamlValidationError_DefaultBindModeInvalidValue(XamlDomMember domMember)
		: base(ErrorCode.WMC0917, domMember)
	{
		XamlMember member = domMember.Member;
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlValidationError_DefaultBindModeInvalidValue, member.Name);
	}
}
