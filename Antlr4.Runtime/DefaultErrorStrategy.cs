using System;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public class DefaultErrorStrategy : IAntlrErrorStrategy
{
	protected internal bool errorRecoveryMode;

	protected internal int lastErrorIndex = -1;

	protected internal IntervalSet lastErrorStates;

	public virtual void Reset(Parser recognizer)
	{
		EndErrorCondition(recognizer);
	}

	protected internal virtual void BeginErrorCondition(Parser recognizer)
	{
		errorRecoveryMode = true;
	}

	public virtual bool InErrorRecoveryMode(Parser recognizer)
	{
		return errorRecoveryMode;
	}

	protected internal virtual void EndErrorCondition(Parser recognizer)
	{
		errorRecoveryMode = false;
		lastErrorStates = null;
		lastErrorIndex = -1;
	}

	public virtual void ReportMatch(Parser recognizer)
	{
		EndErrorCondition(recognizer);
	}

	public virtual void ReportError(Parser recognizer, RecognitionException e)
	{
		if (!InErrorRecoveryMode(recognizer))
		{
			BeginErrorCondition(recognizer);
			if (e is NoViableAltException)
			{
				ReportNoViableAlternative(recognizer, (NoViableAltException)e);
				return;
			}
			if (e is InputMismatchException)
			{
				ReportInputMismatch(recognizer, (InputMismatchException)e);
				return;
			}
			if (e is FailedPredicateException)
			{
				ReportFailedPredicate(recognizer, (FailedPredicateException)e);
				return;
			}
			Console.Error.WriteLine("unknown recognition error type: " + e.GetType().FullName);
			NotifyErrorListeners(recognizer, e.Message, e);
		}
	}

	protected internal virtual void NotifyErrorListeners(Parser recognizer, string message, RecognitionException e)
	{
		recognizer.NotifyErrorListeners(e.OffendingToken, message, e);
	}

	public virtual void Recover(Parser recognizer, RecognitionException e)
	{
		if (lastErrorIndex == ((ITokenStream)recognizer.InputStream).Index && lastErrorStates != null && lastErrorStates.Contains(recognizer.State))
		{
			recognizer.Consume();
		}
		lastErrorIndex = ((ITokenStream)recognizer.InputStream).Index;
		if (lastErrorStates == null)
		{
			lastErrorStates = new IntervalSet();
		}
		lastErrorStates.Add(recognizer.State);
		IntervalSet errorRecoverySet = GetErrorRecoverySet(recognizer);
		ConsumeUntil(recognizer, errorRecoverySet);
	}

	public virtual void Sync(Parser recognizer)
	{
		ATNState aTNState = recognizer.Interpreter.atn.states[recognizer.State];
		if (InErrorRecoveryMode(recognizer))
		{
			return;
		}
		ITokenStream tokenStream = (ITokenStream)recognizer.InputStream;
		int num = tokenStream.La(1);
		if (recognizer.Atn.NextTokens(aTNState).Contains(num) || num == -1 || recognizer.IsExpectedToken(num))
		{
			return;
		}
		switch (aTNState.StateType)
		{
		case StateType.BlockStart:
		case StateType.PlusBlockStart:
		case StateType.StarBlockStart:
		case StateType.StarLoopEntry:
			if (SingleTokenDeletion(recognizer) != null)
			{
				break;
			}
			throw new InputMismatchException(recognizer);
		case StateType.StarLoopBack:
		case StateType.PlusLoopBack:
		{
			ReportUnwantedToken(recognizer);
			IntervalSet expectedTokens = recognizer.GetExpectedTokens();
			IntervalSet set = expectedTokens.Or(GetErrorRecoverySet(recognizer));
			ConsumeUntil(recognizer, set);
			break;
		}
		case StateType.TokenStart:
		case StateType.RuleStop:
		case StateType.BlockEnd:
			break;
		}
	}

	protected internal virtual void ReportNoViableAlternative(Parser recognizer, NoViableAltException e)
	{
		ITokenStream tokenStream = (ITokenStream)recognizer.InputStream;
		string s = ((tokenStream == null) ? "<unknown input>" : ((e.StartToken.Type != -1) ? tokenStream.GetText(e.StartToken, e.OffendingToken) : "<EOF>"));
		string message = "no viable alternative at input " + EscapeWSAndQuote(s);
		NotifyErrorListeners(recognizer, message, e);
	}

	protected internal virtual void ReportInputMismatch(Parser recognizer, InputMismatchException e)
	{
		string message = "mismatched input " + GetTokenErrorDisplay(e.OffendingToken) + " expecting " + e.GetExpectedTokens().ToString(recognizer.Vocabulary);
		NotifyErrorListeners(recognizer, message, e);
	}

	protected internal virtual void ReportFailedPredicate(Parser recognizer, FailedPredicateException e)
	{
		string text = recognizer.RuleNames[recognizer.RuleContext.RuleIndex];
		string message = "rule " + text + " " + e.Message;
		NotifyErrorListeners(recognizer, message, e);
	}

	protected internal virtual void ReportUnwantedToken(Parser recognizer)
	{
		if (!InErrorRecoveryMode(recognizer))
		{
			BeginErrorCondition(recognizer);
			IToken currentToken = recognizer.CurrentToken;
			string tokenErrorDisplay = GetTokenErrorDisplay(currentToken);
			IntervalSet expectedTokens = GetExpectedTokens(recognizer);
			string msg = "extraneous input " + tokenErrorDisplay + " expecting " + expectedTokens.ToString(recognizer.Vocabulary);
			recognizer.NotifyErrorListeners(currentToken, msg, null);
		}
	}

	protected internal virtual void ReportMissingToken(Parser recognizer)
	{
		if (!InErrorRecoveryMode(recognizer))
		{
			BeginErrorCondition(recognizer);
			IToken currentToken = recognizer.CurrentToken;
			IntervalSet expectedTokens = GetExpectedTokens(recognizer);
			string msg = "missing " + expectedTokens.ToString(recognizer.Vocabulary) + " at " + GetTokenErrorDisplay(currentToken);
			recognizer.NotifyErrorListeners(currentToken, msg, null);
		}
	}

	public virtual IToken RecoverInline(Parser recognizer)
	{
		IToken token = SingleTokenDeletion(recognizer);
		if (token != null)
		{
			recognizer.Consume();
			return token;
		}
		if (SingleTokenInsertion(recognizer))
		{
			return GetMissingSymbol(recognizer);
		}
		throw new InputMismatchException(recognizer);
	}

	protected internal virtual bool SingleTokenInsertion(Parser recognizer)
	{
		int el = ((ITokenStream)recognizer.InputStream).La(1);
		ATNState aTNState = recognizer.Interpreter.atn.states[recognizer.State];
		ATNState target = aTNState.Transition(0).target;
		ATN atn = recognizer.Interpreter.atn;
		IntervalSet intervalSet = atn.NextTokens(target, PredictionContext.FromRuleContext(atn, recognizer.RuleContext));
		if (intervalSet.Contains(el))
		{
			ReportMissingToken(recognizer);
			return true;
		}
		return false;
	}

	[return: Nullable]
	protected internal virtual IToken SingleTokenDeletion(Parser recognizer)
	{
		int el = ((ITokenStream)recognizer.InputStream).La(2);
		IntervalSet expectedTokens = GetExpectedTokens(recognizer);
		if (expectedTokens.Contains(el))
		{
			ReportUnwantedToken(recognizer);
			recognizer.Consume();
			IToken currentToken = recognizer.CurrentToken;
			ReportMatch(recognizer);
			return currentToken;
		}
		return null;
	}

	[return: NotNull]
	protected internal virtual IToken GetMissingSymbol(Parser recognizer)
	{
		IToken currentToken = recognizer.CurrentToken;
		IntervalSet expectedTokens = GetExpectedTokens(recognizer);
		int minElement = expectedTokens.MinElement;
		string tokenText = ((minElement != -1) ? ("<missing " + recognizer.Vocabulary.GetDisplayName(minElement) + ">") : "<missing EOF>");
		IToken token = currentToken;
		IToken token2 = ((ITokenStream)recognizer.InputStream).Lt(-1);
		if (token.Type == -1 && token2 != null)
		{
			token = token2;
		}
		return ConstructToken(((ITokenStream)recognizer.InputStream).TokenSource, minElement, tokenText, token);
	}

	protected internal virtual IToken ConstructToken(ITokenSource tokenSource, int expectedTokenType, string tokenText, IToken current)
	{
		ITokenFactory tokenFactory = tokenSource.TokenFactory;
		return tokenFactory.Create(Tuple.Create(tokenSource, current.TokenSource.InputStream), expectedTokenType, tokenText, 0, -1, -1, current.Line, current.Column);
	}

	[return: NotNull]
	protected internal virtual IntervalSet GetExpectedTokens(Parser recognizer)
	{
		return recognizer.GetExpectedTokens();
	}

	protected internal virtual string GetTokenErrorDisplay(IToken t)
	{
		if (t == null)
		{
			return "<no token>";
		}
		string text = GetSymbolText(t);
		if (text == null)
		{
			text = ((GetSymbolType(t) != -1) ? ("<" + GetSymbolType(t) + ">") : "<EOF>");
		}
		return EscapeWSAndQuote(text);
	}

	protected internal virtual string GetSymbolText(IToken symbol)
	{
		return symbol.Text;
	}

	protected internal virtual int GetSymbolType(IToken symbol)
	{
		return symbol.Type;
	}

	[return: NotNull]
	protected internal virtual string EscapeWSAndQuote(string s)
	{
		s = s.Replace("\n", "\\n");
		s = s.Replace("\r", "\\r");
		s = s.Replace("\t", "\\t");
		return "'" + s + "'";
	}

	[return: NotNull]
	protected internal virtual IntervalSet GetErrorRecoverySet(Parser recognizer)
	{
		ATN atn = recognizer.Interpreter.atn;
		RuleContext ruleContext = recognizer.RuleContext;
		IntervalSet intervalSet = new IntervalSet();
		while (ruleContext != null && ruleContext.invokingState >= 0)
		{
			ATNState aTNState = atn.states[ruleContext.invokingState];
			RuleTransition ruleTransition = (RuleTransition)aTNState.Transition(0);
			IntervalSet set = atn.NextTokens(ruleTransition.followState);
			intervalSet.AddAll(set);
			ruleContext = ruleContext.Parent;
		}
		intervalSet.Remove(-2);
		return intervalSet;
	}

	protected internal virtual void ConsumeUntil(Parser recognizer, IntervalSet set)
	{
		int num = ((ITokenStream)recognizer.InputStream).La(1);
		while (num != -1 && !set.Contains(num))
		{
			recognizer.Consume();
			num = ((ITokenStream)recognizer.InputStream).La(1);
		}
	}
}
