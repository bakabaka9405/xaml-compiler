using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class AtomTransition : Transition
{
	public readonly int token;

	public override TransitionType TransitionType => TransitionType.Atom;

	public override IntervalSet Label => IntervalSet.Of(token);

	public AtomTransition(ATNState target, int token)
		: base(target)
	{
		this.token = token;
	}

	public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
	{
		return token == symbol;
	}

	[return: NotNull]
	public override string ToString()
	{
		return token.ToString();
	}
}
