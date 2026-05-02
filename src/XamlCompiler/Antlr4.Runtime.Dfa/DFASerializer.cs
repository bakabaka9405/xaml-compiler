using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Dfa;

public class DFASerializer
{
	private sealed class _IComparer_103 : IComparer<DFAState>
	{
		public int Compare(DFAState o1, DFAState o2)
		{
			return o1.stateNumber - o2.stateNumber;
		}
	}

	[NotNull]
	private readonly DFA dfa;

	[NotNull]
	private readonly IVocabulary vocabulary;

	[Nullable]
	internal readonly string[] ruleNames;

	[Nullable]
	internal readonly ATN atn;

	public DFASerializer(DFA dfa, IVocabulary vocabulary)
		: this(dfa, vocabulary, null, null)
	{
	}

	public DFASerializer(DFA dfa, IRecognizer parser)
		: this(dfa, (parser != null) ? parser.Vocabulary : Vocabulary.EmptyVocabulary, parser?.RuleNames, parser?.Atn)
	{
	}

	public DFASerializer(DFA dfa, IVocabulary vocabulary, string[] ruleNames, ATN atn)
	{
		this.dfa = dfa;
		this.vocabulary = vocabulary;
		this.ruleNames = ruleNames;
		this.atn = atn;
	}

	public override string ToString()
	{
		if (dfa.s0.Get() == null)
		{
			return null;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (dfa.states != null)
		{
			List<DFAState> list = new List<DFAState>(dfa.states.Values);
			list.Sort(new _IComparer_103());
			foreach (DFAState item in list)
			{
				IEnumerable<KeyValuePair<int, DFAState>> edgeMap = item.EdgeMap;
				IEnumerable<KeyValuePair<int, DFAState>> contextEdgeMap = item.ContextEdgeMap;
				foreach (KeyValuePair<int, DFAState> item2 in edgeMap)
				{
					if ((item2.Value != null && item2.Value != ATNSimulator.Error) || item.IsContextSymbol(item2.Key))
					{
						bool flag = false;
						stringBuilder.Append(GetStateString(item)).Append("-").Append(GetEdgeLabel(item2.Key))
							.Append("->");
						if (item.IsContextSymbol(item2.Key))
						{
							stringBuilder.Append("!");
							flag = true;
						}
						DFAState value = item2.Value;
						if (value != null && value.stateNumber != int.MaxValue)
						{
							stringBuilder.Append(GetStateString(value)).Append('\n');
						}
						else if (flag)
						{
							stringBuilder.Append("ctx\n");
						}
					}
				}
				if (!item.IsContextSensitive)
				{
					continue;
				}
				foreach (KeyValuePair<int, DFAState> item3 in contextEdgeMap)
				{
					stringBuilder.Append(GetStateString(item)).Append("-").Append(GetContextLabel(item3.Key))
						.Append("->")
						.Append(GetStateString(item3.Value))
						.Append("\n");
				}
			}
		}
		string text = stringBuilder.ToString();
		if (text.Length == 0)
		{
			return null;
		}
		return text;
	}

	protected internal virtual string GetContextLabel(int i)
	{
		switch (i)
		{
		case int.MaxValue:
			return "ctx:EMPTY_FULL";
		case int.MinValue:
			return "ctx:EMPTY_LOCAL";
		default:
			if (atn != null && i > 0 && i <= atn.states.Count)
			{
				ATNState aTNState = atn.states[i];
				int ruleIndex = aTNState.ruleIndex;
				if (ruleNames != null && ruleIndex >= 0 && ruleIndex < ruleNames.Length)
				{
					return "ctx:" + i + "(" + ruleNames[ruleIndex] + ")";
				}
			}
			return "ctx:" + i;
		}
	}

	protected internal virtual string GetEdgeLabel(int i)
	{
		return vocabulary.GetDisplayName(i);
	}

	internal virtual string GetStateString(DFAState s)
	{
		if (s == ATNSimulator.Error)
		{
			return "ERROR";
		}
		int stateNumber = s.stateNumber;
		string text = (s.IsAcceptState ? ":" : "") + "s" + stateNumber + (s.IsContextSensitive ? "^" : "");
		if (s.IsAcceptState)
		{
			if (s.predicates != null)
			{
				return text + "=>" + Arrays.ToString(s.predicates);
			}
			return text + "=>" + s.Prediction;
		}
		return text;
	}
}
