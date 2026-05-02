using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class PredicateTransition : AbstractPredicateTransition
{
	public readonly int ruleIndex;

	public readonly int predIndex;

	public readonly bool isCtxDependent;

	public override TransitionType TransitionType => TransitionType.Predicate;

	public override bool IsEpsilon => true;

	public SemanticContext.Predicate Predicate => new SemanticContext.Predicate(ruleIndex, predIndex, isCtxDependent);

	public PredicateTransition(ATNState target, int ruleIndex, int predIndex, bool isCtxDependent)
		: base(target)
	{
		this.ruleIndex = ruleIndex;
		this.predIndex = predIndex;
		this.isCtxDependent = isCtxDependent;
	}

	public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
	{
		return false;
	}

	[return: NotNull]
	public override string ToString()
	{
		return "pred_" + ruleIndex + ":" + predIndex;
	}
}
