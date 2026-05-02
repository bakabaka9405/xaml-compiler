using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationCreateFromStringError : XamlCompileError
{
	public XamlValidationCreateFromStringError(string typeName, string createFromStringMethodName, string message, XamlDomNode locationForErrors)
		: base(ErrorCode.WMC0915, locationForErrors)
	{
		base.Message = string.Format(message, createFromStringMethodName, typeName);
	}
}
