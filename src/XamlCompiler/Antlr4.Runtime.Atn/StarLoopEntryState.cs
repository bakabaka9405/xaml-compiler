namespace Antlr4.Runtime.Atn;

public sealed class StarLoopEntryState : DecisionState
{
	public StarLoopbackState loopBackState;

	public bool precedenceRuleDecision;

	public override StateType StateType => StateType.StarLoopEntry;
}
