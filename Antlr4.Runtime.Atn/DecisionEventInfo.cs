using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public class DecisionEventInfo
{
	public readonly int decision;

	[Nullable]
	public readonly SimulatorState state;

	[NotNull]
	public readonly ITokenStream input;

	public readonly int startIndex;

	public readonly int stopIndex;

	public readonly bool fullCtx;

	public DecisionEventInfo(int decision, SimulatorState state, ITokenStream input, int startIndex, int stopIndex, bool fullCtx)
	{
		this.decision = decision;
		this.fullCtx = fullCtx;
		this.stopIndex = stopIndex;
		this.input = input;
		this.startIndex = startIndex;
		this.state = state;
	}
}
