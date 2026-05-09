using System.IO;
using Antlr4.Runtime;

namespace Microsoft.UI.Xaml.Markup.Compiler.Parsing;

internal class ParseErrorListener : BaseErrorListener
{
	public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
	{
		throw new ParseException(ErrorMessages.SyntaxError, offendingSymbol.Text);
	}
}
