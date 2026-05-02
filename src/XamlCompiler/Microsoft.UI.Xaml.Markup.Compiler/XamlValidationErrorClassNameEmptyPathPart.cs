using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorClassNameEmptyPathPart : XamlCompileError
{
	public XamlValidationErrorClassNameEmptyPathPart(XamlDomMember domMember, string classname)
		: base(ErrorCode.WMC0131, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.Harvester_ClassNameEmptyPathPart, classname);
	}
}
