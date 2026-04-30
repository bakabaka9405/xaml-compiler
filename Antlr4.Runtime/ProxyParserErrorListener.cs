using System.Collections.Generic;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime;

public class ProxyParserErrorListener : ProxyErrorListener<IToken>, IParserErrorListener, IAntlrErrorListener<IToken>
{
	public ProxyParserErrorListener(ICollection<IAntlrErrorListener<IToken>> delegates)
		: base((IEnumerable<IAntlrErrorListener<IToken>>)delegates)
	{
	}

	public virtual void ReportAmbiguity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, bool exact, BitSet ambigAlts, ATNConfigSet configs)
	{
		foreach (IAntlrErrorListener<IToken> @delegate in Delegates)
		{
			if (@delegate is IParserErrorListener)
			{
				IParserErrorListener parserErrorListener = (IParserErrorListener)@delegate;
				parserErrorListener.ReportAmbiguity(recognizer, dfa, startIndex, stopIndex, exact, ambigAlts, configs);
			}
		}
	}

	public virtual void ReportAttemptingFullContext(Parser recognizer, DFA dfa, int startIndex, int stopIndex, BitSet conflictingAlts, SimulatorState conflictState)
	{
		foreach (IAntlrErrorListener<IToken> @delegate in Delegates)
		{
			if (@delegate is IParserErrorListener)
			{
				IParserErrorListener parserErrorListener = (IParserErrorListener)@delegate;
				parserErrorListener.ReportAttemptingFullContext(recognizer, dfa, startIndex, stopIndex, conflictingAlts, conflictState);
			}
		}
	}

	public virtual void ReportContextSensitivity(Parser recognizer, DFA dfa, int startIndex, int stopIndex, int prediction, SimulatorState acceptState)
	{
		foreach (IAntlrErrorListener<IToken> @delegate in Delegates)
		{
			if (@delegate is IParserErrorListener)
			{
				IParserErrorListener parserErrorListener = (IParserErrorListener)@delegate;
				parserErrorListener.ReportContextSensitivity(recognizer, dfa, startIndex, stopIndex, prediction, acceptState);
			}
		}
	}
}
