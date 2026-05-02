using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorCannotAssignTextToProperty : XamlCompileError
{
	public XamlValidationErrorCannotAssignTextToProperty(XamlDomNode location, XamlMember member, string value)
		: base(ErrorCode.WMC0055, location)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAssignTextToProperty, value, member.Name, member.Type.Name);
	}
}
