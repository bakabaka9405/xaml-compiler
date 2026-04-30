using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlRewriterErrorEventLongForm : XamlCompileError
{
	public XamlRewriterErrorEventLongForm(int line, int column)
		: base(ErrorCode.WMC0500, line, column)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlRewriter_EventsCannotBeInElementForm);
	}
}
