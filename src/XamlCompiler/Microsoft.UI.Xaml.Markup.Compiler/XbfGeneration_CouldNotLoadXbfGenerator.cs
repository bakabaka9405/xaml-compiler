using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XbfGeneration_CouldNotLoadXbfGenerator : XamlCompileError
{
	public XbfGeneration_CouldNotLoadXbfGenerator(string path)
		: base(ErrorCode.WMC0621, path, 0, 0)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_CouldNotLoadXbfGenerator, path);
	}
}
