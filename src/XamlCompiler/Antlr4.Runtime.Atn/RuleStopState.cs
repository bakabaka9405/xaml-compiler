namespace Antlr4.Runtime.Atn;

public sealed class RuleStopState : ATNState
{
	public override int NonStopStateNumber => -1;

	public override StateType StateType => StateType.RuleStop;
}
