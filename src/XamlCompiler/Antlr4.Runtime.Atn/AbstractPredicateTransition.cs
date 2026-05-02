namespace Antlr4.Runtime.Atn;

public abstract class AbstractPredicateTransition : Transition
{
	public AbstractPredicateTransition(ATNState target)
		: base(target)
	{
	}
}
