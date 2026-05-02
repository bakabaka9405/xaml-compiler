using System;
using System.Collections.Generic;
using System.Globalization;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public class ATNDeserializer
{
	public static readonly int SerializedVersion;

	private static readonly Guid BaseSerializedUuid;

	private static readonly IList<Guid> SupportedUuids;

	public static readonly Guid SerializedUuid;

	[NotNull]
	private readonly ATNDeserializationOptions deserializationOptions;

	private char[] data;

	private int p;

	static ATNDeserializer()
	{
		SerializedVersion = 3;
		BaseSerializedUuid = new Guid("AADB8D7E-AEEF-4415-AD2B-8204D6CF042E");
		SupportedUuids = new List<Guid>();
		SupportedUuids.Add(BaseSerializedUuid);
		SerializedUuid = BaseSerializedUuid;
	}

	public ATNDeserializer()
		: this(ATNDeserializationOptions.Default)
	{
	}

	public ATNDeserializer(ATNDeserializationOptions deserializationOptions)
	{
		if (deserializationOptions == null)
		{
			deserializationOptions = ATNDeserializationOptions.Default;
		}
		this.deserializationOptions = deserializationOptions;
	}

	protected internal virtual bool IsFeatureSupported(Guid feature, Guid actualUuid)
	{
		int num = SupportedUuids.IndexOf(feature);
		if (num < 0)
		{
			return false;
		}
		return SupportedUuids.IndexOf(actualUuid) >= num;
	}

	public virtual ATN Deserialize(char[] data)
	{
		Reset(data);
		CheckVersion();
		CheckUUID();
		ATN aTN = ReadATN();
		ReadStates(aTN);
		ReadRules(aTN);
		ReadModes(aTN);
		IList<IntervalSet> sets = ReadSets(aTN);
		ReadEdges(aTN, sets);
		ReadDecisions(aTN);
		ReadLexerActions(aTN);
		MarkPrecedenceDecisions(aTN);
		if (deserializationOptions.VerifyAtn)
		{
			VerifyATN(aTN);
		}
		if (deserializationOptions.GenerateRuleBypassTransitions && aTN.grammarType == ATNType.Parser)
		{
			GenerateRuleBypassTransitions(aTN);
		}
		if (deserializationOptions.Optimize)
		{
			OptimizeATN(aTN);
		}
		IdentifyTailCalls(aTN);
		return aTN;
	}

	protected internal virtual void OptimizeATN(ATN atn)
	{
		int num;
		bool preserveOrder;
		do
		{
			num = 0;
			num += InlineSetRules(atn);
			num += CombineChainedEpsilons(atn);
			preserveOrder = atn.grammarType == ATNType.Lexer;
		}
		while (num + OptimizeSets(atn, preserveOrder) != 0);
		if (deserializationOptions.VerifyAtn)
		{
			VerifyATN(atn);
		}
	}

	protected internal virtual void GenerateRuleBypassTransitions(ATN atn)
	{
		atn.ruleToTokenType = new int[atn.ruleToStartState.Length];
		for (int i = 0; i < atn.ruleToStartState.Length; i++)
		{
			atn.ruleToTokenType[i] = atn.maxTokenType + i + 1;
		}
		for (int j = 0; j < atn.ruleToStartState.Length; j++)
		{
			BasicBlockStartState basicBlockStartState = new BasicBlockStartState();
			basicBlockStartState.ruleIndex = j;
			atn.AddState(basicBlockStartState);
			BlockEndState blockEndState = new BlockEndState();
			blockEndState.ruleIndex = j;
			atn.AddState(blockEndState);
			basicBlockStartState.endState = blockEndState;
			atn.DefineDecisionState(basicBlockStartState);
			blockEndState.startState = basicBlockStartState;
			Transition transition = null;
			ATNState aTNState;
			if (atn.ruleToStartState[j].isPrecedenceRule)
			{
				aTNState = null;
				foreach (ATNState state in atn.states)
				{
					if (state.ruleIndex == j && state is StarLoopEntryState)
					{
						ATNState target = state.Transition(state.NumberOfTransitions - 1).target;
						if (target is LoopEndState && target.epsilonOnlyTransitions && target.Transition(0).target is RuleStopState)
						{
							aTNState = state;
							break;
						}
					}
				}
				if (aTNState == null)
				{
					throw new NotSupportedException("Couldn't identify final state of the precedence rule prefix section.");
				}
				transition = ((StarLoopEntryState)aTNState).loopBackState.Transition(0);
			}
			else
			{
				aTNState = atn.ruleToStopState[j];
			}
			foreach (ATNState state2 in atn.states)
			{
				foreach (Transition transition2 in state2.transitions)
				{
					if (transition2 != transition && transition2.target == aTNState)
					{
						transition2.target = blockEndState;
					}
				}
			}
			while (atn.ruleToStartState[j].NumberOfTransitions > 0)
			{
				Transition e = atn.ruleToStartState[j].Transition(atn.ruleToStartState[j].NumberOfTransitions - 1);
				atn.ruleToStartState[j].RemoveTransition(atn.ruleToStartState[j].NumberOfTransitions - 1);
				basicBlockStartState.AddTransition(e);
			}
			atn.ruleToStartState[j].AddTransition(new EpsilonTransition(basicBlockStartState));
			blockEndState.AddTransition(new EpsilonTransition(aTNState));
			ATNState aTNState2 = new BasicState();
			atn.AddState(aTNState2);
			aTNState2.AddTransition(new AtomTransition(blockEndState, atn.ruleToTokenType[j]));
			basicBlockStartState.AddTransition(new EpsilonTransition(aTNState2));
		}
		if (deserializationOptions.VerifyAtn)
		{
			VerifyATN(atn);
		}
	}

	protected internal virtual void ReadLexerActions(ATN atn)
	{
		if (atn.grammarType != ATNType.Lexer)
		{
			return;
		}
		atn.lexerActions = new ILexerAction[ReadInt()];
		for (int i = 0; i < atn.lexerActions.Length; i++)
		{
			LexerActionType type = (LexerActionType)ReadInt();
			int num = ReadInt();
			if (num == 65535)
			{
				num = -1;
			}
			int num2 = ReadInt();
			if (num2 == 65535)
			{
				num2 = -1;
			}
			ILexerAction lexerAction = LexerActionFactory(type, num, num2);
			atn.lexerActions[i] = lexerAction;
		}
	}

	protected internal virtual void ReadDecisions(ATN atn)
	{
		int num = ReadInt();
		for (int i = 0; i < num; i++)
		{
			int index = ReadInt();
			DecisionState decisionState = (DecisionState)atn.states[index];
			atn.decisionToState.Add(decisionState);
			decisionState.decision = i;
		}
		atn.decisionToDFA = new DFA[num];
		for (int j = 0; j < num; j++)
		{
			atn.decisionToDFA[j] = new DFA(atn.decisionToState[j], j);
		}
	}

	protected internal virtual void ReadEdges(ATN atn, IList<IntervalSet> sets)
	{
		int num = ReadInt();
		for (int i = 0; i < num; i++)
		{
			int num2 = ReadInt();
			int trg = ReadInt();
			TransitionType type = (TransitionType)ReadInt();
			int arg = ReadInt();
			int arg2 = ReadInt();
			int arg3 = ReadInt();
			Transition e = EdgeFactory(atn, type, num2, trg, arg, arg2, arg3, sets);
			ATNState aTNState = atn.states[num2];
			aTNState.AddTransition(e);
		}
		foreach (ATNState state in atn.states)
		{
			for (int j = 0; j < state.NumberOfTransitions; j++)
			{
				Transition transition = state.Transition(j);
				if (transition is RuleTransition)
				{
					RuleTransition ruleTransition = (RuleTransition)transition;
					int outermostPrecedenceReturn = -1;
					if (atn.ruleToStartState[ruleTransition.target.ruleIndex].isPrecedenceRule && ruleTransition.precedence == 0)
					{
						outermostPrecedenceReturn = ruleTransition.target.ruleIndex;
					}
					EpsilonTransition e2 = new EpsilonTransition(ruleTransition.followState, outermostPrecedenceReturn);
					atn.ruleToStopState[ruleTransition.target.ruleIndex].AddTransition(e2);
				}
			}
		}
		foreach (ATNState state2 in atn.states)
		{
			if (state2 is BlockStartState)
			{
				if (((BlockStartState)state2).endState == null)
				{
					throw new InvalidOperationException();
				}
				if (((BlockStartState)state2).endState.startState != null)
				{
					throw new InvalidOperationException();
				}
				((BlockStartState)state2).endState.startState = (BlockStartState)state2;
			}
			else if (state2 is PlusLoopbackState)
			{
				PlusLoopbackState plusLoopbackState = (PlusLoopbackState)state2;
				for (int k = 0; k < plusLoopbackState.NumberOfTransitions; k++)
				{
					ATNState target = plusLoopbackState.Transition(k).target;
					if (target is PlusBlockStartState)
					{
						((PlusBlockStartState)target).loopBackState = plusLoopbackState;
					}
				}
			}
			else
			{
				if (!(state2 is StarLoopbackState))
				{
					continue;
				}
				StarLoopbackState starLoopbackState = (StarLoopbackState)state2;
				for (int l = 0; l < starLoopbackState.NumberOfTransitions; l++)
				{
					ATNState target2 = starLoopbackState.Transition(l).target;
					if (target2 is StarLoopEntryState)
					{
						((StarLoopEntryState)target2).loopBackState = starLoopbackState;
					}
				}
			}
		}
	}

	protected internal virtual IList<IntervalSet> ReadSets(ATN atn)
	{
		IList<IntervalSet> list = new List<IntervalSet>();
		int num = ReadInt();
		for (int i = 0; i < num; i++)
		{
			IntervalSet intervalSet = new IntervalSet();
			list.Add(intervalSet);
			int num2 = ReadInt();
			if (ReadInt() != 0)
			{
				intervalSet.Add(-1);
			}
			for (int j = 0; j < num2; j++)
			{
				intervalSet.Add(ReadInt(), ReadInt());
			}
		}
		return list;
	}

	protected internal virtual void ReadModes(ATN atn)
	{
		int num = ReadInt();
		for (int i = 0; i < num; i++)
		{
			int index = ReadInt();
			atn.modeToStartState.Add((TokensStartState)atn.states[index]);
		}
		atn.modeToDFA = new DFA[num];
		for (int j = 0; j < num; j++)
		{
			atn.modeToDFA[j] = new DFA(atn.modeToStartState[j]);
		}
	}

	protected internal virtual void ReadRules(ATN atn)
	{
		int num = ReadInt();
		if (atn.grammarType == ATNType.Lexer)
		{
			atn.ruleToTokenType = new int[num];
		}
		atn.ruleToStartState = new RuleStartState[num];
		for (int i = 0; i < num; i++)
		{
			int index = ReadInt();
			RuleStartState ruleStartState = (RuleStartState)atn.states[index];
			atn.ruleToStartState[i] = ruleStartState;
			if (atn.grammarType == ATNType.Lexer)
			{
				int num2 = ReadInt();
				if (num2 == 65535)
				{
					num2 = -1;
				}
				atn.ruleToTokenType[i] = num2;
			}
		}
		atn.ruleToStopState = new RuleStopState[num];
		foreach (ATNState state in atn.states)
		{
			if (state is RuleStopState)
			{
				RuleStopState ruleStopState = (RuleStopState)state;
				atn.ruleToStopState[state.ruleIndex] = ruleStopState;
				atn.ruleToStartState[state.ruleIndex].stopState = ruleStopState;
			}
		}
	}

	protected internal virtual void ReadStates(ATN atn)
	{
		IList<Tuple<LoopEndState, int>> list = new List<Tuple<LoopEndState, int>>();
		IList<Tuple<BlockStartState, int>> list2 = new List<Tuple<BlockStartState, int>>();
		int num = ReadInt();
		for (int i = 0; i < num; i++)
		{
			StateType stateType = (StateType)ReadInt();
			if (stateType == StateType.InvalidType)
			{
				atn.AddState(null);
				continue;
			}
			int num2 = ReadInt();
			if (num2 == 65535)
			{
				num2 = -1;
			}
			ATNState aTNState = StateFactory(stateType, num2);
			if (stateType == StateType.LoopEnd)
			{
				int item = ReadInt();
				list.Add(Tuple.Create((LoopEndState)aTNState, item));
			}
			else if (aTNState is BlockStartState)
			{
				int item2 = ReadInt();
				list2.Add(Tuple.Create((BlockStartState)aTNState, item2));
			}
			atn.AddState(aTNState);
		}
		foreach (Tuple<LoopEndState, int> item3 in list)
		{
			item3.Item1.loopBackState = atn.states[item3.Item2];
		}
		foreach (Tuple<BlockStartState, int> item4 in list2)
		{
			item4.Item1.endState = (BlockEndState)atn.states[item4.Item2];
		}
		int num3 = ReadInt();
		for (int j = 0; j < num3; j++)
		{
			int index = ReadInt();
			((DecisionState)atn.states[index]).nonGreedy = true;
		}
		int num4 = ReadInt();
		for (int k = 0; k < num4; k++)
		{
			int index2 = ReadInt();
			((RuleStartState)atn.states[index2]).isPrecedenceRule = true;
		}
	}

	protected internal virtual ATN ReadATN()
	{
		ATNType grammarType = (ATNType)ReadInt();
		int maxTokenType = ReadInt();
		return new ATN(grammarType, maxTokenType);
	}

	protected internal virtual void CheckUUID()
	{
		Guid guid = ReadUUID();
		if (!SupportedUuids.Contains(guid))
		{
			string message = string.Format(CultureInfo.CurrentCulture, "Could not deserialize ATN with UUID {0} (expected {1} or a legacy UUID).", guid, SerializedUuid);
			throw new NotSupportedException(message);
		}
	}

	protected internal virtual void CheckVersion()
	{
		int num = ReadInt();
		if (num != SerializedVersion)
		{
			string message = string.Format(CultureInfo.CurrentCulture, "Could not deserialize ATN with version {0} (expected {1}).", num, SerializedVersion);
			throw new NotSupportedException(message);
		}
	}

	protected internal virtual void Reset(char[] data)
	{
		this.data = new char[data.Length];
		this.data[0] = data[0];
		for (int i = 1; i < data.Length; i++)
		{
			this.data[i] = (char)(data[i] - 2);
		}
		p = 0;
	}

	protected internal virtual void MarkPrecedenceDecisions(ATN atn)
	{
		foreach (ATNState state in atn.states)
		{
			if (state is StarLoopEntryState && atn.ruleToStartState[state.ruleIndex].isPrecedenceRule)
			{
				ATNState target = state.Transition(state.NumberOfTransitions - 1).target;
				if (target is LoopEndState && target.epsilonOnlyTransitions && target.Transition(0).target is RuleStopState)
				{
					((StarLoopEntryState)state).precedenceRuleDecision = true;
				}
			}
		}
	}

	protected internal virtual void VerifyATN(ATN atn)
	{
		foreach (ATNState state in atn.states)
		{
			if (state == null)
			{
				continue;
			}
			CheckCondition(state.OnlyHasEpsilonTransitions || state.NumberOfTransitions <= 1);
			if (state is PlusBlockStartState)
			{
				CheckCondition(((PlusBlockStartState)state).loopBackState != null);
			}
			if (state is StarLoopEntryState)
			{
				StarLoopEntryState starLoopEntryState = (StarLoopEntryState)state;
				CheckCondition(starLoopEntryState.loopBackState != null);
				CheckCondition(starLoopEntryState.NumberOfTransitions == 2);
				if (starLoopEntryState.Transition(0).target is StarBlockStartState)
				{
					CheckCondition(starLoopEntryState.Transition(1).target is LoopEndState);
					CheckCondition(!starLoopEntryState.nonGreedy);
				}
				else
				{
					if (!(starLoopEntryState.Transition(0).target is LoopEndState))
					{
						throw new InvalidOperationException();
					}
					CheckCondition(starLoopEntryState.Transition(1).target is StarBlockStartState);
					CheckCondition(starLoopEntryState.nonGreedy);
				}
			}
			if (state is StarLoopbackState)
			{
				CheckCondition(state.NumberOfTransitions == 1);
				CheckCondition(state.Transition(0).target is StarLoopEntryState);
			}
			if (state is LoopEndState)
			{
				CheckCondition(((LoopEndState)state).loopBackState != null);
			}
			if (state is RuleStartState)
			{
				CheckCondition(((RuleStartState)state).stopState != null);
			}
			if (state is BlockStartState)
			{
				CheckCondition(((BlockStartState)state).endState != null);
			}
			if (state is BlockEndState)
			{
				CheckCondition(((BlockEndState)state).startState != null);
			}
			if (state is DecisionState)
			{
				DecisionState decisionState = (DecisionState)state;
				CheckCondition(decisionState.NumberOfTransitions <= 1 || decisionState.decision >= 0);
			}
			else
			{
				CheckCondition(state.NumberOfTransitions <= 1 || state is RuleStopState);
			}
		}
	}

	protected internal virtual void CheckCondition(bool condition)
	{
		CheckCondition(condition, null);
	}

	protected internal virtual void CheckCondition(bool condition, string message)
	{
		if (!condition)
		{
			throw new InvalidOperationException(message);
		}
	}

	private static int InlineSetRules(ATN atn)
	{
		int num = 0;
		Transition[] array = new Transition[atn.ruleToStartState.Length];
		for (int i = 0; i < atn.ruleToStartState.Length; i++)
		{
			RuleStartState ruleStartState = atn.ruleToStartState[i];
			ATNState aTNState = ruleStartState;
			while (aTNState.OnlyHasEpsilonTransitions && aTNState.NumberOfOptimizedTransitions == 1 && aTNState.GetOptimizedTransition(0).TransitionType == TransitionType.Epsilon)
			{
				aTNState = aTNState.GetOptimizedTransition(0).target;
			}
			if (aTNState.NumberOfOptimizedTransitions != 1)
			{
				continue;
			}
			Transition optimizedTransition = aTNState.GetOptimizedTransition(0);
			ATNState target = optimizedTransition.target;
			if (!optimizedTransition.IsEpsilon && target.OnlyHasEpsilonTransitions && target.NumberOfOptimizedTransitions == 1 && target.GetOptimizedTransition(0).target is RuleStopState)
			{
				switch (optimizedTransition.TransitionType)
				{
				case TransitionType.Range:
				case TransitionType.Atom:
				case TransitionType.Set:
					array[i] = optimizedTransition;
					break;
				}
			}
		}
		for (int j = 0; j < atn.states.Count; j++)
		{
			ATNState aTNState2 = atn.states[j];
			if (aTNState2.ruleIndex < 0)
			{
				continue;
			}
			IList<Transition> list = null;
			for (int k = 0; k < aTNState2.NumberOfOptimizedTransitions; k++)
			{
				Transition optimizedTransition2 = aTNState2.GetOptimizedTransition(k);
				if (!(optimizedTransition2 is RuleTransition))
				{
					list?.Add(optimizedTransition2);
					continue;
				}
				RuleTransition ruleTransition = (RuleTransition)optimizedTransition2;
				Transition transition = array[ruleTransition.target.ruleIndex];
				if (transition == null)
				{
					list?.Add(optimizedTransition2);
					continue;
				}
				if (list == null)
				{
					list = new List<Transition>();
					for (int l = 0; l < k; l++)
					{
						list.Add(aTNState2.GetOptimizedTransition(k));
					}
				}
				num++;
				ATNState followState = ruleTransition.followState;
				ATNState aTNState3 = new BasicState();
				aTNState3.SetRuleIndex(followState.ruleIndex);
				atn.AddState(aTNState3);
				list.Add(new EpsilonTransition(aTNState3));
				switch (transition.TransitionType)
				{
				case TransitionType.Atom:
					aTNState3.AddTransition(new AtomTransition(followState, ((AtomTransition)transition).token));
					break;
				case TransitionType.Range:
					aTNState3.AddTransition(new RangeTransition(followState, ((RangeTransition)transition).from, ((RangeTransition)transition).to));
					break;
				case TransitionType.Set:
					aTNState3.AddTransition(new SetTransition(followState, transition.Label));
					break;
				default:
					throw new NotSupportedException();
				}
			}
			if (list == null)
			{
				continue;
			}
			if (aTNState2.IsOptimized)
			{
				while (aTNState2.NumberOfOptimizedTransitions > 0)
				{
					aTNState2.RemoveOptimizedTransition(aTNState2.NumberOfOptimizedTransitions - 1);
				}
			}
			foreach (Transition item in list)
			{
				aTNState2.AddOptimizedTransition(item);
			}
		}
		return num;
	}

	private static int CombineChainedEpsilons(ATN atn)
	{
		int num = 0;
		foreach (ATNState state in atn.states)
		{
			if (!state.OnlyHasEpsilonTransitions || state is RuleStopState)
			{
				continue;
			}
			IList<Transition> list = null;
			for (int i = 0; i < state.NumberOfOptimizedTransitions; i++)
			{
				Transition optimizedTransition = state.GetOptimizedTransition(i);
				ATNState target = optimizedTransition.target;
				if (optimizedTransition.TransitionType != TransitionType.Epsilon || ((EpsilonTransition)optimizedTransition).OutermostPrecedenceReturn != -1 || target.StateType != StateType.Basic || !target.OnlyHasEpsilonTransitions)
				{
					list?.Add(optimizedTransition);
					continue;
				}
				int num2 = 0;
				while (true)
				{
					if (num2 < target.NumberOfOptimizedTransitions)
					{
						if (target.GetOptimizedTransition(num2).TransitionType != TransitionType.Epsilon || ((EpsilonTransition)target.GetOptimizedTransition(num2)).OutermostPrecedenceReturn != -1)
						{
							list?.Add(optimizedTransition);
							break;
						}
						num2++;
						continue;
					}
					num++;
					if (list == null)
					{
						list = new List<Transition>();
						for (int j = 0; j < i; j++)
						{
							list.Add(state.GetOptimizedTransition(j));
						}
					}
					for (int k = 0; k < target.NumberOfOptimizedTransitions; k++)
					{
						ATNState target2 = target.GetOptimizedTransition(k).target;
						list.Add(new EpsilonTransition(target2));
					}
					break;
				}
			}
			if (list == null)
			{
				continue;
			}
			if (state.IsOptimized)
			{
				while (state.NumberOfOptimizedTransitions > 0)
				{
					state.RemoveOptimizedTransition(state.NumberOfOptimizedTransitions - 1);
				}
			}
			foreach (Transition item in list)
			{
				state.AddOptimizedTransition(item);
			}
		}
		return num;
	}

	private static int OptimizeSets(ATN atn, bool preserveOrder)
	{
		if (preserveOrder)
		{
			return 0;
		}
		int num = 0;
		IList<DecisionState> decisionToState = atn.decisionToState;
		foreach (DecisionState item in decisionToState)
		{
			IntervalSet intervalSet = new IntervalSet();
			for (int i = 0; i < item.NumberOfOptimizedTransitions; i++)
			{
				Transition optimizedTransition = item.GetOptimizedTransition(i);
				if (optimizedTransition is EpsilonTransition && optimizedTransition.target.NumberOfOptimizedTransitions == 1)
				{
					Transition optimizedTransition2 = optimizedTransition.target.GetOptimizedTransition(0);
					if (optimizedTransition2.target is BlockEndState && !(optimizedTransition2 is NotSetTransition) && (optimizedTransition2 is AtomTransition || optimizedTransition2 is RangeTransition || optimizedTransition2 is SetTransition))
					{
						intervalSet.Add(i);
					}
				}
			}
			if (intervalSet.Count <= 1)
			{
				continue;
			}
			IList<Transition> list = new List<Transition>();
			for (int j = 0; j < item.NumberOfOptimizedTransitions; j++)
			{
				if (!intervalSet.Contains(j))
				{
					list.Add(item.GetOptimizedTransition(j));
				}
			}
			ATNState target = item.GetOptimizedTransition(intervalSet.MinElement).target.GetOptimizedTransition(0).target;
			IntervalSet intervalSet2 = new IntervalSet();
			for (int k = 0; k < intervalSet.GetIntervals().Count; k++)
			{
				Interval interval = intervalSet.GetIntervals()[k];
				for (int l = interval.a; l <= interval.b; l++)
				{
					Transition optimizedTransition3 = item.GetOptimizedTransition(l).target.GetOptimizedTransition(0);
					if (optimizedTransition3 is NotSetTransition)
					{
						throw new NotSupportedException("Not yet implemented.");
					}
					intervalSet2.AddAll(optimizedTransition3.Label);
				}
			}
			Transition e;
			if (intervalSet2.GetIntervals().Count == 1)
			{
				if (intervalSet2.Count == 1)
				{
					e = new AtomTransition(target, intervalSet2.MinElement);
				}
				else
				{
					Interval interval2 = intervalSet2.GetIntervals()[0];
					e = new RangeTransition(target, interval2.a, interval2.b);
				}
			}
			else
			{
				e = new SetTransition(target, intervalSet2);
			}
			ATNState aTNState = new BasicState();
			aTNState.SetRuleIndex(item.ruleIndex);
			atn.AddState(aTNState);
			aTNState.AddTransition(e);
			list.Add(new EpsilonTransition(aTNState));
			num += item.NumberOfOptimizedTransitions - list.Count;
			if (item.IsOptimized)
			{
				while (item.NumberOfOptimizedTransitions > 0)
				{
					item.RemoveOptimizedTransition(item.NumberOfOptimizedTransitions - 1);
				}
			}
			foreach (Transition item2 in list)
			{
				item.AddOptimizedTransition(item2);
			}
		}
		return num;
	}

	private static void IdentifyTailCalls(ATN atn)
	{
		foreach (ATNState state in atn.states)
		{
			foreach (Transition transition in state.transitions)
			{
				if (transition is RuleTransition)
				{
					RuleTransition ruleTransition = (RuleTransition)transition;
					ruleTransition.tailCall = TestTailCall(atn, ruleTransition, optimizedPath: false);
					ruleTransition.optimizedTailCall = TestTailCall(atn, ruleTransition, optimizedPath: true);
				}
			}
			if (!state.IsOptimized)
			{
				continue;
			}
			foreach (Transition optimizedTransition in state.optimizedTransitions)
			{
				if (optimizedTransition is RuleTransition)
				{
					RuleTransition ruleTransition2 = (RuleTransition)optimizedTransition;
					ruleTransition2.tailCall = TestTailCall(atn, ruleTransition2, optimizedPath: false);
					ruleTransition2.optimizedTailCall = TestTailCall(atn, ruleTransition2, optimizedPath: true);
				}
			}
		}
	}

	private static bool TestTailCall(ATN atn, RuleTransition transition, bool optimizedPath)
	{
		if (!optimizedPath && transition.tailCall)
		{
			return true;
		}
		if (optimizedPath && transition.optimizedTailCall)
		{
			return true;
		}
		BitSet bitSet = new BitSet(atn.states.Count);
		Stack<ATNState> stack = new Stack<ATNState>();
		stack.Push(transition.followState);
		while (stack.Count > 0)
		{
			ATNState aTNState = stack.Pop();
			if (bitSet.Get(aTNState.stateNumber) || aTNState is RuleStopState)
			{
				continue;
			}
			if (!aTNState.OnlyHasEpsilonTransitions)
			{
				return false;
			}
			IList<Transition> list = (optimizedPath ? aTNState.optimizedTransitions : aTNState.transitions);
			foreach (Transition item in list)
			{
				if (item.TransitionType != TransitionType.Epsilon)
				{
					return false;
				}
				stack.Push(item.target);
			}
		}
		return true;
	}

	protected internal int ReadInt()
	{
		return data[p++];
	}

	protected internal int ReadInt32()
	{
		return (int)(data[p++] | ((uint)data[p++] << 16));
	}

	protected internal long ReadLong()
	{
		long num = ReadInt32() & 0xFFFFFFFFu;
		return num | ((long)ReadInt32() << 32);
	}

	protected internal Guid ReadUUID()
	{
		byte[] bytes = BitConverter.GetBytes(ReadLong());
		Array.Reverse(bytes);
		short c = (short)ReadInt();
		short b = (short)ReadInt();
		int a = ReadInt32();
		return new Guid(a, b, c, bytes);
	}

	[return: NotNull]
	protected internal virtual Transition EdgeFactory(ATN atn, TransitionType type, int src, int trg, int arg1, int arg2, int arg3, IList<IntervalSet> sets)
	{
		ATNState aTNState = atn.states[trg];
		switch (type)
		{
		case TransitionType.Epsilon:
			return new EpsilonTransition(aTNState);
		case TransitionType.Range:
			if (arg3 != 0)
			{
				return new RangeTransition(aTNState, -1, arg2);
			}
			return new RangeTransition(aTNState, arg1, arg2);
		case TransitionType.Rule:
			return new RuleTransition((RuleStartState)atn.states[arg1], arg2, arg3, aTNState);
		case TransitionType.Predicate:
			return new PredicateTransition(aTNState, arg1, arg2, arg3 != 0);
		case TransitionType.Precedence:
			return new PrecedencePredicateTransition(aTNState, arg1);
		case TransitionType.Atom:
			if (arg3 != 0)
			{
				return new AtomTransition(aTNState, -1);
			}
			return new AtomTransition(aTNState, arg1);
		case TransitionType.Action:
			return new ActionTransition(aTNState, arg1, arg2, arg3 != 0);
		case TransitionType.Set:
			return new SetTransition(aTNState, sets[arg1]);
		case TransitionType.NotSet:
			return new NotSetTransition(aTNState, sets[arg1]);
		case TransitionType.Wildcard:
			return new WildcardTransition(aTNState);
		default:
			throw new ArgumentException("The specified transition type is not valid.");
		}
	}

	protected internal virtual ATNState StateFactory(StateType type, int ruleIndex)
	{
		ATNState aTNState;
		switch (type)
		{
		case StateType.InvalidType:
			return null;
		case StateType.Basic:
			aTNState = new BasicState();
			break;
		case StateType.RuleStart:
			aTNState = new RuleStartState();
			break;
		case StateType.BlockStart:
			aTNState = new BasicBlockStartState();
			break;
		case StateType.PlusBlockStart:
			aTNState = new PlusBlockStartState();
			break;
		case StateType.StarBlockStart:
			aTNState = new StarBlockStartState();
			break;
		case StateType.TokenStart:
			aTNState = new TokensStartState();
			break;
		case StateType.RuleStop:
			aTNState = new RuleStopState();
			break;
		case StateType.BlockEnd:
			aTNState = new BlockEndState();
			break;
		case StateType.StarLoopBack:
			aTNState = new StarLoopbackState();
			break;
		case StateType.StarLoopEntry:
			aTNState = new StarLoopEntryState();
			break;
		case StateType.PlusLoopBack:
			aTNState = new PlusLoopbackState();
			break;
		case StateType.LoopEnd:
			aTNState = new LoopEndState();
			break;
		default:
		{
			string message = string.Format(CultureInfo.CurrentCulture, "The specified state type {0} is not valid.", type);
			throw new ArgumentException(message);
		}
		}
		aTNState.ruleIndex = ruleIndex;
		return aTNState;
	}

	protected internal virtual ILexerAction LexerActionFactory(LexerActionType type, int data1, int data2)
	{
		switch (type)
		{
		case LexerActionType.Channel:
			return new LexerChannelAction(data1);
		case LexerActionType.Custom:
			return new LexerCustomAction(data1, data2);
		case LexerActionType.Mode:
			return new LexerModeAction(data1);
		case LexerActionType.More:
			return LexerMoreAction.Instance;
		case LexerActionType.PopMode:
			return LexerPopModeAction.Instance;
		case LexerActionType.PushMode:
			return new LexerPushModeAction(data1);
		case LexerActionType.Skip:
			return LexerSkipAction.Instance;
		case LexerActionType.Type:
			return new LexerTypeAction(data1);
		default:
		{
			string message = string.Format(CultureInfo.CurrentCulture, "The specified lexer action type {0} is not valid.", type);
			throw new ArgumentException(message);
		}
		}
	}
}
