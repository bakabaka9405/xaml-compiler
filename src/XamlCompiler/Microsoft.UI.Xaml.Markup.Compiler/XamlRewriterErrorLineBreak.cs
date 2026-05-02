using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlRewriterErrorLineBreak : XamlCompileError
{
	public XamlRewriterErrorLineBreak(int line, int column)
		: base(ErrorCode.WMC0501, line, column)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlRewriter_EventsAcrossLine);
	}
}
