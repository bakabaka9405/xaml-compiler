namespace Antlr4.Runtime.Atn;

public sealed class PlusBlockStartState : BlockStartState
{
	public PlusLoopbackState loopBackState;

	public override StateType StateType => StateType.PlusBlockStart;
}
