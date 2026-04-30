using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindTwoWayBindingToANonDependencyPropertyError : XamlCompileError
{
	public XamlXBindTwoWayBindingToANonDependencyPropertyError(XamlDomMember domMember)
		: base(ErrorCode.WMC1118, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_TwoWayTargetNotADependencyProperty, domMember.Member.Name);
	}
}
