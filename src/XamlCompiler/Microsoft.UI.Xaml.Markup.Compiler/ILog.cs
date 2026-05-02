namespace Microsoft.UI.Xaml.Markup.Compiler;

public interface ILog
{
	void LogError(string subcategory, string errorCode, string helpKeyword, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message);

	void LogWarning(string subcategory, string warningCode, string helpKeyword, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message);

	void LogDiagnosticMessage(string message);
}
