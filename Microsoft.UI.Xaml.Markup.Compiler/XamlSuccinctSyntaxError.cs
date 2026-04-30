using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlSuccinctSyntaxError : XamlCompileError
{
	public XamlSuccinctSyntaxError(int line, int col, string offendingToken, string fileName)
		: base(ErrorCode.WMC0155, fileName, line, col)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlValidationError_SuccinctSyntaxError, line, col, offendingToken);
	}
}
