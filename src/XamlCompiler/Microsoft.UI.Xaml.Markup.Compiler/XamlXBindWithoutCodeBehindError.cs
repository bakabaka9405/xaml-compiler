using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindWithoutCodeBehindError : XamlCompileError
{
	public XamlXBindWithoutCodeBehindError(XamlDomMember domMember)
		: base(ErrorCode.WMC1119, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindWithoutCodeBehind);
	}
}
