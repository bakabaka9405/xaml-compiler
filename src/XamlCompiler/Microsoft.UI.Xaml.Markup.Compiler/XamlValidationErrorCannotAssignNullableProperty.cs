using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorCannotAssignNullableProperty : XamlCompileError
{
	public XamlValidationErrorCannotAssignNullableProperty(XamlDomNode location, XamlMember member)
		: base(ErrorCode.WMC0056, location)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_NullablePropertyType, member.Name);
	}
}
