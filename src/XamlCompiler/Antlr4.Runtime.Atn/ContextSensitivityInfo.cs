namespace Antlr4.Runtime.Atn;

public class ContextSensitivityInfo : DecisionEventInfo
{
	public ContextSensitivityInfo(int decision, SimulatorState state, ITokenStream input, int startIndex, int stopIndex)
		: base(decision, state, input, startIndex, stopIndex, fullCtx: true)
	{
	}
}
