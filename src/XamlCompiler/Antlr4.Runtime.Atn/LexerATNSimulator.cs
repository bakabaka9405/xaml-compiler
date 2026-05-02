using System;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public class LexerATNSimulator : ATNSimulator
{
	protected internal class SimState
	{
		protected internal int index = -1;

		protected internal int line;

		protected internal int charPos = -1;

		protected internal DFAState dfaState;

		protected internal virtual void Reset()
		{
			index = -1;
			line = 0;
			charPos = -1;
			dfaState = null;
		}
	}

	public const bool debug = false;

	public const bool dfa_debug = false;

	public const int MinDfaEdge = 0;

	public const int MaxDfaEdge = 127;

	public bool optimize_tail_calls = true;

	[Nullable]
	protected internal readonly Lexer recog;

	protected internal int startIndex = -1;

	private int _line = 1;

	protected internal int charPositionInLine;

	protected internal int mode;

	[NotNull]
	protected internal readonly SimState prevAccept = new SimState();

	public static int match_calls;

	public virtual int Line
	{
		get
		{
			return _line;
		}
		set
		{
			_line = value;
		}
	}

	public virtual int Column
	{
		get
		{
			return charPositionInLine;
		}
		set
		{
			charPositionInLine = value;
		}
	}

	public LexerATNSimulator(ATN atn)
		: this(null, atn)
	{
	}

	public LexerATNSimulator(Lexer recog, ATN atn)
		: base(atn)
	{
		this.recog = recog;
	}

	public virtual void CopyState(LexerATNSimulator simulator)
	{
		charPositionInLine = simulator.charPositionInLine;
		_line = simulator._line;
		mode = simulator.mode;
		startIndex = simulator.startIndex;
	}

	public virtual int Match(ICharStream input, int mode)
	{
		match_calls++;
		this.mode = mode;
		int marker = input.Mark();
		try
		{
			startIndex = input.Index;
			prevAccept.Reset();
			DFAState dFAState = atn.modeToDFA[mode].s0.Get();
			if (dFAState == null)
			{
				return MatchATN(input);
			}
			return ExecATN(input, dFAState);
		}
		finally
		{
			input.Release(marker);
		}
	}

	public override void Reset()
	{
		prevAccept.Reset();
		startIndex = -1;
		_line = 1;
		charPositionInLine = 0;
		mode = 0;
	}

	protected internal virtual int MatchATN(ICharStream input)
	{
		ATNState p = atn.modeToStartState[mode];
		ATNConfigSet aTNConfigSet = ComputeStartState(input, p);
		bool hasSemanticContext = aTNConfigSet.HasSemanticContext;
		if (hasSemanticContext)
		{
			aTNConfigSet.ClearExplicitSemanticContext();
		}
		DFAState dFAState = AddDFAState(aTNConfigSet);
		if (!hasSemanticContext && !atn.modeToDFA[mode].s0.CompareAndSet(null, dFAState))
		{
			dFAState = atn.modeToDFA[mode].s0.Get();
		}
		return ExecATN(input, dFAState);
	}

	protected internal virtual int ExecATN(ICharStream input, DFAState ds0)
	{
		if (ds0.IsAcceptState)
		{
			CaptureSimState(prevAccept, input, ds0);
		}
		int num = input.La(1);
		DFAState dFAState = ds0;
		while (true)
		{
			DFAState dFAState2 = GetExistingTargetState(dFAState, num);
			if (dFAState2 == null)
			{
				dFAState2 = ComputeTargetState(input, dFAState, num);
			}
			if (dFAState2 == ATNSimulator.Error)
			{
				break;
			}
			if (num != -1)
			{
				Consume(input);
			}
			if (dFAState2.IsAcceptState)
			{
				CaptureSimState(prevAccept, input, dFAState2);
				if (num == -1)
				{
					break;
				}
			}
			num = input.La(1);
			dFAState = dFAState2;
		}
		return FailOrAccept(prevAccept, input, dFAState.configs, num);
	}

	[return: Nullable]
	protected internal virtual DFAState GetExistingTargetState(DFAState s, int t)
	{
		return s.GetTarget(t);
	}

	[return: NotNull]
	protected internal virtual DFAState ComputeTargetState(ICharStream input, DFAState s, int t)
	{
		ATNConfigSet aTNConfigSet = new OrderedATNConfigSet();
		GetReachableConfigSet(input, s.configs, aTNConfigSet, t);
		if (aTNConfigSet.IsEmpty())
		{
			if (!aTNConfigSet.HasSemanticContext)
			{
				AddDFAEdge(s, t, ATNSimulator.Error);
			}
			return ATNSimulator.Error;
		}
		return AddDFAEdge(s, t, aTNConfigSet);
	}

	protected internal virtual int FailOrAccept(SimState prevAccept, ICharStream input, ATNConfigSet reach, int t)
	{
		if (prevAccept.dfaState != null)
		{
			LexerActionExecutor lexerActionExecutor = prevAccept.dfaState.LexerActionExecutor;
			Accept(input, lexerActionExecutor, startIndex, prevAccept.index, prevAccept.line, prevAccept.charPos);
			return prevAccept.dfaState.Prediction;
		}
		if (t == -1 && input.Index == startIndex)
		{
			return -1;
		}
		throw new LexerNoViableAltException(recog, input, startIndex, reach);
	}

	protected internal virtual void GetReachableConfigSet(ICharStream input, ATNConfigSet closure, ATNConfigSet reach, int t)
	{
		int num = 0;
		foreach (ATNConfig item in closure)
		{
			bool flag = item.Alt == num;
			if (flag && item.PassedThroughNonGreedyDecision)
			{
				continue;
			}
			int numberOfOptimizedTransitions = item.State.NumberOfOptimizedTransitions;
			for (int i = 0; i < numberOfOptimizedTransitions; i++)
			{
				Transition optimizedTransition = item.State.GetOptimizedTransition(i);
				ATNState reachableTarget = GetReachableTarget(optimizedTransition, t);
				if (reachableTarget != null)
				{
					LexerActionExecutor lexerActionExecutor = item.ActionExecutor;
					if (lexerActionExecutor != null)
					{
						lexerActionExecutor = lexerActionExecutor.FixOffsetBeforeMatch(input.Index - startIndex);
					}
					bool treatEofAsEpsilon = t == -1;
					if (Closure(input, item.Transform(reachableTarget, lexerActionExecutor, checkNonGreedy: true), reach, flag, speculative: true, treatEofAsEpsilon))
					{
						num = item.Alt;
						break;
					}
				}
			}
		}
	}

	protected internal virtual void Accept(ICharStream input, LexerActionExecutor lexerActionExecutor, int startIndex, int index, int line, int charPos)
	{
		input.Seek(index);
		_line = line;
		charPositionInLine = charPos;
		if (lexerActionExecutor != null && recog != null)
		{
			lexerActionExecutor.Execute(recog, input, startIndex);
		}
	}

	[return: Nullable]
	protected internal virtual ATNState GetReachableTarget(Transition trans, int t)
	{
		if (trans.Matches(t, 0, 65535))
		{
			return trans.target;
		}
		return null;
	}

	[return: NotNull]
	protected internal virtual ATNConfigSet ComputeStartState(ICharStream input, ATNState p)
	{
		PredictionContext emptyFull = PredictionContext.EmptyFull;
		ATNConfigSet aTNConfigSet = new OrderedATNConfigSet();
		for (int i = 0; i < p.NumberOfTransitions; i++)
		{
			ATNState target = p.Transition(i).target;
			ATNConfig config = ATNConfig.Create(target, i + 1, emptyFull);
			Closure(input, config, aTNConfigSet, currentAltReachedAcceptState: false, speculative: false, treatEofAsEpsilon: false);
		}
		return aTNConfigSet;
	}

	protected internal virtual bool Closure(ICharStream input, ATNConfig config, ATNConfigSet configs, bool currentAltReachedAcceptState, bool speculative, bool treatEofAsEpsilon)
	{
		if (config.State is RuleStopState)
		{
			PredictionContext context = config.Context;
			if (context.IsEmpty)
			{
				configs.Add(config);
				return true;
			}
			if (context.HasEmpty)
			{
				configs.Add(config.Transform(config.State, PredictionContext.EmptyFull, checkNonGreedy: true));
				currentAltReachedAcceptState = true;
			}
			for (int i = 0; i < context.Size; i++)
			{
				int returnState = context.GetReturnState(i);
				if (returnState != int.MaxValue)
				{
					PredictionContext parent = context.GetParent(i);
					ATNState state = atn.states[returnState];
					ATNConfig config2 = config.Transform(state, parent, checkNonGreedy: false);
					currentAltReachedAcceptState = Closure(input, config2, configs, currentAltReachedAcceptState, speculative, treatEofAsEpsilon);
				}
			}
			return currentAltReachedAcceptState;
		}
		if (!config.State.OnlyHasEpsilonTransitions && (!currentAltReachedAcceptState || !config.PassedThroughNonGreedyDecision))
		{
			configs.Add(config);
		}
		ATNState state2 = config.State;
		for (int j = 0; j < state2.NumberOfOptimizedTransitions; j++)
		{
			Transition optimizedTransition = state2.GetOptimizedTransition(j);
			ATNConfig epsilonTarget = GetEpsilonTarget(input, config, optimizedTransition, configs, speculative, treatEofAsEpsilon);
			if (epsilonTarget != null)
			{
				currentAltReachedAcceptState = Closure(input, epsilonTarget, configs, currentAltReachedAcceptState, speculative, treatEofAsEpsilon);
			}
		}
		return currentAltReachedAcceptState;
	}

	[return: Nullable]
	protected internal virtual ATNConfig GetEpsilonTarget(ICharStream input, ATNConfig config, Transition t, ATNConfigSet configs, bool speculative, bool treatEofAsEpsilon)
	{
		switch (t.TransitionType)
		{
		case TransitionType.Rule:
		{
			RuleTransition ruleTransition = (RuleTransition)t;
			if (optimize_tail_calls && ruleTransition.optimizedTailCall && !config.Context.HasEmpty)
			{
				return config.Transform(t.target, checkNonGreedy: true);
			}
			PredictionContext child = config.Context.GetChild(ruleTransition.followState.stateNumber);
			return config.Transform(t.target, child, checkNonGreedy: true);
		}
		case TransitionType.Precedence:
			throw new NotSupportedException("Precedence predicates are not supported in lexers.");
		case TransitionType.Predicate:
		{
			PredicateTransition predicateTransition = (PredicateTransition)t;
			configs.MarkExplicitSemanticContext();
			if (EvaluatePredicate(input, predicateTransition.ruleIndex, predicateTransition.predIndex, speculative))
			{
				return config.Transform(t.target, checkNonGreedy: true);
			}
			return null;
		}
		case TransitionType.Action:
			if (config.Context.HasEmpty)
			{
				LexerActionExecutor lexerActionExecutor = LexerActionExecutor.Append(config.ActionExecutor, atn.lexerActions[((ActionTransition)t).actionIndex]);
				return config.Transform(t.target, lexerActionExecutor, checkNonGreedy: true);
			}
			return config.Transform(t.target, checkNonGreedy: true);
		case TransitionType.Epsilon:
			return config.Transform(t.target, checkNonGreedy: true);
		case TransitionType.Range:
		case TransitionType.Atom:
		case TransitionType.Set:
			if (treatEofAsEpsilon && t.Matches(-1, 0, 65535))
			{
				return config.Transform(t.target, checkNonGreedy: false);
			}
			return null;
		default:
			return null;
		}
	}

	protected internal virtual bool EvaluatePredicate(ICharStream input, int ruleIndex, int predIndex, bool speculative)
	{
		if (recog == null)
		{
			return true;
		}
		if (!speculative)
		{
			return recog.Sempred(null, ruleIndex, predIndex);
		}
		int num = charPositionInLine;
		int line = _line;
		int index = input.Index;
		int marker = input.Mark();
		try
		{
			Consume(input);
			return recog.Sempred(null, ruleIndex, predIndex);
		}
		finally
		{
			charPositionInLine = num;
			_line = line;
			input.Seek(index);
			input.Release(marker);
		}
	}

	protected internal virtual void CaptureSimState(SimState settings, ICharStream input, DFAState dfaState)
	{
		settings.index = input.Index;
		settings.line = _line;
		settings.charPos = charPositionInLine;
		settings.dfaState = dfaState;
	}

	[return: NotNull]
	protected internal virtual DFAState AddDFAEdge(DFAState from, int t, ATNConfigSet q)
	{
		bool hasSemanticContext = q.HasSemanticContext;
		if (hasSemanticContext)
		{
			q.ClearExplicitSemanticContext();
		}
		DFAState dFAState = AddDFAState(q);
		if (hasSemanticContext)
		{
			return dFAState;
		}
		AddDFAEdge(from, t, dFAState);
		return dFAState;
	}

	protected internal virtual void AddDFAEdge(DFAState p, int t, DFAState q)
	{
		p?.SetTarget(t, q);
	}

	[return: NotNull]
	protected internal virtual DFAState AddDFAState(ATNConfigSet configs)
	{
		DFAState key = new DFAState(atn.modeToDFA[mode], configs);
		if (atn.modeToDFA[mode].states.TryGetValue(key, out var value))
		{
			return value;
		}
		configs.OptimizeConfigs(this);
		DFAState dFAState = new DFAState(atn.modeToDFA[mode], configs.Clone(@readonly: true));
		ATNConfig aTNConfig = null;
		foreach (ATNConfig config in configs)
		{
			if (config.State is RuleStopState)
			{
				aTNConfig = config;
				break;
			}
		}
		if (aTNConfig != null)
		{
			int prediction = atn.ruleToTokenType[aTNConfig.State.ruleIndex];
			LexerActionExecutor actionExecutor = aTNConfig.ActionExecutor;
			dFAState.AcceptStateInfo = new AcceptStateInfo(prediction, actionExecutor);
		}
		return atn.modeToDFA[mode].AddState(dFAState);
	}

	[return: NotNull]
	public DFA GetDFA(int mode)
	{
		return atn.modeToDFA[mode];
	}

	[return: NotNull]
	public virtual string GetText(ICharStream input)
	{
		return input.GetText(Interval.Of(startIndex, input.Index - 1));
	}

	public virtual void Consume(ICharStream input)
	{
		int num = input.La(1);
		if (num == 10)
		{
			_line++;
			charPositionInLine = 0;
		}
		else
		{
			charPositionInLine++;
		}
		input.Consume();
	}

	[return: NotNull]
	public virtual string GetTokenName(int t)
	{
		if (t == -1)
		{
			return "EOF";
		}
		return "'" + (char)t + "'";
	}
}
