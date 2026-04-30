namespace Antlr4.Runtime.Atn;

public abstract class DecisionState : ATNState
{
	public int decision = -1;

	public bool nonGreedy;
}
