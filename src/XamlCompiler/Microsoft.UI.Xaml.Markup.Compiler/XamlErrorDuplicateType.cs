using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlErrorDuplicateType : XamlCompileError
{
	public XamlErrorDuplicateType(string fullName)
		: base(ErrorCode.WMC0901)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DuplicateTypeName, fullName);
	}
}
