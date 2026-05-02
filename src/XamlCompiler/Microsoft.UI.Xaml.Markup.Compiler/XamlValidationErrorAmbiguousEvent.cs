using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorAmbiguousEvent : XamlCompileError
{
	public XamlValidationErrorAmbiguousEvent(XamlDomMember domMember)
		: base(ErrorCode.WMC0154, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlValidationError_AmbiguousEvent);
	}
}
