namespace Antlr4.Runtime.Atn;

public sealed class PrecedencePredicateTransition : AbstractPredicateTransition
{
	public readonly int precedence;

	public override TransitionType TransitionType => TransitionType.Precedence;

	public override bool IsEpsilon => true;

	public SemanticContext.PrecedencePredicate Predicate => new SemanticContext.PrecedencePredicate(precedence);

	public PrecedencePredicateTransition(ATNState target, int precedence)
		: base(target)
	{
		this.precedence = precedence;
	}

	public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
	{
		return false;
	}

	public override string ToString()
	{
		return precedence + " >= _p";
	}
}
