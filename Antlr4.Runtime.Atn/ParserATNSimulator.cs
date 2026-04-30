using System;
using System.Collections.Generic;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public class ParserATNSimulator : ATNSimulator
{
	private sealed class _IComparer_1996 : IComparer<ATNConfig>
	{
		public int Compare(ATNConfig o1, ATNConfig o2)
		{
			int num = o1.State.NonStopStateNumber - o2.State.NonStopStateNumber;
			if (num != 0)
			{
				return num;
			}
			num = o1.Alt - o2.Alt;
			if (num != 0)
			{
				return num;
			}
			return 0;
		}
	}

	public const bool debug = false;

	public const bool dfa_debug = false;

	public const bool retry_debug = false;

	[NotNull]
	private PredictionMode predictionMode = PredictionMode.Ll;

	public bool force_global_context;

	public bool always_try_local_context = true;

	public bool enable_global_context_dfa;

	public bool optimize_unique_closure = true;

	public bool optimize_ll1 = true;

	[Obsolete("This flag is not currently used by the ATN simulator.")]
	public bool optimize_hidden_conflicted_configs;

	public bool optimize_tail_calls = true;

	public bool tail_call_preserves_sll = true;

	public bool treat_sllk1_conflict_as_ambiguity;

	[Nullable]
	private readonly Parser _parser;

	public bool reportAmbiguities;

	protected internal bool userWantsCtxSensitive = true;

	private DFA dfa;

	private static readonly IComparer<ATNConfig> StateAltSortComparator = new _IComparer_1996();

	public PredictionMode PredictionMode
	{
		get
		{
			return predictionMode;
		}
		set
		{
			predictionMode = value;
		}
	}

	public virtual Parser Parser => _parser;

	public ParserATNSimulator(ATN atn)
		: this(null, atn)
	{
	}

	public ParserATNSimulator(Parser parser, ATN atn)
		: base(atn)
	{
		_parser = parser;
	}

	public override void Reset()
	{
	}

	public virtual int AdaptivePredict(ITokenStream input, int decision, ParserRuleContext outerContext)
	{
		return AdaptivePredict(input, decision, outerContext, useContext: false);
	}

	public virtual int AdaptivePredict(ITokenStream input, int decision, ParserRuleContext outerContext, bool useContext)
	{
		DFA dFA = atn.decisionToDFA[decision];
		if (optimize_ll1 && !dFA.IsPrecedenceDfa && !dFA.IsEmpty)
		{
			int num = input.La(1);
			if (num >= 0 && num <= 32767)
			{
				int key = (decision << 16) + num;
				if (atn.LL1Table.TryGetValue(key, out var value))
				{
					return value;
				}
			}
		}
		dfa = dFA;
		if (force_global_context)
		{
			useContext = true;
		}
		else if (!always_try_local_context)
		{
			useContext |= dFA.IsContextSensitive;
		}
		userWantsCtxSensitive = useContext || (predictionMode != PredictionMode.Sll && outerContext != null);
		if (outerContext == null)
		{
			outerContext = ParserRuleContext.EmptyContext;
		}
		SimulatorState simulatorState = null;
		if (!dFA.IsEmpty)
		{
			simulatorState = GetStartState(dFA, input, outerContext, useContext);
		}
		if (simulatorState == null)
		{
			if (outerContext == null)
			{
				outerContext = ParserRuleContext.EmptyContext;
			}
			simulatorState = ComputeStartState(dFA, outerContext, useContext);
		}
		int marker = input.Mark();
		int index = input.Index;
		try
		{
			return ExecDFA(dFA, input, index, simulatorState);
		}
		finally
		{
			dfa = null;
			input.Seek(index);
			input.Release(marker);
		}
	}

	protected internal virtual SimulatorState GetStartState(DFA dfa, ITokenStream input, ParserRuleContext outerContext, bool useContext)
	{
		if (!useContext)
		{
			if (dfa.IsPrecedenceDfa)
			{
				DFAState precedenceStartState = dfa.GetPrecedenceStartState(_parser.Precedence, fullContext: false);
				if (precedenceStartState == null)
				{
					return null;
				}
				return new SimulatorState(outerContext, precedenceStartState, useContext: false, outerContext);
			}
			if (dfa.s0.Get() == null)
			{
				return null;
			}
			return new SimulatorState(outerContext, dfa.s0.Get(), useContext: false, outerContext);
		}
		if (!enable_global_context_dfa)
		{
			return null;
		}
		ParserRuleContext parserRuleContext = outerContext;
		DFAState dFAState = ((!dfa.IsPrecedenceDfa) ? dfa.s0full.Get() : dfa.GetPrecedenceStartState(_parser.Precedence, fullContext: true));
		while (parserRuleContext != null && dFAState != null && dFAState.IsContextSensitive)
		{
			parserRuleContext = SkipTailCalls(parserRuleContext);
			dFAState = dFAState.GetContextTarget(GetReturnState(parserRuleContext));
			if (!parserRuleContext.IsEmpty)
			{
				parserRuleContext = (ParserRuleContext)parserRuleContext.Parent;
			}
		}
		if (dFAState == null)
		{
			return null;
		}
		return new SimulatorState(outerContext, dFAState, useContext, parserRuleContext);
	}

	protected internal virtual int ExecDFA(DFA dfa, ITokenStream input, int startIndex, SimulatorState state)
	{
		ParserRuleContext outerContext = state.outerContext;
		DFAState dFAState = state.s0;
		int num = input.La(1);
		ParserRuleContext parserRuleContext = state.remainingOuterContext;
		while (true)
		{
			if (state.useContext)
			{
				while (dFAState.IsContextSymbol(num))
				{
					DFAState dFAState2 = null;
					if (parserRuleContext != null)
					{
						parserRuleContext = SkipTailCalls(parserRuleContext);
						dFAState2 = dFAState.GetContextTarget(GetReturnState(parserRuleContext));
					}
					if (dFAState2 == null)
					{
						SimulatorState initialState = new SimulatorState(state.outerContext, dFAState, state.useContext, parserRuleContext);
						return ExecATN(dfa, input, startIndex, initialState);
					}
					parserRuleContext = (ParserRuleContext)parserRuleContext.Parent;
					dFAState = dFAState2;
				}
			}
			if (IsAcceptState(dFAState, state.useContext))
			{
				if (dFAState.predicates == null)
				{
				}
				break;
			}
			DFAState existingTargetState = GetExistingTargetState(dFAState, num);
			if (existingTargetState == null)
			{
				SimulatorState initialState2 = new SimulatorState(outerContext, dFAState, state.useContext, parserRuleContext);
				return ExecATN(dfa, input, startIndex, initialState2);
			}
			if (existingTargetState == ATNSimulator.Error)
			{
				SimulatorState previous = new SimulatorState(outerContext, dFAState, state.useContext, parserRuleContext);
				return HandleNoViableAlt(input, startIndex, previous);
			}
			dFAState = existingTargetState;
			if (!IsAcceptState(dFAState, state.useContext) && num != -1)
			{
				input.Consume();
				num = input.La(1);
			}
		}
		if (!state.useContext && dFAState.configs.ConflictInformation != null && dfa.atnStartState is DecisionState && userWantsCtxSensitive && (dFAState.configs.DipsIntoOuterContext || !dFAState.configs.IsExactConflict) && (!treat_sllk1_conflict_as_ambiguity || input.Index != startIndex))
		{
			BitSet bitSet = null;
			DFAState.PredPrediction[] predicates = dFAState.predicates;
			if (predicates != null)
			{
				int index = input.Index;
				if (index != startIndex)
				{
					input.Seek(startIndex);
				}
				bitSet = EvalSemanticContext(predicates, outerContext, complete: true);
				if (bitSet.Cardinality() == 1)
				{
					return bitSet.NextSetBit(0);
				}
				if (index != startIndex)
				{
					input.Seek(index);
				}
			}
			if (reportAmbiguities)
			{
				SimulatorState conflictState = new SimulatorState(outerContext, dFAState, state.useContext, parserRuleContext);
				ReportAttemptingFullContext(dfa, bitSet, conflictState, startIndex, input.Index);
			}
			input.Seek(startIndex);
			return AdaptivePredict(input, dfa.decision, outerContext, useContext: true);
		}
		DFAState.PredPrediction[] predicates2 = dFAState.predicates;
		if (predicates2 != null)
		{
			int index2 = input.Index;
			if (startIndex != index2)
			{
				input.Seek(startIndex);
			}
			BitSet bitSet2 = EvalSemanticContext(predicates2, outerContext, reportAmbiguities && predictionMode == PredictionMode.LlExactAmbigDetection);
			switch (bitSet2.Cardinality())
			{
			case 0:
				throw NoViableAlt(input, outerContext, dFAState.configs, startIndex);
			case 1:
				return bitSet2.NextSetBit(0);
			default:
				if (startIndex != index2)
				{
					input.Seek(index2);
				}
				ReportAmbiguity(dfa, dFAState, startIndex, index2, dFAState.configs.IsExactConflict, bitSet2, dFAState.configs);
				return bitSet2.NextSetBit(0);
			}
		}
		return dFAState.Prediction;
	}

	protected internal virtual bool IsAcceptState(DFAState state, bool useContext)
	{
		if (!state.IsAcceptState)
		{
			return false;
		}
		if (state.configs.ConflictingAlts == null)
		{
			return true;
		}
		if (useContext && predictionMode == PredictionMode.LlExactAmbigDetection)
		{
			return state.configs.IsExactConflict;
		}
		return true;
	}

	protected internal virtual int ExecATN(DFA dfa, ITokenStream input, int startIndex, SimulatorState initialState)
	{
		ParserRuleContext outerContext = initialState.outerContext;
		bool useContext = initialState.useContext;
		int num = input.La(1);
		SimulatorState simulatorState = initialState;
		PredictionContextCache contextCache = new PredictionContextCache();
		SimulatorState simulatorState2;
		DFAState s;
		while (true)
		{
			simulatorState2 = ComputeReachSet(dfa, simulatorState, num, contextCache);
			if (simulatorState2 == null)
			{
				AddDFAEdge(simulatorState.s0, input.La(1), ATNSimulator.Error);
				return HandleNoViableAlt(input, startIndex, simulatorState);
			}
			s = simulatorState2.s0;
			if (IsAcceptState(s, useContext))
			{
				break;
			}
			simulatorState = simulatorState2;
			if (num != -1)
			{
				input.Consume();
				num = input.La(1);
			}
		}
		BitSet bitSet = s.configs.ConflictingAlts;
		int num2 = ((bitSet == null) ? s.Prediction : 0);
		if (num2 != 0)
		{
			if (optimize_ll1 && input.Index == startIndex && !dfa.IsPrecedenceDfa && simulatorState2.outerContext == simulatorState2.remainingOuterContext && dfa.decision >= 0 && !s.configs.HasSemanticContext && num >= 0 && num <= 32767)
			{
				int key = (dfa.decision << 16) + num;
				atn.LL1Table[key] = num2;
			}
			if (useContext && always_try_local_context)
			{
				ReportContextSensitivity(dfa, num2, simulatorState2, startIndex, input.Index);
			}
		}
		num2 = s.Prediction;
		bool flag = bitSet != null && userWantsCtxSensitive;
		if (flag)
		{
			flag = !useContext && (s.configs.DipsIntoOuterContext || !s.configs.IsExactConflict) && (!treat_sllk1_conflict_as_ambiguity || input.Index != startIndex);
		}
		if (s.configs.HasSemanticContext)
		{
			DFAState.PredPrediction[] predicates = s.predicates;
			if (predicates != null)
			{
				int index = input.Index;
				if (index != startIndex)
				{
					input.Seek(startIndex);
				}
				bitSet = EvalSemanticContext(predicates, outerContext, flag || reportAmbiguities);
				switch (bitSet.Cardinality())
				{
				case 0:
					throw NoViableAlt(input, outerContext, s.configs, startIndex);
				case 1:
					return bitSet.NextSetBit(0);
				}
				if (index != startIndex)
				{
					input.Seek(index);
				}
			}
		}
		if (!flag)
		{
			if (bitSet != null)
			{
				if (reportAmbiguities && bitSet.Cardinality() > 1)
				{
					ReportAmbiguity(dfa, s, startIndex, input.Index, s.configs.IsExactConflict, bitSet, s.configs);
				}
				num2 = bitSet.NextSetBit(0);
			}
			return num2;
		}
		SimulatorState initialState2 = ComputeStartState(dfa, outerContext, useContext: true);
		if (reportAmbiguities)
		{
			ReportAttemptingFullContext(dfa, bitSet, simulatorState2, startIndex, input.Index);
		}
		input.Seek(startIndex);
		return ExecATN(dfa, input, startIndex, initialState2);
	}

	protected internal virtual int HandleNoViableAlt(ITokenStream input, int startIndex, SimulatorState previous)
	{
		if (previous.s0 != null)
		{
			BitSet bitSet = new BitSet();
			int num = 0;
			foreach (ATNConfig config in previous.s0.configs)
			{
				if (config.ReachesIntoOuterContext || config.State is RuleStopState)
				{
					bitSet.Set(config.Alt);
					num = Math.Max(num, config.Alt);
				}
			}
			switch (bitSet.Cardinality())
			{
			case 1:
				return bitSet.NextSetBit(0);
			default:
			{
				if (!previous.s0.configs.HasSemanticContext)
				{
					return bitSet.NextSetBit(0);
				}
				ATNConfigSet aTNConfigSet = new ATNConfigSet();
				foreach (ATNConfig config2 in previous.s0.configs)
				{
					if (config2.ReachesIntoOuterContext || config2.State is RuleStopState)
					{
						aTNConfigSet.Add(config2);
					}
				}
				SemanticContext[] predsForAmbigAlts = GetPredsForAmbigAlts(bitSet, aTNConfigSet, num);
				if (predsForAmbigAlts != null)
				{
					DFAState.PredPrediction[] predicatePredictions = GetPredicatePredictions(bitSet, predsForAmbigAlts);
					if (predicatePredictions != null)
					{
						int index = input.Index;
						try
						{
							input.Seek(startIndex);
							BitSet bitSet2 = EvalSemanticContext(predicatePredictions, previous.outerContext, complete: false);
							if (!bitSet2.IsEmpty())
							{
								return bitSet2.NextSetBit(0);
							}
						}
						finally
						{
							input.Seek(index);
						}
					}
				}
				return bitSet.NextSetBit(0);
			}
			case 0:
				break;
			}
		}
		throw NoViableAlt(input, previous.outerContext, previous.s0.configs, startIndex);
	}

	protected internal virtual SimulatorState ComputeReachSet(DFA dfa, SimulatorState previous, int t, PredictionContextCache contextCache)
	{
		bool useContext = previous.useContext;
		ParserRuleContext parserRuleContext = previous.remainingOuterContext;
		DFAState dFAState = previous.s0;
		if (useContext)
		{
			while (dFAState.IsContextSymbol(t))
			{
				DFAState dFAState2 = null;
				if (parserRuleContext != null)
				{
					parserRuleContext = SkipTailCalls(parserRuleContext);
					dFAState2 = dFAState.GetContextTarget(GetReturnState(parserRuleContext));
				}
				if (dFAState2 == null)
				{
					break;
				}
				parserRuleContext = (ParserRuleContext)parserRuleContext.Parent;
				dFAState = dFAState2;
			}
		}
		if (IsAcceptState(dFAState, useContext))
		{
			return new SimulatorState(previous.outerContext, dFAState, useContext, parserRuleContext);
		}
		DFAState s = dFAState;
		DFAState dFAState3 = GetExistingTargetState(s, t);
		if (dFAState3 == null)
		{
			Tuple<DFAState, ParserRuleContext> tuple = ComputeTargetState(dfa, s, parserRuleContext, t, useContext, contextCache);
			dFAState3 = tuple.Item1;
			parserRuleContext = tuple.Item2;
		}
		if (dFAState3 == ATNSimulator.Error)
		{
			return null;
		}
		return new SimulatorState(previous.outerContext, dFAState3, useContext, parserRuleContext);
	}

	[return: Nullable]
	protected internal virtual DFAState GetExistingTargetState(DFAState s, int t)
	{
		return s.GetTarget(t);
	}

	[return: NotNull]
	protected internal virtual Tuple<DFAState, ParserRuleContext> ComputeTargetState(DFA dfa, DFAState s, ParserRuleContext remainingGlobalContext, int t, bool useContext, PredictionContextCache contextCache)
	{
		IList<ATNConfig> list = new List<ATNConfig>(s.configs);
		List<int> list2 = null;
		ATNConfigSet aTNConfigSet = new ATNConfigSet();
		bool dipsIntoOuterContext;
		do
		{
			bool flag = !useContext || remainingGlobalContext != null;
			if (!flag)
			{
				aTNConfigSet.IsOutermostConfigSet = true;
			}
			ATNConfigSet aTNConfigSet2 = new ATNConfigSet();
			IList<ATNConfig> list3 = null;
			foreach (ATNConfig item2 in list)
			{
				if (item2.State is RuleStopState)
				{
					if ((useContext && !item2.ReachesIntoOuterContext) || t == -1)
					{
						if (list3 == null)
						{
							list3 = new List<ATNConfig>();
						}
						list3.Add(item2);
					}
					continue;
				}
				int numberOfOptimizedTransitions = item2.State.NumberOfOptimizedTransitions;
				for (int i = 0; i < numberOfOptimizedTransitions; i++)
				{
					Transition optimizedTransition = item2.State.GetOptimizedTransition(i);
					ATNState reachableTarget = GetReachableTarget(item2, optimizedTransition, t);
					if (reachableTarget != null)
					{
						aTNConfigSet2.Add(item2.Transform(reachableTarget, checkNonGreedy: false), contextCache);
					}
				}
			}
			if (optimize_unique_closure && list3 == null && t != -1 && aTNConfigSet2.UniqueAlt != 0)
			{
				aTNConfigSet2.IsOutermostConfigSet = aTNConfigSet.IsOutermostConfigSet;
				aTNConfigSet = aTNConfigSet2;
				break;
			}
			bool collectPredicates = false;
			bool treatEofAsEpsilon = t == -1;
			Closure(aTNConfigSet2, aTNConfigSet, collectPredicates, flag, contextCache, treatEofAsEpsilon);
			dipsIntoOuterContext = aTNConfigSet.DipsIntoOuterContext;
			if (t == -1)
			{
				aTNConfigSet = RemoveAllConfigsNotInRuleStopState(aTNConfigSet, contextCache);
			}
			if (list3 != null && (!useContext || !PredictionMode.HasConfigInRuleStopState(aTNConfigSet)))
			{
				foreach (ATNConfig item3 in list3)
				{
					aTNConfigSet.Add(item3, contextCache);
				}
			}
			if (!(useContext && dipsIntoOuterContext))
			{
				continue;
			}
			aTNConfigSet.Clear();
			remainingGlobalContext = SkipTailCalls(remainingGlobalContext);
			int returnState = GetReturnState(remainingGlobalContext);
			if (list2 == null)
			{
				list2 = new List<int>();
			}
			remainingGlobalContext = ((!remainingGlobalContext.IsEmpty) ? ((ParserRuleContext)remainingGlobalContext.Parent) : null);
			list2.Add(returnState);
			if (returnState != int.MaxValue)
			{
				for (int j = 0; j < list.Count; j++)
				{
					list[j] = list[j].AppendContext(returnState, contextCache);
				}
			}
		}
		while (useContext && dipsIntoOuterContext);
		if (aTNConfigSet.IsEmpty())
		{
			AddDFAEdge(s, t, ATNSimulator.Error);
			return Tuple.Create(ATNSimulator.Error, remainingGlobalContext);
		}
		DFAState item = AddDFAEdge(dfa, s, t, list2, aTNConfigSet, contextCache);
		return Tuple.Create(item, remainingGlobalContext);
	}

	[return: NotNull]
	protected internal virtual ATNConfigSet RemoveAllConfigsNotInRuleStopState(ATNConfigSet configs, PredictionContextCache contextCache)
	{
		if (PredictionMode.AllConfigsInRuleStopStates(configs))
		{
			return configs;
		}
		ATNConfigSet aTNConfigSet = new ATNConfigSet();
		foreach (ATNConfig config in configs)
		{
			if (config.State is RuleStopState)
			{
				aTNConfigSet.Add(config, contextCache);
			}
		}
		return aTNConfigSet;
	}

	[return: NotNull]
	protected internal virtual SimulatorState ComputeStartState(DFA dfa, ParserRuleContext globalContext, bool useContext)
	{
		DFAState dFAState = (dfa.IsPrecedenceDfa ? dfa.GetPrecedenceStartState(_parser.Precedence, useContext) : (useContext ? dfa.s0full.Get() : dfa.s0.Get()));
		if (dFAState != null)
		{
			if (!useContext)
			{
				return new SimulatorState(globalContext, dFAState, useContext, globalContext);
			}
			dFAState.SetContextSensitive(atn);
		}
		ATNState atnStartState = dfa.atnStartState;
		int num = 0;
		ParserRuleContext parserRuleContext = globalContext;
		PredictionContext predictionContext = (useContext ? PredictionContext.EmptyFull : PredictionContext.EmptyLocal);
		PredictionContextCache contextCache = new PredictionContextCache();
		if (useContext)
		{
			if (!enable_global_context_dfa)
			{
				while (parserRuleContext != null)
				{
					if (parserRuleContext.IsEmpty)
					{
						num = int.MaxValue;
						parserRuleContext = null;
					}
					else
					{
						num = GetReturnState(parserRuleContext);
						predictionContext = predictionContext.AppendContext(num, contextCache);
						parserRuleContext = (ParserRuleContext)parserRuleContext.Parent;
					}
				}
			}
			while (dFAState != null && dFAState.IsContextSensitive && parserRuleContext != null)
			{
				parserRuleContext = SkipTailCalls(parserRuleContext);
				DFAState contextTarget;
				if (parserRuleContext.IsEmpty)
				{
					contextTarget = dFAState.GetContextTarget(int.MaxValue);
					num = int.MaxValue;
					parserRuleContext = null;
				}
				else
				{
					num = GetReturnState(parserRuleContext);
					contextTarget = dFAState.GetContextTarget(num);
					predictionContext = predictionContext.AppendContext(num, contextCache);
					parserRuleContext = (ParserRuleContext)parserRuleContext.Parent;
				}
				if (contextTarget == null)
				{
					break;
				}
				dFAState = contextTarget;
			}
		}
		if (dFAState != null && !dFAState.IsContextSensitive)
		{
			return new SimulatorState(globalContext, dFAState, useContext, parserRuleContext);
		}
		ATNConfigSet aTNConfigSet = new ATNConfigSet();
		while (true)
		{
			ATNConfigSet aTNConfigSet2 = new ATNConfigSet();
			int numberOfTransitions = atnStartState.NumberOfTransitions;
			for (int i = 0; i < numberOfTransitions; i++)
			{
				ATNState target = atnStartState.Transition(i).target;
				aTNConfigSet2.Add(ATNConfig.Create(target, i + 1, predictionContext));
			}
			bool flag = parserRuleContext != null;
			if (!flag)
			{
				aTNConfigSet.IsOutermostConfigSet = true;
			}
			if ((!useContext || enable_global_context_dfa) && !dfa.IsPrecedenceDfa && dfa.atnStartState is StarLoopEntryState && ((StarLoopEntryState)dfa.atnStartState).precedenceRuleDecision)
			{
				dfa.IsPrecedenceDfa = true;
			}
			bool collectPredicates = true;
			Closure(aTNConfigSet2, aTNConfigSet, collectPredicates, flag, contextCache, treatEofAsEpsilon: false);
			bool dipsIntoOuterContext = aTNConfigSet.DipsIntoOuterContext;
			if (useContext && !enable_global_context_dfa)
			{
				dFAState = AddDFAState(dfa, aTNConfigSet, contextCache);
				break;
			}
			DFAState dFAState2;
			if (dFAState == null)
			{
				if (!dfa.IsPrecedenceDfa)
				{
					AtomicReference<DFAState> atomicReference = (useContext ? dfa.s0full : dfa.s0);
					dFAState2 = AddDFAState(dfa, aTNConfigSet, contextCache);
					if (!atomicReference.CompareAndSet(null, dFAState2))
					{
						dFAState2 = atomicReference.Get();
					}
				}
				else
				{
					aTNConfigSet = ApplyPrecedenceFilter(aTNConfigSet, globalContext, contextCache);
					dFAState2 = AddDFAState(dfa, aTNConfigSet, contextCache);
					dfa.SetPrecedenceStartState(_parser.Precedence, useContext, dFAState2);
				}
			}
			else
			{
				if (dfa.IsPrecedenceDfa)
				{
					aTNConfigSet = ApplyPrecedenceFilter(aTNConfigSet, globalContext, contextCache);
				}
				dFAState2 = AddDFAState(dfa, aTNConfigSet, contextCache);
				dFAState.SetContextTarget(num, dFAState2);
			}
			dFAState = dFAState2;
			if (!useContext || !dipsIntoOuterContext)
			{
				break;
			}
			dFAState2.SetContextSensitive(atn);
			aTNConfigSet.Clear();
			parserRuleContext = SkipTailCalls(parserRuleContext);
			int returnState = GetReturnState(parserRuleContext);
			parserRuleContext = ((!parserRuleContext.IsEmpty) ? ((ParserRuleContext)parserRuleContext.Parent) : null);
			if (returnState != int.MaxValue)
			{
				predictionContext = predictionContext.AppendContext(returnState, contextCache);
			}
			num = returnState;
		}
		return new SimulatorState(globalContext, dFAState, useContext, parserRuleContext);
	}

	[return: NotNull]
	protected internal virtual ATNConfigSet ApplyPrecedenceFilter(ATNConfigSet configs, ParserRuleContext globalContext, PredictionContextCache contextCache)
	{
		Dictionary<int, PredictionContext> dictionary = new Dictionary<int, PredictionContext>();
		ATNConfigSet aTNConfigSet = new ATNConfigSet();
		foreach (ATNConfig config in configs)
		{
			if (config.Alt != 1)
			{
				continue;
			}
			SemanticContext semanticContext = config.SemanticContext.EvalPrecedence(_parser, globalContext);
			if (semanticContext != null)
			{
				dictionary[config.State.stateNumber] = config.Context;
				if (semanticContext != config.SemanticContext)
				{
					aTNConfigSet.Add(config.Transform(config.State, semanticContext, checkNonGreedy: false), contextCache);
				}
				else
				{
					aTNConfigSet.Add(config, contextCache);
				}
			}
		}
		foreach (ATNConfig config2 in configs)
		{
			if (config2.Alt != 1 && (config2.PrecedenceFilterSuppressed || !dictionary.TryGetValue(config2.State.stateNumber, out var value) || !value.Equals(config2.Context)))
			{
				aTNConfigSet.Add(config2, contextCache);
			}
		}
		return aTNConfigSet;
	}

	[return: Nullable]
	protected internal virtual ATNState GetReachableTarget(ATNConfig source, Transition trans, int ttype)
	{
		if (trans.Matches(ttype, 0, atn.maxTokenType))
		{
			return trans.target;
		}
		return null;
	}

	protected internal virtual DFAState.PredPrediction[] PredicateDFAState(DFAState D, ATNConfigSet configs, int nalts)
	{
		BitSet conflictingAltsFromConfigSet = GetConflictingAltsFromConfigSet(configs);
		SemanticContext[] predsForAmbigAlts = GetPredsForAmbigAlts(conflictingAltsFromConfigSet, configs, nalts);
		DFAState.PredPrediction[] result = null;
		if (predsForAmbigAlts != null)
		{
			result = (D.predicates = GetPredicatePredictions(conflictingAltsFromConfigSet, predsForAmbigAlts));
		}
		return result;
	}

	protected internal virtual SemanticContext[] GetPredsForAmbigAlts(BitSet ambigAlts, ATNConfigSet configs, int nalts)
	{
		SemanticContext[] array = new SemanticContext[nalts + 1];
		int num = array.Length;
		foreach (ATNConfig config in configs)
		{
			if (ambigAlts.Get(config.Alt))
			{
				array[config.Alt] = SemanticContext.OrOp(array[config.Alt], config.SemanticContext);
			}
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (array[i] == null)
			{
				array[i] = SemanticContext.None;
			}
			else if (array[i] != SemanticContext.None)
			{
				num2++;
			}
		}
		if (num2 == 0)
		{
			array = null;
		}
		return array;
	}

	protected internal virtual DFAState.PredPrediction[] GetPredicatePredictions(BitSet ambigAlts, SemanticContext[] altToPred)
	{
		List<DFAState.PredPrediction> list = new List<DFAState.PredPrediction>();
		bool flag = false;
		for (int i = 1; i < altToPred.Length; i++)
		{
			SemanticContext semanticContext = altToPred[i];
			if (ambigAlts != null && ambigAlts.Get(i) && semanticContext == SemanticContext.None)
			{
				list.Add(new DFAState.PredPrediction(semanticContext, i));
			}
			else if (semanticContext != SemanticContext.None)
			{
				flag = true;
				list.Add(new DFAState.PredPrediction(semanticContext, i));
			}
		}
		if (!flag)
		{
			return null;
		}
		return list.ToArray();
	}

	protected internal virtual BitSet EvalSemanticContext(DFAState.PredPrediction[] predPredictions, ParserRuleContext outerContext, bool complete)
	{
		BitSet bitSet = new BitSet();
		foreach (DFAState.PredPrediction predPrediction in predPredictions)
		{
			if (predPrediction.pred == SemanticContext.None)
			{
				bitSet.Set(predPrediction.alt);
				if (!complete)
				{
					break;
				}
			}
			else if (EvalSemanticContext(predPrediction.pred, outerContext, predPrediction.alt))
			{
				bitSet.Set(predPrediction.alt);
				if (!complete)
				{
					break;
				}
			}
		}
		return bitSet;
	}

	protected internal virtual bool EvalSemanticContext(SemanticContext pred, ParserRuleContext parserCallStack, int alt)
	{
		return pred.Eval(_parser, parserCallStack);
	}

	protected internal virtual void Closure(ATNConfigSet sourceConfigs, ATNConfigSet configs, bool collectPredicates, bool hasMoreContext, PredictionContextCache contextCache, bool treatEofAsEpsilon)
	{
		if (contextCache == null)
		{
			contextCache = PredictionContextCache.Uncached;
		}
		ATNConfigSet aTNConfigSet = sourceConfigs;
		HashSet<ATNConfig> closureBusy = new HashSet<ATNConfig>();
		while (aTNConfigSet.Count > 0)
		{
			ATNConfigSet aTNConfigSet2 = new ATNConfigSet();
			foreach (ATNConfig item in aTNConfigSet)
			{
				Closure(item, configs, aTNConfigSet2, closureBusy, collectPredicates, hasMoreContext, contextCache, 0, treatEofAsEpsilon);
			}
			aTNConfigSet = aTNConfigSet2;
		}
	}

	protected internal virtual void Closure(ATNConfig config, ATNConfigSet configs, ATNConfigSet intermediate, HashSet<ATNConfig> closureBusy, bool collectPredicates, bool hasMoreContexts, PredictionContextCache contextCache, int depth, bool treatEofAsEpsilon)
	{
		if (config.State is RuleStopState)
		{
			if (!config.Context.IsEmpty)
			{
				bool hasEmpty = config.Context.HasEmpty;
				int num = config.Context.Size - (hasEmpty ? 1 : 0);
				for (int i = 0; i < num; i++)
				{
					PredictionContext parent = config.Context.GetParent(i);
					ATNState state = atn.states[config.Context.GetReturnState(i)];
					ATNConfig aTNConfig = ATNConfig.Create(state, config.Alt, parent, config.SemanticContext);
					aTNConfig.OuterContextDepth = config.OuterContextDepth;
					aTNConfig.PrecedenceFilterSuppressed = config.PrecedenceFilterSuppressed;
					Closure(aTNConfig, configs, intermediate, closureBusy, collectPredicates, hasMoreContexts, contextCache, depth - 1, treatEofAsEpsilon);
				}
				if (!hasEmpty || !hasMoreContexts)
				{
					return;
				}
				config = config.Transform(config.State, PredictionContext.EmptyLocal, checkNonGreedy: false);
			}
			else
			{
				if (!hasMoreContexts)
				{
					configs.Add(config, contextCache);
					return;
				}
				if (config.Context == PredictionContext.EmptyFull)
				{
					config = config.Transform(config.State, PredictionContext.EmptyLocal, checkNonGreedy: false);
				}
				else if (!config.ReachesIntoOuterContext && PredictionContext.IsEmptyLocal(config.Context))
				{
					configs.Add(config, contextCache);
				}
			}
		}
		ATNState state2 = config.State;
		if (!state2.OnlyHasEpsilonTransitions)
		{
			configs.Add(config, contextCache);
		}
		for (int j = 0; j < state2.NumberOfOptimizedTransitions; j++)
		{
			Transition optimizedTransition = state2.GetOptimizedTransition(j);
			bool collectPredicates2 = !(optimizedTransition is ActionTransition) && collectPredicates;
			ATNConfig epsilonTarget = GetEpsilonTarget(config, optimizedTransition, collectPredicates2, depth == 0, contextCache, treatEofAsEpsilon);
			if (epsilonTarget == null)
			{
				continue;
			}
			if (optimizedTransition is RuleTransition && intermediate != null && !collectPredicates)
			{
				intermediate.Add(epsilonTarget, contextCache);
			}
			else
			{
				if (!optimizedTransition.IsEpsilon && !closureBusy.Add(epsilonTarget))
				{
					continue;
				}
				int num2 = depth;
				if (config.State is RuleStopState)
				{
					if (!closureBusy.Add(epsilonTarget))
					{
						continue;
					}
					if (dfa != null && dfa.IsPrecedenceDfa)
					{
						int outermostPrecedenceReturn = ((EpsilonTransition)optimizedTransition).OutermostPrecedenceReturn;
						if (outermostPrecedenceReturn == dfa.atnStartState.ruleIndex)
						{
							epsilonTarget.PrecedenceFilterSuppressed = true;
						}
					}
					epsilonTarget.OuterContextDepth++;
					num2--;
				}
				else if (optimizedTransition is RuleTransition)
				{
					if (optimize_tail_calls && ((RuleTransition)optimizedTransition).optimizedTailCall && (!tail_call_preserves_sll || !PredictionContext.IsEmptyLocal(config.Context)))
					{
						if (num2 == 0)
						{
							num2--;
							if (!tail_call_preserves_sll && PredictionContext.IsEmptyLocal(config.Context))
							{
								epsilonTarget.OuterContextDepth++;
							}
						}
					}
					else if (num2 >= 0)
					{
						num2++;
					}
				}
				Closure(epsilonTarget, configs, intermediate, closureBusy, collectPredicates2, hasMoreContexts, contextCache, num2, treatEofAsEpsilon);
			}
		}
	}

	[return: NotNull]
	public virtual string GetRuleName(int index)
	{
		if (_parser != null && index >= 0)
		{
			return _parser.RuleNames[index];
		}
		return "<rule " + index + ">";
	}

	[return: Nullable]
	protected internal virtual ATNConfig GetEpsilonTarget(ATNConfig config, Transition t, bool collectPredicates, bool inContext, PredictionContextCache contextCache, bool treatEofAsEpsilon)
	{
		switch (t.TransitionType)
		{
		case TransitionType.Rule:
			return RuleTransition(config, (RuleTransition)t, contextCache);
		case TransitionType.Precedence:
			return PrecedenceTransition(config, (PrecedencePredicateTransition)t, collectPredicates, inContext);
		case TransitionType.Predicate:
			return PredTransition(config, (PredicateTransition)t, collectPredicates, inContext);
		case TransitionType.Action:
			return ActionTransition(config, (ActionTransition)t);
		case TransitionType.Epsilon:
			return config.Transform(t.target, checkNonGreedy: false);
		case TransitionType.Range:
		case TransitionType.Atom:
		case TransitionType.Set:
			if (treatEofAsEpsilon && t.Matches(-1, 0, 1))
			{
				return config.Transform(t.target, checkNonGreedy: false);
			}
			return null;
		default:
			return null;
		}
	}

	[return: NotNull]
	protected internal virtual ATNConfig ActionTransition(ATNConfig config, ActionTransition t)
	{
		return config.Transform(t.target, checkNonGreedy: false);
	}

	[return: Nullable]
	protected internal virtual ATNConfig PrecedenceTransition(ATNConfig config, PrecedencePredicateTransition pt, bool collectPredicates, bool inContext)
	{
		if (collectPredicates && inContext)
		{
			SemanticContext semanticContext = SemanticContext.AndOp(config.SemanticContext, pt.Predicate);
			return config.Transform(pt.target, semanticContext, checkNonGreedy: false);
		}
		return config.Transform(pt.target, checkNonGreedy: false);
	}

	[return: Nullable]
	protected internal virtual ATNConfig PredTransition(ATNConfig config, PredicateTransition pt, bool collectPredicates, bool inContext)
	{
		if (collectPredicates && (!pt.isCtxDependent || (pt.isCtxDependent && inContext)))
		{
			SemanticContext semanticContext = SemanticContext.AndOp(config.SemanticContext, pt.Predicate);
			return config.Transform(pt.target, semanticContext, checkNonGreedy: false);
		}
		return config.Transform(pt.target, checkNonGreedy: false);
	}

	[return: NotNull]
	protected internal virtual ATNConfig RuleTransition(ATNConfig config, RuleTransition t, PredictionContextCache contextCache)
	{
		ATNState followState = t.followState;
		return config.Transform(context: (optimize_tail_calls && t.optimizedTailCall && (!tail_call_preserves_sll || !PredictionContext.IsEmptyLocal(config.Context))) ? config.Context : ((contextCache == null) ? config.Context.GetChild(followState.stateNumber) : contextCache.GetChild(config.Context, followState.stateNumber)), state: t.target, checkNonGreedy: false);
	}

	private ConflictInfo IsConflicted(ATNConfigSet configset, PredictionContextCache contextCache)
	{
		if (configset.UniqueAlt != 0 || configset.Count <= 1)
		{
			return null;
		}
		List<ATNConfig> list = new List<ATNConfig>(configset);
		list.Sort(StateAltSortComparator);
		bool flag = !configset.DipsIntoOuterContext;
		BitSet bitSet = new BitSet();
		int alt = list[0].Alt;
		bitSet.Set(alt);
		int num = list[0].State.NonStopStateNumber;
		foreach (ATNConfig item in list)
		{
			int nonStopStateNumber = item.State.NonStopStateNumber;
			if (nonStopStateNumber != num)
			{
				if (item.Alt != alt)
				{
					return null;
				}
				num = nonStopStateNumber;
			}
		}
		if (flag)
		{
			num = list[0].State.NonStopStateNumber;
			BitSet bitSet2 = new BitSet();
			int num2 = alt;
			foreach (ATNConfig item2 in list)
			{
				if (item2.State.NonStopStateNumber != num)
				{
					break;
				}
				int alt2 = item2.Alt;
				bitSet2.Set(alt2);
				num2 = alt2;
			}
			num = list[0].State.NonStopStateNumber;
			int num3 = alt;
			foreach (ATNConfig item3 in list)
			{
				int nonStopStateNumber2 = item3.State.NonStopStateNumber;
				int alt3 = item3.Alt;
				if (nonStopStateNumber2 != num)
				{
					if (num3 != num2)
					{
						flag = false;
						break;
					}
					num = nonStopStateNumber2;
					num3 = alt;
				}
				else if (alt3 != num3)
				{
					if (alt3 != bitSet2.NextSetBit(num3 + 1))
					{
						flag = false;
						break;
					}
					num3 = alt3;
				}
			}
		}
		num = list[0].State.NonStopStateNumber;
		int num4 = 0;
		int num5 = 0;
		PredictionContext predictionContext = list[0].Context;
		for (int i = 1; i < list.Count; i++)
		{
			ATNConfig aTNConfig = list[i];
			if (aTNConfig.Alt != alt || aTNConfig.State.NonStopStateNumber != num)
			{
				break;
			}
			num5 = i;
			predictionContext = contextCache.Join(predictionContext, list[i].Context);
		}
		int num6;
		for (num6 = num5 + 1; num6 < list.Count; num6++)
		{
			ATNConfig aTNConfig2 = list[num6];
			ATNState state = aTNConfig2.State;
			bitSet.Set(aTNConfig2.Alt);
			if (state.NonStopStateNumber != num)
			{
				num = state.NonStopStateNumber;
				num4 = num6;
				num5 = num6;
				predictionContext = aTNConfig2.Context;
				for (int j = num4 + 1; j < list.Count; j++)
				{
					ATNConfig aTNConfig3 = list[j];
					if (aTNConfig3.Alt != alt || aTNConfig3.State.NonStopStateNumber != num)
					{
						break;
					}
					num5 = j;
					predictionContext = contextCache.Join(predictionContext, aTNConfig3.Context);
				}
				num6 = num5;
			}
			else
			{
				PredictionContext predictionContext2 = aTNConfig2.Context;
				int alt4 = aTNConfig2.Alt;
				int num7 = num6;
				for (int k = num7 + 1; k < list.Count; k++)
				{
					ATNConfig aTNConfig4 = list[k];
					if (aTNConfig4.Alt != alt4 || aTNConfig4.State.NonStopStateNumber != num)
					{
						break;
					}
					num7 = k;
					predictionContext2 = contextCache.Join(predictionContext2, aTNConfig4.Context);
				}
				num6 = num7;
				PredictionContext obj = contextCache.Join(predictionContext, predictionContext2);
				if (!predictionContext.Equals(obj))
				{
					return null;
				}
				flag = flag && predictionContext.Equals(predictionContext2);
			}
		}
		return new ConflictInfo(bitSet, flag);
	}

	protected internal virtual BitSet GetConflictingAltsFromConfigSet(ATNConfigSet configs)
	{
		BitSet bitSet = configs.ConflictingAlts;
		if (bitSet == null && configs.UniqueAlt != 0)
		{
			bitSet = new BitSet();
			bitSet.Set(configs.UniqueAlt);
		}
		return bitSet;
	}

	[return: NotNull]
	public virtual string GetTokenName(int t)
	{
		if (t == -1)
		{
			return "EOF";
		}
		IVocabulary vocabulary;
		if (_parser == null)
		{
			IVocabulary emptyVocabulary = Vocabulary.EmptyVocabulary;
			vocabulary = emptyVocabulary;
		}
		else
		{
			vocabulary = _parser.Vocabulary;
		}
		IVocabulary vocabulary2 = vocabulary;
		string displayName = vocabulary2.GetDisplayName(t);
		if (displayName.Equals(t.ToString(), StringComparison.Ordinal))
		{
			return displayName;
		}
		return displayName + "<" + t + ">";
	}

	public virtual string GetLookaheadName(ITokenStream input)
	{
		return GetTokenName(input.La(1));
	}

	public virtual void DumpDeadEndConfigs(NoViableAltException nvae)
	{
		Console.Error.WriteLine("dead end configs: ");
		foreach (ATNConfig deadEndConfig in nvae.DeadEndConfigs)
		{
			string text = "no edges";
			if (deadEndConfig.State.NumberOfOptimizedTransitions > 0)
			{
				Transition optimizedTransition = deadEndConfig.State.GetOptimizedTransition(0);
				if (optimizedTransition is AtomTransition)
				{
					AtomTransition atomTransition = (AtomTransition)optimizedTransition;
					text = "Atom " + GetTokenName(atomTransition.token);
				}
				else if (optimizedTransition is SetTransition)
				{
					SetTransition setTransition = (SetTransition)optimizedTransition;
					text = ((setTransition is NotSetTransition) ? "~" : string.Empty) + "Set " + setTransition.set.ToString();
				}
			}
			Console.Error.WriteLine(deadEndConfig.ToString(_parser, showAlt: true) + ":" + text);
		}
	}

	[return: NotNull]
	protected internal virtual NoViableAltException NoViableAlt(ITokenStream input, ParserRuleContext outerContext, ATNConfigSet configs, int startIndex)
	{
		return new NoViableAltException(_parser, input, input.Get(startIndex), input.Lt(1), configs, outerContext);
	}

	protected internal virtual int GetUniqueAlt(IEnumerable<ATNConfig> configs)
	{
		int num = 0;
		foreach (ATNConfig config in configs)
		{
			if (num == 0)
			{
				num = config.Alt;
			}
			else if (config.Alt != num)
			{
				return 0;
			}
		}
		return num;
	}

	protected internal virtual bool ConfigWithAltAtStopState(IEnumerable<ATNConfig> configs, int alt)
	{
		foreach (ATNConfig config in configs)
		{
			if (config.Alt == alt && config.State is RuleStopState)
			{
				return true;
			}
		}
		return false;
	}

	[return: NotNull]
	protected internal virtual DFAState AddDFAEdge(DFA dfa, DFAState fromState, int t, List<int> contextTransitions, ATNConfigSet toConfigs, PredictionContextCache contextCache)
	{
		DFAState dFAState = fromState;
		DFAState dFAState2 = AddDFAState(dfa, toConfigs, contextCache);
		if (contextTransitions != null)
		{
			int[] array = contextTransitions.ToArray();
			foreach (int num in array)
			{
				if (num != int.MaxValue || !dFAState.configs.IsOutermostConfigSet)
				{
					dFAState.SetContextSensitive(atn);
					dFAState.SetContextSymbol(t);
					DFAState contextTarget = dFAState.GetContextTarget(num);
					if (contextTarget != null)
					{
						dFAState = contextTarget;
						continue;
					}
					contextTarget = AddDFAContextState(dfa, dFAState.configs, num, contextCache);
					dFAState.SetContextTarget(num, contextTarget);
					dFAState = contextTarget;
				}
			}
		}
		AddDFAEdge(dFAState, t, dFAState2);
		return dFAState2;
	}

	protected internal virtual void AddDFAEdge(DFAState p, int t, DFAState q)
	{
		p?.SetTarget(t, q);
	}

	[return: NotNull]
	protected internal virtual DFAState AddDFAContextState(DFA dfa, ATNConfigSet configs, int returnContext, PredictionContextCache contextCache)
	{
		if (returnContext != int.MaxValue)
		{
			ATNConfigSet aTNConfigSet = new ATNConfigSet();
			foreach (ATNConfig config in configs)
			{
				aTNConfigSet.Add(config.AppendContext(returnContext, contextCache));
			}
			return AddDFAState(dfa, aTNConfigSet, contextCache);
		}
		configs = configs.Clone(@readonly: true);
		configs.IsOutermostConfigSet = true;
		return AddDFAState(dfa, configs, contextCache);
	}

	[return: NotNull]
	protected internal virtual DFAState AddDFAState(DFA dfa, ATNConfigSet configs, PredictionContextCache contextCache)
	{
		bool flag = enable_global_context_dfa || !configs.IsOutermostConfigSet;
		if (flag)
		{
			if (!configs.IsReadOnly)
			{
				configs.OptimizeConfigs(this);
			}
			DFAState key = CreateDFAState(dfa, configs);
			if (dfa.states.TryGetValue(key, out var value))
			{
				return value;
			}
		}
		if (!configs.IsReadOnly && configs.ConflictInformation == null)
		{
			configs.ConflictInformation = IsConflicted(configs, contextCache);
		}
		DFAState dFAState = CreateDFAState(dfa, configs.Clone(@readonly: true));
		DecisionState decisionState = atn.GetDecisionState(dfa.decision);
		int uniqueAlt = GetUniqueAlt(configs);
		if (uniqueAlt != 0)
		{
			dFAState.AcceptStateInfo = new AcceptStateInfo(uniqueAlt);
		}
		else if (configs.ConflictingAlts != null)
		{
			dFAState.AcceptStateInfo = new AcceptStateInfo(dFAState.configs.ConflictingAlts.NextSetBit(0));
		}
		if (dFAState.IsAcceptState && configs.HasSemanticContext)
		{
			PredicateDFAState(dFAState, configs, decisionState.NumberOfTransitions);
		}
		if (!flag)
		{
			return dFAState;
		}
		return dfa.AddState(dFAState);
	}

	[return: NotNull]
	protected internal virtual DFAState CreateDFAState(DFA dfa, ATNConfigSet configs)
	{
		return new DFAState(dfa, configs);
	}

	protected internal virtual void ReportAttemptingFullContext(DFA dfa, BitSet conflictingAlts, SimulatorState conflictState, int startIndex, int stopIndex)
	{
		if (_parser != null)
		{
			((IParserErrorListener)_parser.ErrorListenerDispatch).ReportAttemptingFullContext(_parser, dfa, startIndex, stopIndex, conflictingAlts, conflictState);
		}
	}

	protected internal virtual void ReportContextSensitivity(DFA dfa, int prediction, SimulatorState acceptState, int startIndex, int stopIndex)
	{
		if (_parser != null)
		{
			((IParserErrorListener)_parser.ErrorListenerDispatch).ReportContextSensitivity(_parser, dfa, startIndex, stopIndex, prediction, acceptState);
		}
	}

	protected internal virtual void ReportAmbiguity(DFA dfa, DFAState D, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
	{
		if (_parser != null)
		{
			((IParserErrorListener)_parser.ErrorListenerDispatch).ReportAmbiguity(_parser, dfa, startIndex, stopIndex, exact, ambigAlts, configs);
		}
	}

	protected internal int GetReturnState(RuleContext context)
	{
		if (context.IsEmpty)
		{
			return int.MaxValue;
		}
		ATNState aTNState = atn.states[context.invokingState];
		RuleTransition ruleTransition = (RuleTransition)aTNState.Transition(0);
		return ruleTransition.followState.stateNumber;
	}

	protected internal ParserRuleContext SkipTailCalls(ParserRuleContext context)
	{
		if (!optimize_tail_calls)
		{
			return context;
		}
		while (!context.IsEmpty)
		{
			ATNState aTNState = atn.states[context.invokingState];
			RuleTransition ruleTransition = (RuleTransition)aTNState.Transition(0);
			if (!ruleTransition.tailCall)
			{
				break;
			}
			context = (ParserRuleContext)context.Parent;
		}
		return context;
	}
}
