namespace Antlr4.Runtime.Atn;

public class AmbiguityInfo : DecisionEventInfo
{
	public AmbiguityInfo(int decision, SimulatorState state, ITokenStream input, int startIndex, int stopIndex)
		: base(decision, state, input, startIndex, stopIndex, state.useContext)
	{
	}
}
