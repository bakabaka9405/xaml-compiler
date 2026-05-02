using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class RangeTransition : Transition
{
	public readonly int from;

	public readonly int to;

	public override TransitionType TransitionType => TransitionType.Range;

	public override IntervalSet Label => IntervalSet.Of(from, to);

	public RangeTransition(ATNState target, int from, int to)
		: base(target)
	{
		this.from = from;
		this.to = to;
	}

	public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
	{
		if (symbol >= from)
		{
			return symbol <= to;
		}
		return false;
	}

	[return: NotNull]
	public override string ToString()
	{
		return "'" + (char)from + "'..'" + (char)to + "'";
	}
}
