using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorNonPublicType : XamlCompileError
{
	public XamlValidationErrorNonPublicType(XamlDomObject domObject)
		: base(ErrorCode.WMC0005, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAccessNonPublicType, domObject.Type.Name, domObject.Type.PreferredXamlNamespace);
	}
}
