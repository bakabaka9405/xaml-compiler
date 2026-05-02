using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XbfOutputFileOpenFailure : XamlCompileError
{
	public XbfOutputFileOpenFailure(string xbfFile, string message)
		: base(ErrorCode.WMC0600, xbfFile, 0, 0)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_XbfOutputFileOpenFailure, message);
	}
}
