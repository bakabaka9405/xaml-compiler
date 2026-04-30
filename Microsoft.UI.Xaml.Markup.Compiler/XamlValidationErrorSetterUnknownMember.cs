using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorSetterUnknownMember : XamlCompileError
{
	public XamlValidationErrorSetterUnknownMember(XamlDomMember domPropertyMember, XamlType xamlTargetType, string propertyName)
		: base(ErrorCode.WMC0090, domPropertyMember)
	{
		if (propertyName.IndexOf('.') == -1)
		{
			base.Code = ErrorCode.WMC0090;
			base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownMember, propertyName, xamlTargetType.Name);
		}
		else
		{
			base.Code = ErrorCode.WMC0091;
			base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownSetterAttachableMember, propertyName, xamlTargetType.Name);
		}
	}
}
