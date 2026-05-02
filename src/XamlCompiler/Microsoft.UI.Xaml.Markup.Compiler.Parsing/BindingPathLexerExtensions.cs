namespace Microsoft.UI.Xaml.Markup.Compiler.Parsing;

internal static class BindingPathLexerExtensions
{
	public static void ConfirmInputFullyConsumed(this BindingPathLexer lexer)
	{
		if (!lexer.HitEOF)
		{
			string text = lexer.InputStream.ToString();
			string text2 = ((lexer.Token == null) ? text : text.Substring(lexer.Token.StartIndex));
			throw new ParseException(ErrorMessages.SyntaxError, text2);
		}
	}
}
