using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorCannotAssignToReadOnlyProperty : XamlCompileError
{
	public XamlValidationErrorCannotAssignToReadOnlyProperty(XamlDomMember domMember)
		: base(ErrorCode.WMC0050, domMember)
	{
		XamlDomValue xamlDomValue = domMember.Items[0] as XamlDomValue;
		XamlDomObject xamlDomObject = domMember.Items[0] as XamlDomObject;
		string text = ((xamlDomValue != null) ? (xamlDomValue.Value as string) : xamlDomObject.Type.Name);
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAssignToReadOnlyProperty, text, domMember.Member.Name);
	}

	public XamlValidationErrorCannotAssignToReadOnlyProperty(XamlDomNode location, XamlMember member, string value)
		: base(ErrorCode.WMC0050, location)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAssignToReadOnlyProperty, value, member.Name);
	}
}
