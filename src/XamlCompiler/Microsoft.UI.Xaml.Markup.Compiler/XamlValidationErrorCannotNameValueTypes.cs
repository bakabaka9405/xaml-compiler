using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorCannotNameValueTypes : XamlCompileError
{
	public XamlValidationErrorCannotNameValueTypes(XamlDomObject domObject)
		: base(ErrorCode.WMC0045, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantNameValueTypes, domObject.Type.Name);
	}
}
