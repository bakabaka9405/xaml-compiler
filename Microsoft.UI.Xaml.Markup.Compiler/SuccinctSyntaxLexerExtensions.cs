using SuccinctCollectionSyntax;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal static class SuccinctSyntaxLexerExtensions
{
	public static void ConfirmInputFullyConsumed(this SuccinctCollectionSyntaxLexer lexer)
	{
		if (!lexer.HitEOF)
		{
			string text = lexer.InputStream.ToString();
			string offendingSymbolText = ((lexer.Token == null) ? text : text.Substring(lexer.Token.StartIndex));
			throw new SuccinctCollectionSyntaxException(ErrorMessages.SyntaxError, offendingSymbolText);
		}
	}
}
