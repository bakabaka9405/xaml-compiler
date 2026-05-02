namespace Microsoft.UI.Xaml.Markup.Compiler;

public class XamlCompileErrorBase
{
	public ErrorCode Code { get; protected set; }

	public string Message { get; protected set; }

	public string FileName { get; protected set; }

	public int LineNumber { get; private set; }

	public int LineOffset { get; private set; }

	public XamlCompileErrorBase(ErrorCode code, string fileName, int lineNumber, int lineOffset)
	{
		Code = code;
		FileName = fileName;
		LineNumber = lineNumber;
		LineOffset = lineOffset;
	}
}
