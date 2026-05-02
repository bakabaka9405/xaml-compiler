using Antlr4.Runtime.Dfa;

namespace Antlr4.Runtime.Atn;

public class SimulatorState
{
	public readonly ParserRuleContext outerContext;

	public readonly DFAState s0;

	public readonly bool useContext;

	public readonly ParserRuleContext remainingOuterContext;

	public SimulatorState(ParserRuleContext outerContext, DFAState s0, bool useContext, ParserRuleContext remainingOuterContext)
	{
		this.outerContext = ((outerContext != null) ? outerContext : ParserRuleContext.EmptyContext);
		this.s0 = s0;
		this.useContext = useContext;
		this.remainingOuterContext = remainingOuterContext;
	}
}
