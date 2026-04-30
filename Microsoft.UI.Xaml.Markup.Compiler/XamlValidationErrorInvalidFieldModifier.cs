using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorInvalidFieldModifier : XamlCompileError
{
	public XamlValidationErrorInvalidFieldModifier(XamlDomObject domObject, string invalidModifier)
		: base(ErrorCode.WMC0905, domObject)
	{
		XamlDomMember aliasedMemberNode = DomHelper.GetAliasedMemberNode(domObject, XamlLanguage.Name);
		if (aliasedMemberNode == null)
		{
			base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidFieldModifier, invalidModifier, domObject.Type.Name);
		}
		else
		{
			base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_InvalidFieldModifier, invalidModifier, DomHelper.GetStringValueOfProperty(aliasedMemberNode));
		}
	}
}
