using Antlr4.Runtime;

namespace Microsoft.UI.Xaml.Markup.Compiler.Parsing;

internal class BailErrorStrategy : DefaultErrorStrategy
{
	public override void Recover(Parser recognizer, RecognitionException e)
	{
		throw e;
	}

	public override IToken RecoverInline(Parser recognizer)
	{
		throw new InputMismatchException(recognizer);
	}

	public override void Sync(Parser recognizer)
	{
	}
}
