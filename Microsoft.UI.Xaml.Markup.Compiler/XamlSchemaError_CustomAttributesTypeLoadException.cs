using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlSchemaError_CustomAttributesTypeLoadException : XamlCompileError
{
	public XamlSchemaError_CustomAttributesTypeLoadException(string asmName)
		: base(ErrorCode.WMC0810)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_CustomAttributesTypeLoadException, asmName);
	}
}
