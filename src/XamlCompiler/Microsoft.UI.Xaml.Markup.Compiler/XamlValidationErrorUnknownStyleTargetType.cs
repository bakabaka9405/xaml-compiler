using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorUnknownStyleTargetType : XamlCompileError
{
	public XamlValidationErrorUnknownStyleTargetType(XamlDomMember targetTypeMember, string typeName)
		: base(ErrorCode.WMC0110, targetTypeMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownStyleTargetType, typeName);
	}
}
