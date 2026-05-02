using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorStyleBasedOnMustBeStyle : XamlCompileError
{
	public XamlValidationErrorStyleBasedOnMustBeStyle(XamlDomObject styleObject, string keyString, XamlDomObject domBaseStyleObject, string otherFile)
		: base(ErrorCode.WMC0140, styleObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_StyleBasedOnMustBeStyle_SR, keyString, domBaseStyleObject.Type.Name);
	}

	public XamlValidationErrorStyleBasedOnMustBeStyle(XamlDomObject styleObject, XamlDomObject domNotStyleObject)
		: base(ErrorCode.WMC0141, styleObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_StyleBasedOnMustBeStyle_BadObj, domNotStyleObject.Type.Name);
	}

	public XamlValidationErrorStyleBasedOnMustBeStyle(XamlDomObject styleObject, string text)
		: base(ErrorCode.WMC0142, styleObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_StyleBasedOnMustBeStyle_Text, text);
	}
}
