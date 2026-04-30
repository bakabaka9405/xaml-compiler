using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorStyleMustHaveTargetType : XamlCompileError
{
	public XamlValidationErrorStyleMustHaveTargetType(XamlDomNode styleOrTargetType)
		: base(ErrorCode.WMC0080, styleOrTargetType)
	{
		base.Message = XamlCompilerResources.XamlCompiler_StyleMustHaveTargetType;
	}
}
