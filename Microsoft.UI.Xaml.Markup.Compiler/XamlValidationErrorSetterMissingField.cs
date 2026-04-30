using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorSetterMissingField : XamlCompileError
{
	public XamlValidationErrorSetterMissingField(XamlDomNode setterOrProperty, bool isProperty)
		: base(ErrorCode.WMC0085, setterOrProperty)
	{
		if (isProperty)
		{
			base.Code = ErrorCode.WMC0085;
			base.Message = XamlCompilerResources.XamlCompiler_SettersMustHaveProperty;
		}
		else
		{
			base.Code = ErrorCode.WMC0086;
			base.Message = XamlCompilerResources.XamlCompiler_SetterMustHaveValue;
		}
	}
}
