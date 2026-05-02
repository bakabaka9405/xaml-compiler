using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlRewriterErrorCompiledBindingLongForm : XamlCompileError
{
	public XamlRewriterErrorCompiledBindingLongForm(int line, int column)
		: base(ErrorCode.WMC0503, line, column)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlRewriter_CompiledBindingsCannotBeInElementForm);
	}
}
