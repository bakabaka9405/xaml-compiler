namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class CompiledBindingParseException : CompiledBindingException
{
	public string ExpressionBeingParsed { get; }

	public CompiledBindingParseException(string expressionBeingParsed, string exceptionMessage, int startCharacterPosition)
		: base(exceptionMessage, startCharacterPosition)
	{
		ExpressionBeingParsed = expressionBeingParsed;
	}
}
