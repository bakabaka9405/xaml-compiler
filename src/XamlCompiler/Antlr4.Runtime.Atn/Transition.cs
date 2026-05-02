using System;
using System.Collections.ObjectModel;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Atn;

public abstract class Transition
{
	public static readonly ReadOnlyCollection<string> serializationNames = new ReadOnlyCollection<string>(Arrays.AsList<string>("INVALID", "EPSILON", "RANGE", "RULE", "PREDICATE", "ATOM", "ACTION", "SET", "NOT_SET", "WILDCARD", "PRECEDENCE"));

	[NotNull]
	public ATNState target;

	public abstract TransitionType TransitionType { get; }

	public virtual bool IsEpsilon => false;

	public virtual IntervalSet Label => null;

	protected internal Transition(ATNState target)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target cannot be null.");
		}
		this.target = target;
	}

	public abstract bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol);
}
