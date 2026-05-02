namespace Antlr4.Runtime.Atn;

public sealed class LoopEndState : ATNState
{
	public ATNState loopBackState;

	public override StateType StateType => StateType.LoopEnd;
}
