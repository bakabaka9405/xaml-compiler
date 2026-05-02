using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XbfGenerationParseError : XamlCompileError
{
	public XbfGenerationParseError(string fileName, int line, int column, int xbfErrorCode)
		: base(ErrorCode.WMC0610, fileName, line, column)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_SyntaxError, "0x" + xbfErrorCode.ToString("x4"));
	}
}
