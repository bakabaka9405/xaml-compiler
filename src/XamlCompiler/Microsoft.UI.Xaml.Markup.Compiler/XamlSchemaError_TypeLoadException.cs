using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlSchemaError_TypeLoadException : XamlCompileError
{
	public XamlSchemaError_TypeLoadException(string typeName, string asmName)
		: base(ErrorCode.WMC0805)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_TypeLoadException, typeName, asmName);
	}

	public XamlSchemaError_TypeLoadException(XamlDomObject domObject, string typeName, string innerMessage)
		: base(ErrorCode.WMC0806, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_TypeLoadExceptionMessage, typeName, innerMessage);
	}
}
