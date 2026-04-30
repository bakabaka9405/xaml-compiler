using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public class LL1Analyzer
{
	public const int HitPred = 0;

	[NotNull]
	public readonly ATN atn;

	public LL1Analyzer(ATN atn)
	{
		this.atn = atn;
	}

	[return: Nullable]
	public virtual IntervalSet[] GetDecisionLookahead(ATNState s)
	{
		if (s == null)
		{
			return null;
		}
		IntervalSet[] array = new IntervalSet[s.NumberOfTransitions];
		for (int i = 0; i < s.NumberOfTransitions; i++)
		{
			array[i] = new IntervalSet();
			HashSet<ATNConfig> lookBusy = new HashSet<ATNConfig>();
			bool seeThruPreds = false;
			Look(s.Transition(i).target, null, PredictionContext.EmptyLocal, array[i], lookBusy, new BitSet(), seeThruPreds, addEOF: false);
			if (array[i].Count == 0 || array[i].Contains(0))
			{
				array[i] = null;
			}
		}
		return array;
	}

	[return: NotNull]
	public virtual IntervalSet Look(ATNState s, PredictionContext ctx)
	{
		return Look(s, s.atn.ruleToStopState[s.ruleIndex], ctx);
	}

	[return: NotNull]
	public virtual IntervalSet Look(ATNState s, ATNState stopState, PredictionContext ctx)
	{
		IntervalSet intervalSet = new IntervalSet();
		bool seeThruPreds = true;
		bool addEOF = true;
		Look(s, stopState, ctx, intervalSet, new HashSet<ATNConfig>(), new BitSet(), seeThruPreds, addEOF);
		return intervalSet;
	}

	protected internal virtual void Look(ATNState s, ATNState stopState, PredictionContext ctx, IntervalSet look, HashSet<ATNConfig> lookBusy, BitSet calledRuleStack, bool seeThruPreds, bool addEOF)
	{
		ATNConfig item = ATNConfig.Create(s, 0, ctx);
		if (!lookBusy.Add(item))
		{
			return;
		}
		if (s == stopState)
		{
			if (PredictionContext.IsEmptyLocal(ctx))
			{
				look.Add(-2);
				return;
			}
			if (ctx.IsEmpty)
			{
				if (addEOF)
				{
					look.Add(-1);
				}
				return;
			}
		}
		if (s is RuleStopState)
		{
			if (ctx.IsEmpty && !PredictionContext.IsEmptyLocal(ctx))
			{
				if (addEOF)
				{
					look.Add(-1);
				}
				return;
			}
			bool flag = calledRuleStack.Get(s.ruleIndex);
			try
			{
				calledRuleStack.Clear(s.ruleIndex);
				for (int i = 0; i < ctx.Size; i++)
				{
					if (ctx.GetReturnState(i) != int.MaxValue)
					{
						ATNState s2 = atn.states[ctx.GetReturnState(i)];
						Look(s2, stopState, ctx.GetParent(i), look, lookBusy, calledRuleStack, seeThruPreds, addEOF);
					}
				}
			}
			finally
			{
				if (flag)
				{
					calledRuleStack.Set(s.ruleIndex);
				}
			}
		}
		int numberOfTransitions = s.NumberOfTransitions;
		for (int j = 0; j < numberOfTransitions; j++)
		{
			Transition transition = s.Transition(j);
			if (transition is RuleTransition)
			{
				RuleTransition ruleTransition = (RuleTransition)transition;
				if (!calledRuleStack.Get(ruleTransition.ruleIndex))
				{
					PredictionContext child = ctx.GetChild(ruleTransition.followState.stateNumber);
					try
					{
						calledRuleStack.Set(ruleTransition.ruleIndex);
						Look(transition.target, stopState, child, look, lookBusy, calledRuleStack, seeThruPreds, addEOF);
					}
					finally
					{
						calledRuleStack.Clear(ruleTransition.ruleIndex);
					}
				}
				continue;
			}
			if (transition is AbstractPredicateTransition)
			{
				if (seeThruPreds)
				{
					Look(transition.target, stopState, ctx, look, lookBusy, calledRuleStack, seeThruPreds, addEOF);
				}
				else
				{
					look.Add(0);
				}
				continue;
			}
			if (transition.IsEpsilon)
			{
				Look(transition.target, stopState, ctx, look, lookBusy, calledRuleStack, seeThruPreds, addEOF);
				continue;
			}
			if (transition.GetType() == typeof(WildcardTransition))
			{
				look.AddAll(IntervalSet.Of(1, atn.maxTokenType));
				continue;
			}
			IntervalSet intervalSet = transition.Label;
			if (intervalSet != null)
			{
				if (transition is NotSetTransition)
				{
					intervalSet = intervalSet.Complement(IntervalSet.Of(1, atn.maxTokenType));
				}
				look.AddAll(intervalSet);
			}
		}
	}
}
