using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public abstract class Recognizer<Symbol, ATNInterpreter> : IRecognizer where ATNInterpreter : ATNSimulator
{
	public const int Eof = -1;

	private static readonly ConditionalWeakTable<IVocabulary, IDictionary<string, int>> tokenTypeMapCache = new ConditionalWeakTable<IVocabulary, IDictionary<string, int>>();

	private static readonly ConditionalWeakTable<string[], IDictionary<string, int>> ruleIndexMapCache = new ConditionalWeakTable<string[], IDictionary<string, int>>();

	[NotNull]
	private IAntlrErrorListener<Symbol>[] _listeners = new IAntlrErrorListener<Symbol>[1] { ConsoleErrorListener<Symbol>.Instance };

	private ATNInterpreter _interp;

	private int _stateNumber = -1;

	public abstract string[] RuleNames { get; }

	public abstract IVocabulary Vocabulary { get; }

	[NotNull]
	public virtual IDictionary<string, int> TokenTypeMap => tokenTypeMapCache.GetValue(Vocabulary, CreateTokenTypeMap);

	[NotNull]
	public virtual IDictionary<string, int> RuleIndexMap
	{
		get
		{
			string[] ruleNames = RuleNames;
			if (ruleNames == null)
			{
				throw new NotSupportedException("The current recognizer does not provide a list of rule names.");
			}
			return ruleIndexMapCache.GetValue(ruleNames, Utils.ToMap);
		}
	}

	public virtual string SerializedAtn
	{
		[return: NotNull]
		get
		{
			throw new NotSupportedException("there is no serialized ATN");
		}
	}

	public abstract string GrammarFileName { get; }

	public virtual ATN Atn => _interp.atn;

	public virtual ATNInterpreter Interpreter
	{
		get
		{
			return _interp;
		}
		protected set
		{
			_interp = value;
		}
	}

	public virtual ParseInfo ParseInfo => null;

	[NotNull]
	public virtual IList<IAntlrErrorListener<Symbol>> ErrorListeners => new List<IAntlrErrorListener<Symbol>>(_listeners);

	public virtual IAntlrErrorListener<Symbol> ErrorListenerDispatch => new ProxyErrorListener<Symbol>(ErrorListeners);

	public int State
	{
		get
		{
			return _stateNumber;
		}
		set
		{
			_stateNumber = value;
		}
	}

	public abstract IIntStream InputStream { get; }

	protected virtual IDictionary<string, int> CreateTokenTypeMap(IVocabulary vocabulary)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		for (int i = 0; i < Atn.maxTokenType; i++)
		{
			string literalName = vocabulary.GetLiteralName(i);
			if (literalName != null)
			{
				dictionary[literalName] = i;
			}
			string symbolicName = vocabulary.GetSymbolicName(i);
			if (symbolicName != null)
			{
				dictionary[symbolicName] = i;
			}
		}
		dictionary["EOF"] = -1;
		return dictionary;
	}

	public virtual int GetTokenType(string tokenName)
	{
		if (TokenTypeMap.TryGetValue(tokenName, out var value))
		{
			return value;
		}
		return 0;
	}

	[return: NotNull]
	public virtual string GetErrorHeader(RecognitionException e)
	{
		int line = e.OffendingToken.Line;
		int column = e.OffendingToken.Column;
		return "line " + line + ":" + column;
	}

	[Obsolete("This method is not called by the ANTLR 4 Runtime. Specific implementations of IAntlrErrorStrategy may provide a similar feature when necessary. For example, see DefaultErrorStrategy.GetTokenErrorDisplay(IToken).")]
	public virtual string GetTokenErrorDisplay(IToken t)
	{
		if (t == null)
		{
			return "<no token>";
		}
		string text = t.Text;
		if (text == null)
		{
			text = ((t.Type != -1) ? ("<" + t.Type + ">") : "<EOF>");
		}
		text = text.Replace("\n", "\\n");
		text = text.Replace("\r", "\\r");
		text = text.Replace("\t", "\\t");
		return "'" + text + "'";
	}

	public virtual void AddErrorListener(IAntlrErrorListener<Symbol> listener)
	{
		Args.NotNull("listener", listener);
		IAntlrErrorListener<Symbol>[] array = _listeners;
		Array.Resize(ref array, array.Length + 1);
		array[array.Length - 1] = listener;
		_listeners = array;
	}

	public virtual void RemoveErrorListener(IAntlrErrorListener<Symbol> listener)
	{
		IAntlrErrorListener<Symbol>[] array = _listeners;
		int num = Array.IndexOf(array, listener);
		if (num >= 0)
		{
			Array.Copy(array, num + 1, array, num, array.Length - num - 1);
			Array.Resize(ref array, array.Length - 1);
			_listeners = array;
		}
	}

	public virtual void RemoveErrorListeners()
	{
		_listeners = new IAntlrErrorListener<Symbol>[0];
	}

	public virtual bool Sempred(RuleContext _localctx, int ruleIndex, int actionIndex)
	{
		return true;
	}

	public virtual bool Precpred(RuleContext localctx, int precedence)
	{
		return true;
	}

	public virtual void Action(RuleContext _localctx, int ruleIndex, int actionIndex)
	{
	}
}
