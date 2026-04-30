using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class ErrorXPropertyUsageNotSupported : XamlCompileError
{
	public ErrorXPropertyUsageNotSupported(XamlDomObject domObject, Language language)
		: base(ErrorCode.WMC0505, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XPropertyUsageNotSupportedForLanguage, language.Name);
	}
}
