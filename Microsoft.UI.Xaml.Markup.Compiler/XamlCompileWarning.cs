using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlCompileWarning : XamlCompileErrorBase
{
	public XamlCompileWarning(ErrorCode code, IXamlDomNode domNode)
		: base(code, domNode.SourceFilePath, domNode.StartLineNumber, domNode.StartLinePosition)
	{
	}

	protected XamlCompileWarning(ErrorCode code)
		: base(code, null, 0, 0)
	{
	}
}
