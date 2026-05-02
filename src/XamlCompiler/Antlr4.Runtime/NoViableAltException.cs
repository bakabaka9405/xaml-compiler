using System;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

[Serializable]
public class NoViableAltException : RecognitionException
{
	private const long serialVersionUID = 5096000008992867052L;

	[Nullable]
	private readonly ATNConfigSet deadEndConfigs;

	[NotNull]
	private readonly IToken startToken;

	public virtual IToken StartToken => startToken;

	[Nullable]
	public virtual ATNConfigSet DeadEndConfigs => deadEndConfigs;

	public NoViableAltException(Parser recognizer)
		: this(recognizer, (ITokenStream)recognizer.InputStream, recognizer.CurrentToken, recognizer.CurrentToken, null, recognizer.RuleContext)
	{
	}

	public NoViableAltException(IRecognizer recognizer, ITokenStream input, IToken startToken, IToken offendingToken, ATNConfigSet deadEndConfigs, ParserRuleContext ctx)
		: base(recognizer, input, ctx)
	{
		this.deadEndConfigs = deadEndConfigs;
		this.startToken = startToken;
		base.OffendingToken = offendingToken;
	}
}
