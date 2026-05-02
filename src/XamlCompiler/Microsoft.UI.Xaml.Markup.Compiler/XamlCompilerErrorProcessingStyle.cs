using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlCompilerErrorProcessingStyle : XamlCompileError
{
	public XamlCompilerErrorProcessingStyle(XamlDomObject domStyle)
		: base(ErrorCode.WMC0115, domStyle)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InternalErrorProcessingStyle);
	}
}
