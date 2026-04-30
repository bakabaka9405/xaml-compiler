using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlCompilerTypeMustHaveANamespace : XamlCompileError
{
	public XamlCompilerTypeMustHaveANamespace(XamlDomObject domObject)
		: base(ErrorCode.WMC0105, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_TypeMustHaveANamespace, domObject.Type.Name);
	}
}
