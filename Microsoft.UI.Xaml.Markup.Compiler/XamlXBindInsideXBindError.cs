using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindInsideXBindError : XamlCompileError
{
	public XamlXBindInsideXBindError(XamlDomMember domMember)
		: base(ErrorCode.WMC1122, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindInsideXBind, domMember.Member.Name);
	}
}
