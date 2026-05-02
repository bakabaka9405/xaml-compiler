using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorNotConstructibleObject : XamlCompileError
{
	public XamlValidationErrorNotConstructibleObject(XamlDomObject domObject, XamlType xamlType)
		: base(ErrorCode.WMC0100, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_NotConstructibleObj, xamlType.Name);
	}
}
