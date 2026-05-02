using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlBindingAotCompatibilityWarning : XamlCompileWarning
{
	public XamlBindingAotCompatibilityWarning(IXamlDomNode node)
		: base(ErrorCode.WMC1510, node)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_BindingAotCompatibility);
	}
}
