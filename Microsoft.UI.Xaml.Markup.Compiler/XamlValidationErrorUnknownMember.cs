using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorUnknownMember : XamlCompileError
{
	public XamlValidationErrorUnknownMember(XamlDomObject domObject, XamlDomMember domMember)
		: base(ErrorCode.WMC0010, domMember)
	{
		XamlMember member = domMember.Member;
		XamlType type = domMember.Parent.Type;
		XamlType declaringType = member.DeclaringType;
		if (declaringType != null && member.IsAttachable)
		{
			base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownAttachableMember, declaringType.Name, member.Name, type.Name);
		}
		else
		{
			base.Code = ErrorCode.WMC0011;
			base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownMember, member.Name, type.Name);
		}
	}
}
