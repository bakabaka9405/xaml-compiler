using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public class SetTransition : Transition
{
	[NotNull]
	public readonly IntervalSet set;

	public override TransitionType TransitionType => TransitionType.Set;

	public override IntervalSet Label => set;

	public SetTransition(ATNState target, IntervalSet set)
		: base(target)
	{
		if (set == null)
		{
			set = IntervalSet.Of(0);
		}
		this.set = set;
	}

	public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
	{
		return set.Contains(symbol);
	}

	[return: NotNull]
	public override string ToString()
	{
		return set.ToString();
	}
}
