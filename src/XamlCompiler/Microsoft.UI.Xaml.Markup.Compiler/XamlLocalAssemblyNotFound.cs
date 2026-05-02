using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlLocalAssemblyNotFound : XamlCompileWarning
{
	public XamlLocalAssemblyNotFound()
		: base(ErrorCode.WMC1509)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_LocalAssemblyMissingWarning);
	}
}
