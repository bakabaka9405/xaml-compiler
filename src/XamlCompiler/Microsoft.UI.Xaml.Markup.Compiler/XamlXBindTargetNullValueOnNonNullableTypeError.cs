using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindTargetNullValueOnNonNullableTypeError : XamlCompileError
{
	public XamlXBindTargetNullValueOnNonNullableTypeError(XamlDomMember domMember)
		: base(ErrorCode.WMC1120, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindTargetNullValueOnNonNullableType, domMember.Member.Name, domMember.Member.Type.Name);
	}
}
