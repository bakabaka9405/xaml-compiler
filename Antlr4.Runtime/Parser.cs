using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Dfa;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree;
using Antlr4.Runtime.Tree.Pattern;

namespace Antlr4.Runtime;

public abstract class Parser : Recognizer<IToken, ParserATNSimulator>
{
	public class TraceListener : IParseTreeListener
	{
		private readonly Parser _enclosing;

		public virtual void EnterEveryRule(ParserRuleContext ctx)
		{
			Console.Out.WriteLine("enter   " + _enclosing.RuleNames[ctx.RuleIndex] + ", LT(1)=" + _enclosing._input.Lt(1).Text);
		}

		public virtual void ExitEveryRule(ParserRuleContext ctx)
		{
			Console.Out.WriteLine("exit    " + _enclosing.RuleNames[ctx.RuleIndex] + ", LT(1)=" + _enclosing._input.Lt(1).Text);
		}

		public virtual void VisitErrorNode(IErrorNode node)
		{
		}

		public virtual void VisitTerminal(ITerminalNode node)
		{
			ParserRuleContext parserRuleContext = (ParserRuleContext)node.Parent.RuleContext;
			IToken symbol = node.Symbol;
			Console.Out.WriteLine("consume " + symbol?.ToString() + " rule " + _enclosing.RuleNames[parserRuleContext.RuleIndex]);
		}

		internal TraceListener(Parser _enclosing)
		{
			this._enclosing = _enclosing;
		}
	}

	public class TrimToSizeListener : IParseTreeListener
	{
		public static readonly TrimToSizeListener Instance = new TrimToSizeListener();

		public virtual void VisitTerminal(ITerminalNode node)
		{
		}

		public virtual void VisitErrorNode(IErrorNode node)
		{
		}

		public virtual void EnterEveryRule(ParserRuleContext ctx)
		{
		}

		public virtual void ExitEveryRule(ParserRuleContext ctx)
		{
			if (ctx.children is List<IParseTree>)
			{
				((List<IParseTree>)ctx.children).TrimExcess();
			}
		}
	}

	private static readonly IDictionary<string, ATN> bypassAltsAtnCache = new Dictionary<string, ATN>();

	[NotNull]
	private IAntlrErrorStrategy _errHandler = new DefaultErrorStrategy();

	private ITokenStream _input;

	private readonly List<int> _precedenceStack = new List<int> { 0 };

	private ParserRuleContext _ctx;

	private bool _buildParseTrees = true;

	private TraceListener _tracer;

	[Nullable]
	private IList<IParseTreeListener> _parseListeners;

	private int _syntaxErrors;

	public virtual bool BuildParseTree
	{
		get
		{
			return _buildParseTrees;
		}
		set
		{
			_buildParseTrees = value;
		}
	}

	public virtual bool TrimParseTree
	{
		get
		{
			return ParseListeners.Contains(TrimToSizeListener.Instance);
		}
		set
		{
			if (value)
			{
				if (!TrimParseTree)
				{
					AddParseListener(TrimToSizeListener.Instance);
				}
			}
			else
			{
				RemoveParseListener(TrimToSizeListener.Instance);
			}
		}
	}

	public virtual IList<IParseTreeListener> ParseListeners
	{
		get
		{
			IList<IParseTreeListener> parseListeners = _parseListeners;
			if (parseListeners == null)
			{
				return Collections.EmptyList<IParseTreeListener>();
			}
			return parseListeners;
		}
	}

	public virtual int NumberOfSyntaxErrors => _syntaxErrors;

	public virtual ITokenFactory TokenFactory => _input.TokenSource.TokenFactory;

	public virtual IAntlrErrorStrategy ErrorHandler
	{
		get
		{
			return _errHandler;
		}
		set
		{
			_errHandler = value;
		}
	}

	public override IIntStream InputStream => _input;

