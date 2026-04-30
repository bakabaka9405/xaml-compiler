using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationError_LoadNotSupported : XamlCompileError
{
	public XamlValidationError_LoadNotSupported(XamlDomMember domMember)
		: base(ErrorCode.WMC0913, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_LoadNotSupported);
	}
}
