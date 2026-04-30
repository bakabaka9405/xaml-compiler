using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorMissingCPA : XamlCompileError
{
	public XamlValidationErrorMissingCPA(XamlDomObject domParentObject, XamlDomItem firstChild)
		: base(ErrorCode.WMC0075, firstChild)
	{
		XamlDomValue xamlDomValue = firstChild as XamlDomValue;
		XamlDomObject xamlDomObject = firstChild as XamlDomObject;
		string text = ((xamlDomValue != null) ? (xamlDomValue.Value as string) : xamlDomObject.Type.Name);
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_MissingCPA, domParentObject.Type.Name, text);
	}
}
