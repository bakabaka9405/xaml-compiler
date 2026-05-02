namespace Antlr4.Runtime.Atn;

public sealed class StarLoopbackState : ATNState
{
	public StarLoopEntryState LoopEntryState => (StarLoopEntryState)Transition(0).target;

	public override StateType StateType => StateType.StarLoopBack;
}
