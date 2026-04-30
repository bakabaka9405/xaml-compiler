using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XbfGeneration_NoWindowsSdk : XamlCompileError
{
	public XbfGeneration_NoWindowsSdk()
		: base(ErrorCode.WMC0620)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_MissingGenXbfPath);
	}
}
