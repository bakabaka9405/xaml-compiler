using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlSchemaError_BindableNotSupportedOnGeneric : XamlCompileError
{
	public XamlSchemaError_BindableNotSupportedOnGeneric(string typeName)
		: base(ErrorCode.WMC0821)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_BindableNotSupportedOnGeneric, typeName);
	}
}
