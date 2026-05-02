using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindUsedInStyleError : XamlCompileError
{
	public XamlXBindUsedInStyleError(IXamlDomNode node)
		: base(ErrorCode.WMC1112, node)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_XamlXBindUsedInStyleError);
	}
}
