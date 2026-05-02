using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorStyleBasedOnBadStyleTargetType : XamlCompileError
{
	public XamlValidationErrorStyleBasedOnBadStyleTargetType(XamlDomNode styleObject, XamlType targetType, XamlType basedOnTargetType)
		: base(ErrorCode.WMC0145, styleObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_StyleBasedOnBadStyleTargetType, targetType.Name, basedOnTargetType.Name);
	}
}
