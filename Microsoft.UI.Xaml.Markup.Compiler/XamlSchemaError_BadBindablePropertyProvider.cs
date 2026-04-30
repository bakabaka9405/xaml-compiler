using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlSchemaError_BadBindablePropertyProvider : XamlCompileError
{
	public XamlSchemaError_BadBindablePropertyProvider(string typeName)
		: base(ErrorCode.WMC0800)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_BadBindablePropertyProvider, typeName);
	}
}
