using System;
using System.Collections.Concurrent;
using System.Threading;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Dfa;

public class DFA
{
	[NotNull]
	public readonly ConcurrentDictionary<DFAState, DFAState> states = new ConcurrentDictionary<DFAState, DFAState>();

	[NotNull]
	public readonly AtomicReference<DFAState> s0 = new AtomicReference<DFAState>();

	[NotNull]
	public readonly AtomicReference<DFAState> s0full = new AtomicReference<DFAState>();

	public readonly int decision;

	[NotNull]
	public readonly ATNState atnStartState;

	private int nextStateNumber;

	private readonly int minDfaEdge;

	private readonly int maxDfaEdge;

	[NotNull]
	private static readonly EmptyEdgeMap<DFAState> emptyPrecedenceEdges = new EmptyEdgeMap<DFAState>(0, 200);

	[NotNull]
	private readonly EmptyEdgeMap<DFAState> emptyEdgeMap;

	[NotNull]
	private readonly EmptyEdgeMap<DFAState> emptyContextEdgeMap;

	private volatile bool precedenceDfa;

	public int MinDfaEdge => minDfaEdge;

	public int MaxDfaEdge => maxDfaEdge;

	public virtual EmptyEdgeMap<DFAState> EmptyEdgeMap => emptyEdgeMap;

	public virtual EmptyEdgeMap<DFAState> EmptyContextEdgeMap => emptyContextEdgeMap;

	public bool IsPrecedenceDfa
	{
		get
		{
			return precedenceDfa;
		}
		set
		{
			bool flag = value;
			lock (this)
			{
				if (precedenceDfa != flag)
				{
					states.Clear();
					if (flag)
					{
						s0.Set(new DFAState(emptyPrecedenceEdges, EmptyContextEdgeMap, new ATNConfigSet()));
						s0full.Set(new DFAState(emptyPrecedenceEdges, EmptyContextEdgeMap, new ATNConfigSet()));
					}
					else
					{
						s0.Set(null);
						s0full.Set(null);
					}
					precedenceDfa = flag;
				}
			}
		}
	}

	public virtual bool IsEmpty
	{
		get
		{
			if (IsPrecedenceDfa)
			{
				if (s0.Get().EdgeMap.Count == 0)
				{
					return s0full.Get().EdgeMap.Count == 0;
				}
				return false;
			}
			if (s0.Get() == null)
			{
				return s0full.Get() == null;
			}
			return false;
		}
	}

	public virtual bool IsContextSensitive
	{
		get
		{
			if (IsPrecedenceDfa)
			{
				return s0full.Get().EdgeMap.Count != 0;
			}
			return s0full.Get() != null;
		}
	}

	public DFA(ATNState atnStartState)
		: this(atnStartState, 0)
	{
	}

	public DFA(ATNState atnStartState, int decision)
	{
		this.atnStartState = atnStartState;
		this.decision = decision;
		if (this.atnStartState.atn.grammarType == ATNType.Lexer)
		{
			minDfaEdge = 0;
			maxDfaEdge = 127;
		}
		else
		{
			minDfaEdge = -1;
			maxDfaEdge = atnStartState.atn.maxTokenType;
		}
		emptyEdgeMap = new EmptyEdgeMap<DFAState>(minDfaEdge, maxDfaEdge);
		emptyContextEdgeMap = new EmptyEdgeMap<DFAState>(-1, atnStartState.atn.states.Count - 1);
	}

	public DFAState GetPrecedenceStartState(int precedence, bool fullContext)
	{
		if (!IsPrecedenceDfa)
		{
			throw new InvalidOperationException("Only precedence DFAs may contain a precedence start state.");
		}
		if (fullContext)
		{
			return s0full.Get().GetTarget(precedence);
		}
		return s0.Get().GetTarget(precedence);
	}

	public void SetPrecedenceStartState(int precedence, bool fullContext, DFAState startState)
	{
		if (!IsPrecedenceDfa)
		{
			throw new InvalidOperationException("Only precedence DFAs may contain a precedence start state.");
		}
		if (precedence < 0)
		{
			return;
		}
		if (fullContext)
		{
			lock (s0full)
			{
				s0full.Get().SetTarget(precedence, startState);
				return;
			}
		}
		lock (s0)
		{
			s0.Get().SetTarget(precedence, startState);
		}
	}

	public virtual DFAState AddState(DFAState state)
	{
		state.stateNumber = Interlocked.Increment(ref nextStateNumber) - 1;
		return states.GetOrAdd(state, state);
	}

	public override string ToString()
	{
		return ToString(Vocabulary.EmptyVocabulary);
	}

	public virtual string ToString(IVocabulary vocabulary)
	{
		if (s0.Get() == null)
		{
			return string.Empty;
		}
		DFASerializer dFASerializer = new DFASerializer(this, vocabulary);
		return dFASerializer.ToString();
	}

	public virtual string ToString(IVocabulary vocabulary, string[] ruleNames)
	{
		if (s0.Get() == null)
		{
			return string.Empty;
		}
		DFASerializer dFASerializer = new DFASerializer(this, vocabulary, ruleNames, atnStartState.atn);
		return dFASerializer.ToString();
	}

	public virtual string ToLexerString()
	{
		if (s0.Get() == null)
		{
			return string.Empty;
		}
		DFASerializer dFASerializer = new LexerDFASerializer(this);
		return dFASerializer.ToString();
	}
}
