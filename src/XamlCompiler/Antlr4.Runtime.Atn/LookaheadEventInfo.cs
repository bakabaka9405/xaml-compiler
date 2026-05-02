namespace Antlr4.Runtime.Atn;

public class LookaheadEventInfo : DecisionEventInfo
{
	public LookaheadEventInfo(int decision, SimulatorState state, ITokenStream input, int startIndex, int stopIndex, bool fullCtx)
		: base(decision, state, input, startIndex, stopIndex, fullCtx)
	{
	}
}
