using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindOnControlTemplateError : XamlCompileError
{
	public XamlXBindOnControlTemplateError(XamlDomMember domMember)
		: base(ErrorCode.WMC1123, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindOnControlTemplate);
	}
}
