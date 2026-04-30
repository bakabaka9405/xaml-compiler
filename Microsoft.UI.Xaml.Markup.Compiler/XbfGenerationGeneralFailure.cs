using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XbfGenerationGeneralFailure : XamlCompileError
{
	public XbfGenerationGeneralFailure(string message)
		: base(ErrorCode.WMC0605)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_GeneralFailure, message);
	}
}
