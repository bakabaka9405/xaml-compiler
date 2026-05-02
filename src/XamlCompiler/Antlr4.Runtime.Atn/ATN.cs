using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public class ATN
{
	public const int InvalidAltNumber = 0;

	[NotNull]
	public readonly IList<ATNState> states = new List<ATNState>();

	[NotNull]
	public readonly IList<DecisionState> decisionToState = new List<DecisionState>();

	public RuleStartState[] ruleToStartState;

	public RuleStopState[] ruleToStopState;

	[NotNull]
	public readonly IDictionary<string, TokensStartState> modeNameToStartState = new Dictionary<string, TokensStartState>();

	public readonly ATNType grammarType;

	public readonly int maxTokenType;

	public int[] ruleToTokenType;

	public ILexerAction[] lexerActions;

	[NotNull]
	public readonly IList<TokensStartState> modeToStartState = new List<TokensStartState>();

	private readonly ConcurrentDictionary<PredictionContext, PredictionContext> contextCache = new ConcurrentDictionary<PredictionContext, PredictionContext>();

	[NotNull]
	public DFA[] decisionToDFA = new DFA[0];

	[NotNull]
	public DFA[] modeToDFA = new DFA[0];

	protected internal readonly ConcurrentDictionary<int, int> LL1Table = new ConcurrentDictionary<int, int>();

	public virtual int ContextCacheSize => contextCache.Count;

	public virtual int NumberOfDecisions => decisionToState.Count;

	public ATN(ATNType grammarType, int maxTokenType)
	{
		this.grammarType = grammarType;
		this.maxTokenType = maxTokenType;
	}

	public void ClearDFA()
	{
		decisionToDFA = new DFA[decisionToState.Count];
		for (int i = 0; i < decisionToDFA.Length; i++)
		{
			decisionToDFA[i] = new DFA(decisionToState[i], i);
		}
		modeToDFA = new DFA[modeToStartState.Count];
		for (int j = 0; j < modeToDFA.Length; j++)
		{
			modeToDFA[j] = new DFA(modeToStartState[j]);
		}
		contextCache.Clear();
		LL1Table.Clear();
	}

	public virtual PredictionContext GetCachedContext(PredictionContext context)
	{
		return PredictionContext.GetCachedContext(context, contextCache, new PredictionContext.IdentityHashMap());
	}

	[return: NotNull]
	public virtual IntervalSet NextTokens(ATNState s, PredictionContext ctx)
	{
		Args.NotNull("ctx", ctx);
		LL1Analyzer lL1Analyzer = new LL1Analyzer(this);
		return lL1Analyzer.Look(s, ctx);
	}

	[return: NotNull]
	public virtual IntervalSet NextTokens(ATNState s)
	{
		if (s.nextTokenWithinRule != null)
		{
			return s.nextTokenWithinRule;
		}
		s.nextTokenWithinRule = NextTokens(s, PredictionContext.EmptyLocal);
		s.nextTokenWithinRule.SetReadonly(@readonly: true);
		return s.nextTokenWithinRule;
	}

	public virtual void AddState(ATNState state)
	{
		if (state != null)
		{
			state.atn = this;
			state.stateNumber = states.Count;
		}
		states.Add(state);
	}

	public virtual void RemoveState(ATNState state)
	{
		states[state.stateNumber] = null;
	}

	public virtual void DefineMode(string name, TokensStartState s)
	{
		modeNameToStartState[name] = s;
		modeToStartState.Add(s);
		modeToDFA = Arrays.CopyOf(modeToDFA, modeToStartState.Count);
		modeToDFA[modeToDFA.Length - 1] = new DFA(s);
		DefineDecisionState(s);
	}

	public virtual int DefineDecisionState(DecisionState s)
	{
		decisionToState.Add(s);
		s.decision = decisionToState.Count - 1;
		decisionToDFA = Arrays.CopyOf(decisionToDFA, decisionToState.Count);
		decisionToDFA[decisionToDFA.Length - 1] = new DFA(s, s.decision);
		return s.decision;
	}

	public virtual DecisionState GetDecisionState(int decision)
	{
		if (decisionToState.Count != 0)
		{
			return decisionToState[decision];
		}
		return null;
	}

	[return: NotNull]
	public virtual IntervalSet GetExpectedTokens(int stateNumber, RuleContext context)
	{
		if (stateNumber < 0 || stateNumber >= states.Count)
		{
			throw new ArgumentException("Invalid state number.");
		}
		RuleContext ruleContext = context;
		ATNState s = states[stateNumber];
		IntervalSet intervalSet = NextTokens(s);
		if (!intervalSet.Contains(-2))
		{
			return intervalSet;
		}
		IntervalSet intervalSet2 = new IntervalSet();
		intervalSet2.AddAll(intervalSet);
		intervalSet2.Remove(-2);
		while (ruleContext != null && ruleContext.invokingState >= 0 && intervalSet.Contains(-2))
		{
			ATNState aTNState = states[ruleContext.invokingState];
			RuleTransition ruleTransition = (RuleTransition)aTNState.Transition(0);
			intervalSet = NextTokens(ruleTransition.followState);
			intervalSet2.AddAll(intervalSet);
			intervalSet2.Remove(-2);
			ruleContext = ruleContext.Parent;
		}
		if (intervalSet.Contains(-2))
		{
			intervalSet2.Add(-1);
		}
		return intervalSet2;
	}
}
