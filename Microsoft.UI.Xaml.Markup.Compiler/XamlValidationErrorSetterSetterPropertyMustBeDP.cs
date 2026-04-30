using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorSetterSetterPropertyMustBeDP : XamlCompileError
{
	public XamlValidationErrorSetterSetterPropertyMustBeDP(XamlDomMember domPropertyMember, string propertyName)
		: base(ErrorCode.WMC0095, domPropertyMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_SetterPropertyMustBeDP, propertyName);
	}
}
