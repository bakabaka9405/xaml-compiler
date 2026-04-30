using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorBadCPA : XamlCompileError
{
	public XamlValidationErrorBadCPA(XamlDomObject domObject, string cpaName)
		: base(ErrorCode.WMC0070, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidCPA, domObject.Type.Name, cpaName);
	}
}
