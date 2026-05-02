using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationPlatformConditionalStrict : XamlCompileError
{
	public XamlValidationPlatformConditionalStrict(XamlDomNode domNode)
		: base(ErrorCode.WMC0918, domNode)
	{
		base.Message = XamlCompilerResources.ConditionalNamespace_ConditionalInStandard;
	}
}
