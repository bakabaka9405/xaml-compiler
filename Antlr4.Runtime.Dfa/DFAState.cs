using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Dfa;

public class DFAState
{
	public class PredPrediction
	{
		[NotNull]
		public SemanticContext pred;

		public int alt;

		public PredPrediction(SemanticContext pred, int alt)
		{
			this.alt = alt;
			this.pred = pred;
		}

		public override string ToString()
		{
			return "(" + pred?.ToString() + ", " + alt + ")";
		}
	}

	public int stateNumber = -1;

	[NotNull]
	public readonly ATNConfigSet configs;

	[NotNull]
	private volatile AbstractEdgeMap<DFAState> edges;

	private AcceptStateInfo acceptStateInfo;

	[NotNull]
	private volatile AbstractEdgeMap<DFAState> contextEdges;

	[Nullable]
	private BitSet contextSymbols;

	[Nullable]
	public PredPrediction[] predicates;

	public bool IsContextSensitive => contextSymbols != null;

	public AcceptStateInfo AcceptStateInfo
	{
		get
		{
			return acceptStateInfo;
		}
		set
		{
			acceptStateInfo = value;
		}
	}

	public bool IsAcceptState => acceptStateInfo != null;

	public int Prediction
	{
		get
		{
			if (acceptStateInfo == null)
			{
				return 0;
			}
			return acceptStateInfo.Prediction;
		}
	}

	public LexerActionExecutor LexerActionExecutor
	{
		get
		{
			if (acceptStateInfo == null)
			{
				return null;
			}
			return acceptStateInfo.LexerActionExecutor;
		}
	}

	public virtual IDictionary<int, DFAState> EdgeMap => edges.ToMap();

	public virtual IDictionary<int, DFAState> ContextEdgeMap
	{
		get
		{
			IDictionary<int, DFAState> dictionary = contextEdges.ToMap();
			if (dictionary.ContainsKey(-1))
			{
				if (dictionary.Count == 1)
				{
					return Collections.SingletonMap(int.MaxValue, dictionary[-1]);
				}
				Dictionary<int, DFAState> dictionary2 = new Dictionary<int, DFAState>(dictionary);
				dictionary2.Add(int.MaxValue, dictionary2[-1]);
				dictionary2.Remove(-1);
				dictionary = new SortedDictionary<int, DFAState>(dictionary2);
			}
			return dictionary;
		}
	}

	public DFAState(DFA dfa, ATNConfigSet configs)
		: this(dfa.EmptyEdgeMap, dfa.EmptyContextEdgeMap, configs)
	{
	}

	public DFAState(EmptyEdgeMap<DFAState> emptyEdges, EmptyEdgeMap<DFAState> emptyContextEdges, ATNConfigSet configs)
	{
		this.configs = configs;
		edges = emptyEdges;
		contextEdges = emptyContextEdges;
	}

	public bool IsContextSymbol(int symbol)
	{
		if (!IsContextSensitive || symbol < edges.minIndex)
		{
			return false;
		}
		return contextSymbols.Get(symbol - edges.minIndex);
	}

	public void SetContextSymbol(int symbol)
	{
		if (symbol >= edges.minIndex)
		{
			contextSymbols.Set(symbol - edges.minIndex);
		}
	}

	public virtual void SetContextSensitive(ATN atn)
	{
		if (IsContextSensitive)
		{
			return;
		}
		lock (this)
		{
			if (contextSymbols == null)
			{
				contextSymbols = new BitSet();
			}
		}
	}

	public virtual DFAState GetTarget(int symbol)
	{
		return edges[symbol];
	}

	public virtual void SetTarget(int symbol, DFAState target)
	{
		edges = edges.Put(symbol, target);
	}

	public virtual DFAState GetContextTarget(int invokingState)
	{
		lock (this)
		{
			if (invokingState == int.MaxValue)
			{
				invokingState = -1;
			}
			return contextEdges[invokingState];
		}
	}

	public virtual void SetContextTarget(int invokingState, DFAState target)
	{
		lock (this)
		{
			if (!IsContextSensitive)
			{
				throw new InvalidOperationException("The state is not context sensitive.");
			}
			if (invokingState == int.MaxValue)
			{
				invokingState = -1;
			}
			contextEdges = contextEdges.Put(invokingState, target);
		}
	}

	public override int GetHashCode()
	{
		int hash = MurmurHash.Initialize(7);
		hash = MurmurHash.Update(hash, configs.GetHashCode());
		return MurmurHash.Finish(hash, 1);
	}

	public override bool Equals(object o)
	{
		if (this == o)
		{
			return true;
		}
		if (!(o is DFAState))
		{
			return false;
		}
		DFAState dFAState = (DFAState)o;
		return configs.Equals(dFAState.configs);
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(stateNumber).Append(":").Append(configs);
		if (IsAcceptState)
		{
			stringBuilder.Append("=>");
			if (predicates != null)
			{
				stringBuilder.Append(Arrays.ToString(predicates));
			}
			else
			{
				stringBuilder.Append(Prediction);
			}
		}
		return stringBuilder.ToString();
	}
}
