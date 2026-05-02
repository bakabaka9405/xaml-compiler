namespace Antlr4.Runtime.Atn;

public sealed class BlockEndState : ATNState
{
	public BlockStartState startState;

	public override StateType StateType => StateType.BlockEnd;
}
