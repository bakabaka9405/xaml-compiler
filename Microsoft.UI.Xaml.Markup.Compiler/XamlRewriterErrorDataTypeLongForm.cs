using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlRewriterErrorDataTypeLongForm : XamlCompileError
{
	public XamlRewriterErrorDataTypeLongForm(int line, int column)
		: base(ErrorCode.WMC0504, line, column)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlRewriter_XamlRewriterErrorDataTypeLongForm);
	}
}
