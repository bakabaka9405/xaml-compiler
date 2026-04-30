namespace Antlr4.Runtime.Atn;

public class ErrorInfo : DecisionEventInfo
{
	public ErrorInfo(int decision, SimulatorState state, ITokenStream input, int startIndex, int stopIndex)
		: base(decision, state, input, startIndex, stopIndex, state.useContext)
	{
	}
}
