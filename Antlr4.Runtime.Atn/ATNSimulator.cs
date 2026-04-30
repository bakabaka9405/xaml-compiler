using System;
using System.Collections.Generic;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public abstract class ATNSimulator
{
	[Obsolete("Use ATNDeserializer.SerializedVersion instead.")]
	public static readonly int SerializedVersion = ATNDeserializer.SerializedVersion;

	[Obsolete("Use ATNDeserializer.CheckCondition(bool) instead.")]
	public static readonly Guid SerializedUuid = ATNDeserializer.SerializedUuid;

	public const char RuleVariantDelimiter = '$';

	public const string RuleLfVariantMarker = "$lf$";

	public const string RuleNolfVariantMarker = "$nolf$";

	[NotNull]
	public static readonly DFAState Error = new DFAState(new EmptyEdgeMap<DFAState>(0, -1), new EmptyEdgeMap<DFAState>(0, -1), new ATNConfigSet())
	{
		stateNumber = int.MaxValue
	};

	[NotNull]
	public readonly ATN atn;

	public ATNSimulator(ATN atn)
	{
		this.atn = atn;
	}

	public abstract void Reset();

	public virtual void ClearDFA()
	{
		atn.ClearDFA();
	}

	[Obsolete("Use ATNDeserializer.Deserialize(char[]) instead.")]
	public static ATN Deserialize(char[] data)
	{
		return new ATNDeserializer().Deserialize(data);
	}

	[Obsolete("Use ATNDeserializer.EdgeFactory(ATN, TransitionType, int, int, int, int, int, System.Collections.Generic.IList{E}) instead.")]
	[return: NotNull]
	public static Transition EdgeFactory(ATN atn, TransitionType type, int src, int trg, int arg1, int arg2, int arg3, IList<IntervalSet> sets)
	{
		return new ATNDeserializer().EdgeFactory(atn, type, src, trg, arg1, arg2, arg3, sets);
	}

	[Obsolete("Use ATNDeserializer.StateFactory(StateType, int) instead.")]
	public static ATNState StateFactory(StateType type, int ruleIndex)
	{
		return new ATNDeserializer().StateFactory(type, ruleIndex);
	}
}