	public ITokenStream TokenStream
	{
		get
		{
			return _input;
		}
		set
		{
			_input = null;
			Reset();
			_input = value;
		}
	}

	public virtual IToken CurrentToken => _input.Lt(1);

	public int Precedence
	{
		get
		{
			if (_precedenceStack.Count == 0)
			{
				return -1;
			}
			return _precedenceStack[_precedenceStack.Count - 1];
		}
	}

	public virtual ParserRuleContext Context
	{
		get
		{
			return _ctx;
		}
		set
		{
			_ctx = value;
		}
	}

	public override IAntlrErrorListener<IToken> ErrorListenerDispatch => new ProxyParserErrorListener(ErrorListeners);

	public virtual ParserRuleContext RuleContext => _ctx;

	public virtual string SourceName => _input.SourceName;

	public override ParseInfo ParseInfo
	{
		get
		{
			ParserATNSimulator interpreter = Interpreter;
			if (interpreter is ProfilingATNSimulator)
			{
				return new ParseInfo((ProfilingATNSimulator)interpreter);
			}
			return null;
		}
	}

	public virtual bool Profile
	{
		set
		{
			bool flag = value;
			ParserATNSimulator interpreter = Interpreter;
			if (flag)
			{
				if (!(interpreter is ProfilingATNSimulator))
				{
					Interpreter = new ProfilingATNSimulator(this);
				}
			}
			else if (interpreter is ProfilingATNSimulator)
			{
				Interpreter = new ParserATNSimulator(this, Atn);
			}
		}
	}

	public virtual bool Trace
	{
		get
		{
			foreach (IParseTreeListener parseListener in ParseListeners)
			{
				if (parseListener is TraceListener)
				{
					return true;
				}
			}
			return false;
		}
		set
		{
			if (!value)
			{
				RemoveParseListener(_tracer);
				_tracer = null;
				return;
			}
			if (_tracer != null)
			{
				RemoveParseListener(_tracer);
			}
			else
			{
				_tracer = new TraceListener(this);
			}
			AddParseListener(_tracer);
		}
	}

	public Parser(ITokenStream input)
	{
		TokenStream = input;
	}

	public virtual void Reset()
	{
		if ((ITokenStream)InputStream != null)
		{
			((ITokenStream)InputStream).Seek(0);
		}
		_errHandler.Reset(this);
		_ctx = null;
		_syntaxErrors = 0;
		Trace = false;
		_precedenceStack.Clear();
		_precedenceStack.Add(0);
		Interpreter?.Reset();
	}

	[return: NotNull]
	public virtual IToken Match(int ttype)
	{
		IToken token = CurrentToken;
		if (token.Type == ttype)
		{
			_errHandler.ReportMatch(this);
			Consume();
		}
		else
		{
			token = _errHandler.RecoverInline(this);
			if (_buildParseTrees && token.TokenIndex == -1)
			{
				_ctx.AddErrorNode(token);
			}
		}
		return token;
	}

	[return: NotNull]
	public virtual IToken MatchWildcard()
	{
		IToken token = CurrentToken;
		if (token.Type > 0)
		{
			_errHandler.ReportMatch(this);
			Consume();
		}
		else
		{
			token = _errHandler.RecoverInline(this);
			if (_buildParseTrees && token.TokenIndex == -1)
			{
				_ctx.AddErrorNode(token);
			}
		}
		return token;
	}

	public virtual void AddParseListener(IParseTreeListener listener)
	{
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		if (_parseListeners == null)
		{
			_parseListeners = new List<IParseTreeListener>();
		}
		_parseListeners.Add(listener);
	}

	public virtual void RemoveParseListener(IParseTreeListener listener)
	{
		if (_parseListeners != null && _parseListeners.Remove(listener) && _parseListeners.Count == 0)
		{
			_parseListeners = null;
		}
	}

	public virtual void RemoveParseListeners()
	{
		_parseListeners = null;
	}

