using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XbfGenerationPropertyNotFoundError : XamlCompileError
{
	public XbfGenerationPropertyNotFoundError(string fileName, int line, int column)
		: base(ErrorCode.WMC0612, fileName, line, column)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XbfGeneration_PropertyNotFoundError);
	}
}
