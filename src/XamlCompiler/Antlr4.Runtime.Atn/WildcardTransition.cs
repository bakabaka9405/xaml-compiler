using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class WildcardTransition : Transition
{
	public override TransitionType TransitionType => TransitionType.Wildcard;

	public WildcardTransition(ATNState target)
		: base(target)
	{
	}

	public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
	{
		if (symbol >= minVocabSymbol)
		{
			return symbol <= maxVocabSymbol;
		}
		return false;
	}

	[return: NotNull]
	public override string ToString()
	{
		return ".";
	}
}