	protected internal virtual void TriggerEnterRuleEvent()
	{
		foreach (IParseTreeListener parseListener in _parseListeners)
		{
			parseListener.EnterEveryRule(_ctx);
			_ctx.EnterRule(parseListener);
		}
	}

	protected internal virtual void TriggerExitRuleEvent()
	{
		if (_parseListeners != null)
		{
			for (int num = _parseListeners.Count - 1; num >= 0; num--)
			{
				IParseTreeListener parseTreeListener = _parseListeners[num];
				_ctx.ExitRule(parseTreeListener);
				parseTreeListener.ExitEveryRule(_ctx);
			}
		}
	}

	[return: NotNull]
	public virtual ATN GetATNWithBypassAlts()
	{
		string serializedAtn = SerializedAtn;
		if (serializedAtn == null)
		{
			throw new NotSupportedException("The current parser does not support an ATN with bypass alternatives.");
		}
		lock (bypassAltsAtnCache)
		{
			ATN aTN = bypassAltsAtnCache.Get(serializedAtn);
			if (aTN == null)
			{
				ATNDeserializationOptions aTNDeserializationOptions = new ATNDeserializationOptions();
				aTNDeserializationOptions.GenerateRuleBypassTransitions = true;
				aTN = new ATNDeserializer(aTNDeserializationOptions).Deserialize(serializedAtn.ToCharArray());
				bypassAltsAtnCache.Put(serializedAtn, aTN);
			}
			return aTN;
		}
	}

	public virtual ParseTreePattern CompileParseTreePattern(string pattern, int patternRuleIndex)
	{
		if ((ITokenStream)InputStream != null)
		{
			ITokenSource tokenSource = ((ITokenStream)InputStream).TokenSource;
			if (tokenSource is Lexer)
			{
				Lexer lexer = (Lexer)tokenSource;
				return CompileParseTreePattern(pattern, patternRuleIndex, lexer);
			}
		}
		throw new NotSupportedException("Parser can't discover a lexer to use");
	}

	public virtual ParseTreePattern CompileParseTreePattern(string pattern, int patternRuleIndex, Lexer lexer)
	{
		ParseTreePatternMatcher parseTreePatternMatcher = new ParseTreePatternMatcher(lexer, this);
		return parseTreePatternMatcher.Compile(pattern, patternRuleIndex);
	}

	public void NotifyErrorListeners(string msg)
	{
		NotifyErrorListeners(CurrentToken, msg, null);
	}

	public virtual void NotifyErrorListeners(IToken offendingToken, string msg, RecognitionException e)
	{
		_syntaxErrors++;
		int line = -1;
		int charPositionInLine = -1;
		if (offendingToken != null)
		{
			line = offendingToken.Line;
			charPositionInLine = offendingToken.Column;
		}
		IAntlrErrorListener<IToken> antlrErrorListener = (IParserErrorListener)ErrorListenerDispatch;
		antlrErrorListener.SyntaxError(this, offendingToken, line, charPositionInLine, msg, e);
	}

	public virtual IToken Consume()
	{
		IToken currentToken = CurrentToken;
		if (currentToken.Type != -1)
		{
			((ITokenStream)InputStream).Consume();
		}
		bool flag = _parseListeners != null && _parseListeners.Count != 0;
		if (_buildParseTrees || flag)
		{
			if (_errHandler.InErrorRecoveryMode(this))
			{
				IErrorNode node = _ctx.AddErrorNode(currentToken);
				if (_parseListeners != null)
				{
					foreach (IParseTreeListener parseListener in _parseListeners)
					{
						parseListener.VisitErrorNode(node);
					}
				}
			}
			else
			{
				ITerminalNode node2 = _ctx.AddChild(currentToken);
				if (_parseListeners != null)
				{
					foreach (IParseTreeListener parseListener2 in _parseListeners)
					{
						parseListener2.VisitTerminal(node2);
					}
				}
			}
		}
		return currentToken;
	}

