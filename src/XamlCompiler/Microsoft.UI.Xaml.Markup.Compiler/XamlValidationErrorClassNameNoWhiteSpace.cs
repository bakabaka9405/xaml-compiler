using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorClassNameNoWhiteSpace : XamlCompileError
{
	public XamlValidationErrorClassNameNoWhiteSpace(XamlDomMember domMember, string classname)
		: base(ErrorCode.WMC0132, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.Harvester_ClassNameNoWhiteSpace, classname);
	}
}
