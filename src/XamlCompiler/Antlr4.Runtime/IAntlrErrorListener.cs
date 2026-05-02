namespace Antlr4.Runtime;

public interface IAntlrErrorListener<in TSymbol>
{
	void SyntaxError(IRecognizer recognizer, TSymbol offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e);
}
