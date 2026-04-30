using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindParseWarning : XamlCompileWarning
{
	public XamlXBindParseWarning(XamlDomObject domObject, string message)
		: base(ErrorCode.WMC1507, domObject)
	{
		base.Message = message;
	}
}
