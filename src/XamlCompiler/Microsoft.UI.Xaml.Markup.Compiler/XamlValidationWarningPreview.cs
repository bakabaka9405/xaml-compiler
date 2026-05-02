using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationWarningPreview : XamlCompileWarning
{
	public XamlValidationWarningPreview(ErrorCode warningCode, string name)
		: base(warningCode)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_Preview, name);
	}
}
