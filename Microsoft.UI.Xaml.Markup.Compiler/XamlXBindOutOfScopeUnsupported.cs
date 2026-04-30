using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXBindOutOfScopeUnsupported : XamlCompileError
{
	public XamlXBindOutOfScopeUnsupported(BindAssignment ba, string elementName, int namedElementLineNumber)
		: base(ErrorCode.WMC1124, ba.ConnectionIdElement.ParentFileCodeInfo.FullPathToXamlFile, ba.LineNumberInfo.StartLineNumber, ba.LineNumberInfo.StartLinePosition)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XBindOutOfScopeUnsupported, elementName, namedElementLineNumber);
	}
}
