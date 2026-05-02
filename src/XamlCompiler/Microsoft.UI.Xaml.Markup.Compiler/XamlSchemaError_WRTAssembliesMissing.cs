using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlSchemaError_WRTAssembliesMissing : XamlCompileError
{
	public XamlSchemaError_WRTAssembliesMissing()
		: base(ErrorCode.WMC0815)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_WRTAssembliesMissing);
	}
}
