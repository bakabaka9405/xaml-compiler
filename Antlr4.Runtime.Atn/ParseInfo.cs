using System.Collections.Generic;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public class ParseInfo
{
	protected internal readonly ProfilingATNSimulator atnSimulator;

	[NotNull]
	public virtual DecisionInfo[] DecisionInfo => atnSimulator.DecisionInfo;

	public ParseInfo(ProfilingATNSimulator atnSimulator)
	{
		this.atnSimulator = atnSimulator;
	}

	[return: NotNull]
	public virtual IList<int> GetLLDecisions()
	{
		DecisionInfo[] decisionInfo = atnSimulator.DecisionInfo;
		IList<int> list = new List<int>();
		for (int i = 0; i < decisionInfo.Length; i++)
		{
			long lL_Fallback = decisionInfo[i].LL_Fallback;
			if (lL_Fallback > 0)
			{
				list.Add(i);
			}
		}
		return list;
	}

	public virtual long GetTotalSLLLookaheadOps()
	{
		DecisionInfo[] decisionInfo = atnSimulator.DecisionInfo;
		long num = 0L;
		for (int i = 0; i < decisionInfo.Length; i++)
		{
			num += decisionInfo[i].SLL_TotalLook;
		}
		return num;
	}

	public virtual long GetTotalLLLookaheadOps()
	{
		DecisionInfo[] decisionInfo = atnSimulator.DecisionInfo;
		long num = 0L;
		for (int i = 0; i < decisionInfo.Length; i++)
		{
			num += decisionInfo[i].LL_TotalLook;
		}
		return num;
	}

	public virtual long GetTotalSLLATNLookaheadOps()
	{
		DecisionInfo[] decisionInfo = atnSimulator.DecisionInfo;
		long num = 0L;
		for (int i = 0; i < decisionInfo.Length; i++)
		{
			num += decisionInfo[i].SLL_ATNTransitions;
		}
		return num;
	}

	public virtual long GetTotalLLATNLookaheadOps()
	{
		DecisionInfo[] decisionInfo = atnSimulator.DecisionInfo;
		long num = 0L;
		for (int i = 0; i < decisionInfo.Length; i++)
		{
			num += decisionInfo[i].LL_ATNTransitions;
		}
		return num;
	}

	public virtual long GetTotalATNLookaheadOps()
	{
		DecisionInfo[] decisionInfo = atnSimulator.DecisionInfo;
		long num = 0L;
		for (int i = 0; i < decisionInfo.Length; i++)
		{
			num += decisionInfo[i].SLL_ATNTransitions;
			num += decisionInfo[i].LL_ATNTransitions;
		}
		return num;
	}

	public virtual int GetDFASize()
	{
		int num = 0;
		DFA[] decisionToDFA = atnSimulator.atn.decisionToDFA;
		for (int i = 0; i < decisionToDFA.Length; i++)
		{
			num += GetDFASize(i);
		}
		return num;
	}

	public virtual int GetDFASize(int decision)
	{
		DFA dFA = atnSimulator.atn.decisionToDFA[decision];
		return dFA.states.Count;
	}
}
