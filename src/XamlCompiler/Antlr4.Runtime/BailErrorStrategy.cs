using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public class BailErrorStrategy : DefaultErrorStrategy
{
	public override void Recover(Parser recognizer, RecognitionException e)
	{
		for (ParserRuleContext parserRuleContext = recognizer.Context; parserRuleContext != null; parserRuleContext = (ParserRuleContext)parserRuleContext.Parent)
		{
			parserRuleContext.exception = e;
		}
		throw new ParseCanceledException(e);
	}

	public override IToken RecoverInline(Parser recognizer)
	{
		InputMismatchException ex = new InputMismatchException(recognizer);
		for (ParserRuleContext parserRuleContext = recognizer.Context; parserRuleContext != null; parserRuleContext = (ParserRuleContext)parserRuleContext.Parent)
		{
			parserRuleContext.exception = ex;
		}
		throw new ParseCanceledException(ex);
	}

	public override void Sync(Parser recognizer)
	{
	}
}
