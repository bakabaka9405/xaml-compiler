namespace Antlr4.Runtime.Atn;

public sealed class RuleStartState : ATNState
{
	public RuleStopState stopState;

	public bool isPrecedenceRule;

	public override StateType StateType => StateType.RuleStart;
}
