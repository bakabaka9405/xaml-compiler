using System.Collections.Generic;

namespace Antlr4.Runtime.Atn;

public class DecisionInfo
{
	public readonly int decision;

	public long invocations;

	public long SLL_TotalLook;

	public long SLL_MinLook;

	public long SLL_MaxLook;

	public LookaheadEventInfo SLL_MaxLookEvent;

	public long LL_TotalLook;

	public long LL_MinLook;

	public long LL_MaxLook;

	public LookaheadEventInfo LL_MaxLookEvent;

	public readonly IList<ContextSensitivityInfo> contextSensitivities = new List<ContextSensitivityInfo>();

	public readonly IList<ErrorInfo> errors = new List<ErrorInfo>();

	public readonly IList<AmbiguityInfo> ambiguities = new List<AmbiguityInfo>();

	public readonly IList<PredicateEvalInfo> predicateEvals = new List<PredicateEvalInfo>();

	public long SLL_ATNTransitions;

	public long SLL_DFATransitions;

	public long LL_Fallback;

	public long LL_ATNTransitions;

	public long LL_DFATransitions;

	public DecisionInfo(int decision)
	{
		this.decision = decision;
	}

	public override string ToString()
	{
		return "{decision=" + decision + ", contextSensitivities=" + contextSensitivities.Count + ", errors=" + errors.Count + ", ambiguities=" + ambiguities.Count + ", SLL_lookahead=" + SLL_TotalLook + ", SLL_ATNTransitions=" + SLL_ATNTransitions + ", SLL_DFATransitions=" + SLL_DFATransitions + ", LL_Fallback=" + LL_Fallback + ", LL_lookahead=" + LL_TotalLook + ", LL_ATNTransitions=" + LL_ATNTransitions + "}";
	}
}
