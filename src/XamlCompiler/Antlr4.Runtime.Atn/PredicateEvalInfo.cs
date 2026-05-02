namespace Antlr4.Runtime.Atn;

public class PredicateEvalInfo : DecisionEventInfo
{
	public readonly SemanticContext semctx;

	public readonly int predictedAlt;

	public readonly bool evalResult;

	public PredicateEvalInfo(SimulatorState state, int decision, ITokenStream input, int startIndex, int stopIndex, SemanticContext semctx, bool evalResult, int predictedAlt)
		: base(decision, state, input, startIndex, stopIndex, state.useContext)
	{
		this.semctx = semctx;
		this.evalResult = evalResult;
		this.predictedAlt = predictedAlt;
	}
}
