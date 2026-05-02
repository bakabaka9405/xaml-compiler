using System;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public class ProfilingATNSimulator : ParserATNSimulator
{
	protected internal readonly DecisionInfo[] decisions;

	protected internal int numDecisions;

	private ITokenStream _input;

	private int _startIndex;

	private int _sllStopIndex;

	private int _llStopIndex;

	protected internal int currentDecision;

	protected internal SimulatorState currentState;

	protected internal int conflictingAltResolvedBySLL;

	public virtual DecisionInfo[] DecisionInfo => decisions;

	public ProfilingATNSimulator(Parser parser)
		: base(parser, parser.Interpreter.atn)
	{
		optimize_ll1 = false;
		reportAmbiguities = true;
		numDecisions = atn.decisionToState.Count;
		decisions = new DecisionInfo[numDecisions];
		for (int i = 0; i < numDecisions; i++)
		{
			decisions[i] = new DecisionInfo(i);
		}
	}

	public override int AdaptivePredict(ITokenStream input, int decision, ParserRuleContext outerContext)
	{
		try
		{
			_input = input;
			_startIndex = input.Index;
			_sllStopIndex = _startIndex - 1;
			_llStopIndex = -1;
			currentDecision = decision;
			currentState = null;
			conflictingAltResolvedBySLL = 0;
			int result = base.AdaptivePredict(input, decision, outerContext);
			decisions[decision].invocations++;
			int num = _sllStopIndex - _startIndex + 1;
			decisions[decision].SLL_TotalLook += num;
			decisions[decision].SLL_MinLook = ((decisions[decision].SLL_MinLook == 0L) ? num : Math.Min(decisions[decision].SLL_MinLook, num));
			if (num > decisions[decision].SLL_MaxLook)
			{
				decisions[decision].SLL_MaxLook = num;
				decisions[decision].SLL_MaxLookEvent = new LookaheadEventInfo(decision, null, input, _startIndex, _sllStopIndex, fullCtx: false);
			}
			if (_llStopIndex >= 0)
			{
				int num2 = _llStopIndex - _startIndex + 1;
				decisions[decision].LL_TotalLook += num2;
				decisions[decision].LL_MinLook = ((decisions[decision].LL_MinLook == 0L) ? num2 : Math.Min(decisions[decision].LL_MinLook, num2));
				if (num2 > decisions[decision].LL_MaxLook)
				{
					decisions[decision].LL_MaxLook = num2;
					decisions[decision].LL_MaxLookEvent = new LookaheadEventInfo(decision, null, input, _startIndex, _llStopIndex, fullCtx: true);
				}
			}
			return result;
		}
		finally
		{
			_input = null;
			currentDecision = -1;
		}
	}

	protected internal override SimulatorState GetStartState(DFA dfa, ITokenStream input, ParserRuleContext outerContext, bool useContext)
	{
		return currentState = base.GetStartState(dfa, input, outerContext, useContext);
	}

	protected internal override SimulatorState ComputeStartState(DFA dfa, ParserRuleContext globalContext, bool useContext)
	{
		return currentState = base.ComputeStartState(dfa, globalContext, useContext);
	}

	protected internal override SimulatorState ComputeReachSet(DFA dfa, SimulatorState previous, int t, PredictionContextCache contextCache)
	{
		SimulatorState simulatorState = base.ComputeReachSet(dfa, previous, t, contextCache);
		if (simulatorState == null)
		{
			decisions[currentDecision].errors.Add(new ErrorInfo(currentDecision, previous, _input, _startIndex, _input.Index));
		}
		currentState = simulatorState;
		return simulatorState;
	}

	protected internal override DFAState GetExistingTargetState(DFAState previousD, int t)
	{
		if (currentState.useContext)
		{
			_llStopIndex = _input.Index;
		}
		else
		{
			_sllStopIndex = _input.Index;
		}
		DFAState existingTargetState = base.GetExistingTargetState(previousD, t);
		if (existingTargetState != null)
		{
			currentState = new SimulatorState(currentState.outerContext, existingTargetState, currentState.useContext, currentState.remainingOuterContext);
			if (currentState.useContext)
			{
				decisions[currentDecision].LL_DFATransitions++;
			}
			else
			{
				decisions[currentDecision].SLL_DFATransitions++;
			}
			if (existingTargetState == ATNSimulator.Error)
			{
				SimulatorState state = new SimulatorState(currentState.outerContext, previousD, currentState.useContext, currentState.remainingOuterContext);
				decisions[currentDecision].errors.Add(new ErrorInfo(currentDecision, state, _input, _startIndex, _input.Index));
			}
		}
		return existingTargetState;
	}

	protected internal override Tuple<DFAState, ParserRuleContext> ComputeTargetState(DFA dfa, DFAState s, ParserRuleContext remainingGlobalContext, int t, bool useContext, PredictionContextCache contextCache)
	{
		Tuple<DFAState, ParserRuleContext> result = base.ComputeTargetState(dfa, s, remainingGlobalContext, t, useContext, contextCache);
		if (useContext)
		{
			decisions[currentDecision].LL_ATNTransitions++;
		}
		else
		{
			decisions[currentDecision].SLL_ATNTransitions++;
		}
		return result;
	}

	protected internal override bool EvalSemanticContext(SemanticContext pred, ParserRuleContext parserCallStack, int alt)
	{
		bool flag = base.EvalSemanticContext(pred, parserCallStack, alt);
		if (!(pred is SemanticContext.PrecedencePredicate))
		{
			int stopIndex = ((_llStopIndex >= 0) ? _llStopIndex : _sllStopIndex);
			decisions[currentDecision].predicateEvals.Add(new PredicateEvalInfo(currentState, currentDecision, _input, _startIndex, stopIndex, pred, flag, alt));
		}
		return flag;
	}

	protected internal override void ReportContextSensitivity(DFA dfa, int prediction, SimulatorState acceptState, int startIndex, int stopIndex)
	{
		if (prediction != conflictingAltResolvedBySLL)
		{
			decisions[currentDecision].contextSensitivities.Add(new ContextSensitivityInfo(currentDecision, acceptState, _input, startIndex, stopIndex));
		}
		base.ReportContextSensitivity(dfa, prediction, acceptState, startIndex, stopIndex);
	}

	protected internal override void ReportAttemptingFullContext(DFA dfa, BitSet conflictingAlts, SimulatorState conflictState, int startIndex, int stopIndex)
	{
		if (conflictingAlts != null)
		{
			conflictingAltResolvedBySLL = conflictingAlts.NextSetBit(0);
		}
		else
		{
			conflictingAltResolvedBySLL = conflictState.s0.configs.RepresentedAlternatives.NextSetBit(0);
		}
		decisions[currentDecision].LL_Fallback++;
		base.ReportAttemptingFullContext(dfa, conflictingAlts, conflictState, startIndex, stopIndex);
	}

	protected internal override void ReportAmbiguity(DFA dfa, DFAState D, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
	{
		int num = ambigAlts?.NextSetBit(0) ?? configs.RepresentedAlternatives.NextSetBit(0);
		if (conflictingAltResolvedBySLL != 0 && num != conflictingAltResolvedBySLL)
		{
			decisions[currentDecision].contextSensitivities.Add(new ContextSensitivityInfo(currentDecision, currentState, _input, startIndex, stopIndex));
		}
		decisions[currentDecision].ambiguities.Add(new AmbiguityInfo(currentDecision, currentState, _input, startIndex, stopIndex));
		base.ReportAmbiguity(dfa, D, startIndex, stopIndex, exact, ambigAlts, configs);
	}
}
