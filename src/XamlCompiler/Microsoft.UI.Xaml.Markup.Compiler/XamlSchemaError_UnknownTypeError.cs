using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlSchemaError_UnknownTypeError : XamlCompileError
{
	public XamlSchemaError_UnknownTypeError(string typeName)
		: base(ErrorCode.WMC0822)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownTypeError, typeName);
	}
}
