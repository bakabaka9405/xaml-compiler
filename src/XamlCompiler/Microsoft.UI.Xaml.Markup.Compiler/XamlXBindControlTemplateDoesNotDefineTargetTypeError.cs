using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindControlTemplateDoesNotDefineTargetTypeError : XamlCompileError
{
	public XamlXBindControlTemplateDoesNotDefineTargetTypeError(IXamlDomNode node)
		: base(ErrorCode.WMC1111, node)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.Harvester_ControlTemplateDoesNotDefineTargetType);
	}
}
