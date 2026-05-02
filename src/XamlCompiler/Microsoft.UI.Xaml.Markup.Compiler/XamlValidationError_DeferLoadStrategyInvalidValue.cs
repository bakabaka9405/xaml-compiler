using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationError_DeferLoadStrategyInvalidValue : XamlCompileError
{
	public XamlValidationError_DeferLoadStrategyInvalidValue(XamlDomMember domMember)
		: base(ErrorCode.WMC0906, domMember)
	{
		XamlMember member = domMember.Member;
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlValidationError_DeferLoadStrategyInvalidValue, member.Name);
	}
}
