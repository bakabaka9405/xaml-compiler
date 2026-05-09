using System.IO;
using Antlr4.Runtime;

namespace SuccinctCollectionSyntax;

public class SuccinctCollectionSyntaxErrorListener : BaseErrorListener
{
	public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
	{
		throw new SuccinctCollectionSyntaxException("Offending Symbol Text: {0}, at line Number {1} , character position : {2}", offendingSymbol.Text, line, charPositionInLine);
	}
}
