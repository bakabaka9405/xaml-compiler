using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class NotSetTransition : SetTransition
{
	public override TransitionType TransitionType => TransitionType.NotSet;

	public NotSetTransition(ATNState target, IntervalSet set)
		: base(target, set)
	{
	}

	public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
	{
		if (symbol >= minVocabSymbol && symbol <= maxVocabSymbol)
		{
			return !base.Matches(symbol, minVocabSymbol, maxVocabSymbol);
		}
		return false;
	}

	public override string ToString()
	{
		return "~" + base.ToString();
	}
}
