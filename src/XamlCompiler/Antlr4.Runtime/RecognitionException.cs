using System;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

[Serializable]
public class RecognitionException : Exception
{
	private const long serialVersionUID = -3861826954750022374L;

	[Nullable]
	private readonly IRecognizer recognizer;

	[Nullable]
	private readonly RuleContext ctx;

	[Nullable]
	private readonly IIntStream input;

	private IToken offendingToken;

	private int offendingState = -1;

	public int OffendingState
	{
		get
		{
			return offendingState;
		}
		protected set
		{
			offendingState = value;
		}
	}

	public virtual RuleContext Context => ctx;

	public virtual IIntStream InputStream => input;

	public IToken OffendingToken
	{
		get
		{
			return offendingToken;
		}
		protected set
		{
			offendingToken = value;
		}
	}

	public virtual IRecognizer Recognizer => recognizer;

	public RecognitionException(Lexer lexer, ICharStream input)
	{
		recognizer = lexer;
		this.input = input;
		ctx = null;
	}

	public RecognitionException(IRecognizer recognizer, IIntStream input, ParserRuleContext ctx)
	{
		this.recognizer = recognizer;
		this.input = input;
		this.ctx = ctx;
		if (recognizer != null)
		{
			offendingState = recognizer.State;
		}
	}

	public RecognitionException(string message, IRecognizer recognizer, IIntStream input, ParserRuleContext ctx)
		: base(message)
	{
		this.recognizer = recognizer;
		this.input = input;
		this.ctx = ctx;
		if (recognizer != null)
		{
			offendingState = recognizer.State;
		}
	}

	[return: Nullable]
	public virtual IntervalSet GetExpectedTokens()
	{
		if (recognizer != null)
		{
			return recognizer.Atn.GetExpectedTokens(offendingState, ctx);
		}
		return null;
	}
}
