using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindRootNoLoadingEvent : XamlCompileError
{
	public XamlXBindRootNoLoadingEvent(XamlDomMember domMember, string rootElementType)
		: base(ErrorCode.WMC1125, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindRootMustHaveLoading, rootElementType);
	}
}
