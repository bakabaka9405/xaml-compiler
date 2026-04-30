using System;
using System.Globalization;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

[Serializable]
public class FailedPredicateException : RecognitionException
{
	private const long serialVersionUID = 5379330841495778709L;

	private readonly int ruleIndex;

	private readonly int predicateIndex;

	private readonly string predicate;

	public virtual int RuleIndex => ruleIndex;

	public virtual int PredIndex => predicateIndex;

	[Nullable]
	public virtual string Predicate => predicate;

	public FailedPredicateException(Parser recognizer)
		: this(recognizer, null)
	{
	}

	public FailedPredicateException(Parser recognizer, string predicate)
		: this(recognizer, predicate, null)
	{
	}

	public FailedPredicateException(Parser recognizer, string predicate, string message)
		: base(FormatMessage(predicate, message), recognizer, (ITokenStream)recognizer.InputStream, recognizer.RuleContext)
	{
		ATNState aTNState = recognizer.Interpreter.atn.states[recognizer.State];
		AbstractPredicateTransition abstractPredicateTransition = (AbstractPredicateTransition)aTNState.Transition(0);
		if (abstractPredicateTransition is PredicateTransition)
		{
			ruleIndex = ((PredicateTransition)abstractPredicateTransition).ruleIndex;
			predicateIndex = ((PredicateTransition)abstractPredicateTransition).predIndex;
		}
		else
		{
			ruleIndex = 0;
			predicateIndex = 0;
		}
		this.predicate = predicate;
		base.OffendingToken = recognizer.CurrentToken;
	}

	[return: NotNull]
	private static string FormatMessage(string predicate, string message)
	{
		if (message != null)
		{
			return message;
		}
		return string.Format(CultureInfo.CurrentCulture, "failed predicate: {{{0}}}?", predicate);
	}
}
