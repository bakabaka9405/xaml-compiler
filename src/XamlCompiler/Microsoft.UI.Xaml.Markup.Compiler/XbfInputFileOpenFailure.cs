using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XbfInputFileOpenFailure : XamlCompileError
{
	public XbfInputFileOpenFailure(string xbfFile, string message)
		: base(ErrorCode.WMC0601, xbfFile, 0, 0)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_XamlInputFileOpenFailure, message);
	}
}
