using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorElementNameAlreadyUsed : XamlCompileError
{
	public XamlValidationErrorElementNameAlreadyUsed(XamlDomObject domObject, string duplicateName)
		: base(ErrorCode.WMC0047, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_ElementNameAlreadyUsed, duplicateName);
	}
}
