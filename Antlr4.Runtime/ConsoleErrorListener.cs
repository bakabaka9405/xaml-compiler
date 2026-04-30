using System;

namespace Antlr4.Runtime;

public class ConsoleErrorListener<Symbol> : IAntlrErrorListener<Symbol>
{
	public static readonly ConsoleErrorListener<Symbol> Instance = new ConsoleErrorListener<Symbol>();

	public virtual void SyntaxError(IRecognizer recognizer, Symbol offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
	{
		Console.Error.WriteLine("line " + line + ":" + charPositionInLine + " " + msg);
	}
}
