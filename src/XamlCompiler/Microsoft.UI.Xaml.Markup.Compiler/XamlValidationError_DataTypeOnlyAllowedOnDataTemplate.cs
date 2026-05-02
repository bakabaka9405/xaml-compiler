using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationError_DataTypeOnlyAllowedOnDataTemplate : XamlCompileError
{
	public XamlValidationError_DataTypeOnlyAllowedOnDataTemplate(XamlDomObject domObject)
		: base(ErrorCode.WMC0908, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlValidationError_DataTypeOnlyAllowedOnDataTemplate);
	}
}
