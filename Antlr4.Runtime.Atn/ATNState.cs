using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public abstract class ATNState
{
	public const int InitialNumTransitions = 4;

	public static readonly ReadOnlyCollection<string> serializationNames = new ReadOnlyCollection<string>(Arrays.AsList<string>("INVALID", "BASIC", "RULE_START", "BLOCK_START", "PLUS_BLOCK_START", "STAR_BLOCK_START", "TOKEN_START", "RULE_STOP", "BLOCK_END", "STAR_LOOP_BACK", "STAR_LOOP_ENTRY", "PLUS_LOOP_BACK", "LOOP_END"));

	public const int InvalidStateNumber = -1;

	public ATN atn;

	public int stateNumber = -1;

	public int ruleIndex;

	public bool epsilonOnlyTransitions;

	protected internal readonly List<Transition> transitions = new List<Transition>(4);

	protected internal List<Transition> optimizedTransitions;

	public IntervalSet nextTokenWithinRule;

	public virtual int NonStopStateNumber => stateNumber;

	public virtual bool IsNonGreedyExitState => false;

	public virtual Transition[] TransitionsArray => transitions.ToArray();

	public virtual int NumberOfTransitions => transitions.Count;

	public abstract StateType StateType { get; }

	public bool OnlyHasEpsilonTransitions => epsilonOnlyTransitions;

	public virtual bool IsOptimized => optimizedTransitions != transitions;

	public virtual int NumberOfOptimizedTransitions => optimizedTransitions.Count;

	public override int GetHashCode()
	{
		return stateNumber;
	}

	public override bool Equals(object o)
	{
		if (o is ATNState)
		{
			return stateNumber == ((ATNState)o).stateNumber;
		}
		return false;
	}

	public override string ToString()
	{
		return stateNumber.ToString();
	}

	public virtual void AddTransition(Transition e)
	{
		AddTransition(transitions.Count, e);
	}

	public virtual void AddTransition(int index, Transition e)
	{
		if (transitions.Count == 0)
		{
			epsilonOnlyTransitions = e.IsEpsilon;
		}
		else if (epsilonOnlyTransitions != e.IsEpsilon)
		{
			Console.Error.WriteLine("ATN state {0} has both epsilon and non-epsilon transitions.", stateNumber);
			epsilonOnlyTransitions = false;
		}
		transitions.Insert(index, e);
	}

	public virtual Transition Transition(int i)
	{
		return transitions[i];
	}

	public virtual void SetTransition(int i, Transition e)
	{
		transitions[i] = e;
	}

	public virtual void RemoveTransition(int index)
	{
		transitions.RemoveAt(index);
	}

	public virtual void SetRuleIndex(int ruleIndex)
	{
		this.ruleIndex = ruleIndex;
	}

	public virtual Transition GetOptimizedTransition(int i)
	{
		return optimizedTransitions[i];
	}

	public virtual void AddOptimizedTransition(Transition e)
	{
		if (!IsOptimized)
		{
			optimizedTransitions = new List<Transition>();
		}
		optimizedTransitions.Add(e);
	}

	public virtual void SetOptimizedTransition(int i, Transition e)
	{
		if (!IsOptimized)
		{
			throw new InvalidOperationException();
		}
		optimizedTransitions[i] = e;
	}

	public virtual void RemoveOptimizedTransition(int i)
	{
		if (!IsOptimized)
		{
			throw new InvalidOperationException();
		}
		optimizedTransitions.RemoveAt(i);
	}

	public ATNState()
	{
		optimizedTransitions = transitions;
	}
}
