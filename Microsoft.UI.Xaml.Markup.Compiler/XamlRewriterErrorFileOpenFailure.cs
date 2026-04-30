using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlRewriterErrorFileOpenFailure : XamlCompileError
{
	public XamlRewriterErrorFileOpenFailure(string xamlFileName, string message)
		: base(ErrorCode.WMC0502, xamlFileName, 0, 0)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_FileOpenFailure, message);
	}
}
