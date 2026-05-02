using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class XamlCompileError : XamlCompileErrorBase
{
	protected XamlCompileError(ErrorCode code)
		: base(code, null, 0, 0)
	{
	}

	public XamlCompileError(ErrorCode code, IXamlDomNode domNode)
		: base(code, domNode?.SourceFilePath ?? null, domNode?.StartLineNumber ?? 0, domNode?.StartLinePosition ?? 0)
	{
	}

	public XamlCompileError(ErrorCode code, int lineNumber, int lineOffset)
		: base(code, null, lineNumber, lineOffset)
	{
	}

	protected XamlCompileError(ErrorCode code, string fileName, int lineNumber, int lineOffset)
		: base(code, fileName, lineNumber, lineOffset)
	{
	}
}
