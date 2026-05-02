namespace Microsoft.UI.Xaml.Markup.Compiler.Parsing;

internal static class ConditionalNamespaceLexerExtensions
{
	public static void ConfirmInputFullyConsumed(this ConditionalNamespaceLexer lexer)
	{
		if (!lexer.HitEOF)
		{
			string text = lexer.InputStream.ToString();
			string text2 = ((lexer.Token == null) ? text : text.Substring(lexer.Token.StartIndex));
			throw new ParseException(ErrorMessages.SyntaxError, text2);
		}
	}
}
