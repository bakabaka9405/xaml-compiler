using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XbfGeneration_NonMeInCurlyBraces : XamlCompileError
{
	public XbfGeneration_NonMeInCurlyBraces(string fileName, int line, int column, string nonMeName, int xbfErrorCode)
		: base(ErrorCode.WMC0615, fileName, line, column)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_SyntaxErrorME, nonMeName, "0x" + xbfErrorCode.ToString("x4"));
	}
}
