using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorDuplicateAssigment : XamlCompileError
{
	public XamlValidationErrorDuplicateAssigment(XamlDomMember domMember)
		: base(ErrorCode.WMC0035, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DuplicationAssignment, domMember.Member.Name, domMember.Parent.Type.Name);
	}
}
