using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindDataTemplateDoesNotDefineDataTypeError : XamlCompileError
{
	public XamlXBindDataTemplateDoesNotDefineDataTypeError(IXamlDomNode node)
		: base(ErrorCode.WMC1111, node)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.Harvester_DataTemplateDoesNotDefineDataType);
	}
}