	protected internal virtual void AddContextToParseTree()
	{
		((ParserRuleContext)_ctx.Parent)?.AddChild(_ctx);
	}

	public virtual void EnterRule(ParserRuleContext localctx, int state, int ruleIndex)
	{
		base.State = state;
		_ctx = localctx;
		_ctx.Start = _input.Lt(1);
		if (_buildParseTrees)
		{
			AddContextToParseTree();
		}
		if (_parseListeners != null)
		{
			TriggerEnterRuleEvent();
		}
	}

	public virtual void EnterLeftFactoredRule(ParserRuleContext localctx, int state, int ruleIndex)
	{
		base.State = state;
		if (_buildParseTrees)
		{
			ParserRuleContext parserRuleContext = (ParserRuleContext)_ctx.GetChild(_ctx.ChildCount - 1);
			_ctx.RemoveLastChild();
			parserRuleContext.Parent = localctx;
			localctx.AddChild(parserRuleContext);
		}
		_ctx = localctx;
		_ctx.Start = _input.Lt(1);
		if (_buildParseTrees)
		{
			AddContextToParseTree();
		}
		if (_parseListeners != null)
		{
			TriggerEnterRuleEvent();
		}
	}

	public virtual void ExitRule()
	{
		_ctx.Stop = _input.Lt(-1);
		if (_parseListeners != null)
		{
			TriggerExitRuleEvent();
		}
		base.State = _ctx.invokingState;
		_ctx = (ParserRuleContext)_ctx.Parent;
	}

	public virtual void EnterOuterAlt(ParserRuleContext localctx, int altNum)
	{
		if (_buildParseTrees && _ctx != localctx)
		{
			ParserRuleContext parserRuleContext = (ParserRuleContext)_ctx.Parent;
			if (parserRuleContext != null)
			{
				parserRuleContext.RemoveLastChild();
				parserRuleContext.AddChild(localctx);
			}
		}
		_ctx = localctx;
	}

	[Obsolete("UseEnterRecursionRule(ParserRuleContext, int, int, int) instead.")]
	public virtual void EnterRecursionRule(ParserRuleContext localctx, int ruleIndex)
	{
		EnterRecursionRule(localctx, Atn.ruleToStartState[ruleIndex].stateNumber, ruleIndex, 0);
	}

	public virtual void EnterRecursionRule(ParserRuleContext localctx, int state, int ruleIndex, int precedence)
	{
		base.State = state;
		_precedenceStack.Add(precedence);
		_ctx = localctx;
		_ctx.Start = _input.Lt(1);
		if (_parseListeners != null)
		{
			TriggerEnterRuleEvent();
		}
	}

	public virtual void PushNewRecursionContext(ParserRuleContext localctx, int state, int ruleIndex)
	{
		ParserRuleContext ctx = _ctx;
		ctx.Parent = localctx;
		ctx.invokingState = state;
		ctx.Stop = _input.Lt(-1);
		_ctx = localctx;
		_ctx.Start = ctx.Start;
		if (_buildParseTrees)
		{
			_ctx.AddChild(ctx);
		}
		if (_parseListeners != null)
		{
			TriggerEnterRuleEvent();
		}
	}

	public virtual void UnrollRecursionContexts(ParserRuleContext _parentctx)
	{
		_precedenceStack.RemoveAt(_precedenceStack.Count - 1);
		_ctx.Stop = _input.Lt(-1);
		ParserRuleContext ctx = _ctx;
		if (_parseListeners != null)
		{
			while (_ctx != _parentctx)
			{
				TriggerExitRuleEvent();
				_ctx = (ParserRuleContext)_ctx.Parent;
			}
		}
		else
		{
			_ctx = _parentctx;
		}
		ctx.Parent = _parentctx;
		if (_buildParseTrees)
		{
			_parentctx?.AddChild(ctx);
		}
	}

