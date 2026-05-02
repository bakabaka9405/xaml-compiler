using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorUnresolvedForwardedTypeAssembly : XamlCompileError
{
	public XamlValidationErrorUnresolvedForwardedTypeAssembly(XamlDomMember domMember, string errorMessage)
		: base(ErrorCode.WMC0003, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnresolvedForwardedTypeAssembly, errorMessage);
	}

	public XamlValidationErrorUnresolvedForwardedTypeAssembly(XamlDomObject domObject, string errorMessage)
		: base(ErrorCode.WMC0003, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnresolvedForwardedTypeAssembly, errorMessage);
	}
}
