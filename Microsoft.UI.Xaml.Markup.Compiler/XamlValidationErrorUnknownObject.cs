using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorUnknownObject : XamlCompileError
{
	public XamlValidationErrorUnknownObject(XamlDomObject domObject)
		: base(ErrorCode.WMC0001, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_UnknownObject, domObject.Type.Name, domObject.Type.PreferredXamlNamespace);
	}
}
