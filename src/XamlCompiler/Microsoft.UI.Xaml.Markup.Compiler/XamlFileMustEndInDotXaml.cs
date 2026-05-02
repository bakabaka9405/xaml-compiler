using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlFileMustEndInDotXaml : XamlCompileError
{
	public XamlFileMustEndInDotXaml(string fileName)
		: base(ErrorCode.WMC1010, fileName, 0, 0)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XamlFileMustEndInDotXaml);
	}
}
