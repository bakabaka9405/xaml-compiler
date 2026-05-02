using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class EpsilonTransition : Transition
{
	private readonly int outermostPrecedenceReturn;

	public int OutermostPrecedenceReturn => outermostPrecedenceReturn;

	public override TransitionType TransitionType => TransitionType.Epsilon;

	public override bool IsEpsilon => true;

	public EpsilonTransition(ATNState target)
		: this(target, -1)
	{
	}

	public EpsilonTransition(ATNState target, int outermostPrecedenceReturn)
		: base(target)
	{
		this.outermostPrecedenceReturn = outermostPrecedenceReturn;
	}

	public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
	{
		return false;
	}

	[return: NotNull]
	public override string ToString()
	{
		return "epsilon";
	}
}
