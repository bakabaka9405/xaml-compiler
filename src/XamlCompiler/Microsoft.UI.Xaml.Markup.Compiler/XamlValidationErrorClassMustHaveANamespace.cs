using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorClassMustHaveANamespace : XamlCompileError
{
	public XamlValidationErrorClassMustHaveANamespace(XamlDomMember domMember, string classname)
		: base(ErrorCode.WMC0130, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_EventValuesMustBeText, classname);
	}
}
