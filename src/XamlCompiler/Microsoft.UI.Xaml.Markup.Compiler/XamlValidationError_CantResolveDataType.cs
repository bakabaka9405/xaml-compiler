using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationError_CantResolveDataType : XamlCompileError
{
	public XamlValidationError_CantResolveDataType(XamlDomObject domObject, string dataTypeName)
		: base(ErrorCode.WMC0909, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantResolveDataType, dataTypeName);
	}
}