	public virtual ParserRuleContext GetInvokingContext(int ruleIndex)
	{
		for (ParserRuleContext parserRuleContext = _ctx; parserRuleContext != null; parserRuleContext = (ParserRuleContext)parserRuleContext.Parent)
		{
			if (parserRuleContext.RuleIndex == ruleIndex)
			{
				return parserRuleContext;
			}
		}
		return null;
	}

	public override bool Precpred(RuleContext localctx, int precedence)
	{
		return precedence >= _precedenceStack[_precedenceStack.Count - 1];
	}

	public virtual bool InContext(string context)
	{
		return false;
	}

	public virtual bool IsExpectedToken(int symbol)
	{
		ATN atn = Interpreter.atn;
		ParserRuleContext parserRuleContext = _ctx;
		ATNState s = atn.states[base.State];
		IntervalSet intervalSet = atn.NextTokens(s);
		if (intervalSet.Contains(symbol))
		{
			return true;
		}
		if (!intervalSet.Contains(-2))
		{
			return false;
		}
		while (parserRuleContext != null && parserRuleContext.invokingState >= 0 && intervalSet.Contains(-2))
		{
			ATNState aTNState = atn.states[parserRuleContext.invokingState];
			RuleTransition ruleTransition = (RuleTransition)aTNState.Transition(0);
			intervalSet = atn.NextTokens(ruleTransition.followState);
			if (intervalSet.Contains(symbol))
			{
				return true;
			}
			parserRuleContext = (ParserRuleContext)parserRuleContext.Parent;
		}
		if (intervalSet.Contains(-2) && symbol == -1)
		{
			return true;
		}
		return false;
	}

	[return: NotNull]
	public virtual IntervalSet GetExpectedTokens()
	{
		return Atn.GetExpectedTokens(base.State, Context);
	}

	[return: NotNull]
	public virtual IntervalSet GetExpectedTokensWithinCurrentRule()
	{
		ATN atn = Interpreter.atn;
		ATNState s = atn.states[base.State];
		return atn.NextTokens(s);
	}

	public virtual int GetRuleIndex(string ruleName)
	{
		if (RuleIndexMap.TryGetValue(ruleName, out var value))
		{
			return value;
		}
		return -1;
	}

	public virtual IList<string> GetRuleInvocationStack()
	{
		return GetRuleInvocationStack(_ctx);
	}

	public virtual string GetRuleInvocationStackAsString()
	{
		StringBuilder stringBuilder = new StringBuilder("[");
		foreach (string item in GetRuleInvocationStack())
		{
			stringBuilder.Append(item);
			stringBuilder.Append(", ");
		}
		stringBuilder.Length -= 2;
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	public virtual IList<string> GetRuleInvocationStack(RuleContext p)
	{
		string[] ruleNames = RuleNames;
		IList<string> list = new List<string>();
		while (p != null)
		{
			int ruleIndex = p.RuleIndex;
			if (ruleIndex < 0)
			{
				list.Add("n/a");
			}
			else
			{
				list.Add(ruleNames[ruleIndex]);
			}
			p = p.Parent;
		}
		return list;
	}

	public virtual IList<string> GetDFAStrings()
	{
		IList<string> list = new List<string>();
		for (int i = 0; i < Interpreter.atn.decisionToDFA.Length; i++)
		{
			DFA dFA = Interpreter.atn.decisionToDFA[i];
			list.Add(dFA.ToString(Vocabulary, RuleNames));
		}
		return list;
	}

	public virtual void DumpDFA()
	{
		bool flag = false;
		for (int i = 0; i < Interpreter.atn.decisionToDFA.Length; i++)
		{
			DFA dFA = Interpreter.atn.decisionToDFA[i];
			if (!dFA.IsEmpty)
			{
				if (flag)
				{
					Console.Out.WriteLine();
				}
				Console.Out.WriteLine("Decision " + dFA.decision + ":");
				Console.Out.Write(dFA.ToString(Vocabulary, RuleNames));
				flag = true;
			}
		}
	}
}
