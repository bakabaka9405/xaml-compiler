using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public abstract class Lexer : Recognizer<int, LexerATNSimulator>, ITokenSource
{
	public const int DefaultMode = 0;

	public const int DefaultTokenChannel = 0;

	public const int Hidden = 1;

	public const int MinCharValue = 0;

	public const int MaxCharValue = 65534;

	private ICharStream _input;

	private Tuple<ITokenSource, ICharStream> _tokenFactorySourcePair;

	private ITokenFactory _factory = CommonTokenFactory.Default;

	private IToken _token;

	private int _tokenStartCharIndex = -1;

	private int _tokenStartLine;

	private int _tokenStartColumn;

	private bool _hitEOF;

	private int _channel;

	private int _type;

	private readonly Stack<int> _modeStack = new Stack<int>();

	private int _mode;

	private string _text;

	public virtual ITokenFactory TokenFactory
	{
		get
		{
			return _factory;
		}
		set
		{
			_factory = value;
		}
	}

	public virtual string SourceName => _input.SourceName;

	public override IIntStream InputStream => _input;

	ICharStream ITokenSource.InputStream => _input;

	public virtual int Line
	{
		get
		{
			return Interpreter.Line;
		}
		set
		{
			Interpreter.Line = value;
		}
	}

	public virtual int Column
	{
		get
		{
			return Interpreter.Column;
		}
		set
		{
			Interpreter.Column = value;
		}
	}

	public virtual int CharIndex => _input.Index;

	public virtual int TokenStartCharIndex => _tokenStartCharIndex;

	public virtual int TokenStartLine => _tokenStartLine;

	public virtual int TokenStartColumn => _tokenStartColumn;

	public virtual string Text
	{
		get
		{
			if (_text != null)
			{
				return _text;
			}
			return Interpreter.GetText(_input);
		}
		set
		{
			_text = value;
		}
	}

	public virtual IToken Token
	{
		get
		{
			return _token;
		}
		set
		{
			_token = value;
		}
	}

	public virtual int Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	public virtual int Channel
	{
		get
		{
			return _channel;
		}
		set
		{
			_channel = value;
		}
	}

	public virtual Stack<int> ModeStack => _modeStack;

	public virtual int CurrentMode
	{
		get
		{
			return _mode;
		}
		set
		{
			_mode = value;
		}
	}

	public virtual bool HitEOF
	{
		get
		{
			return _hitEOF;
		}
		set
		{
			bool hitEOF = value;
			_hitEOF = hitEOF;
		}
	}

	public virtual string[] ModeNames => null;

	public Lexer(ICharStream input)
	{
		_input = input;
		_tokenFactorySourcePair = Tuple.Create((ITokenSource)this, input);
	}

	public virtual void Reset()
	{
		if (_input != null)
		{
			_input.Seek(0);
		}
		_token = null;
		_type = 0;
		_channel = 0;
		_tokenStartCharIndex = -1;
		_tokenStartColumn = -1;
		_tokenStartLine = -1;
		_text = null;
		_hitEOF = false;
		_mode = 0;
		_modeStack.Clear();
		Interpreter.Reset();
	}

	public virtual IToken NextToken()
	{
		if (_input == null)
		{
			throw new InvalidOperationException("nextToken requires a non-null input stream.");
		}
		int marker = _input.Mark();
		try
		{
			while (!_hitEOF)
			{
				_token = null;
				_channel = 0;
				_tokenStartCharIndex = _input.Index;
				_tokenStartColumn = Interpreter.Column;
				_tokenStartLine = Interpreter.Line;
				_text = null;
				while (true)
				{
					_type = 0;
					int type;
					try
					{
						type = Interpreter.Match(_input, _mode);
					}
					catch (LexerNoViableAltException e)
					{
						NotifyListeners(e);
						Recover(e);
						type = -3;
					}
					if (_input.La(1) == -1)
					{
						_hitEOF = true;
					}
					if (_type == 0)
					{
						_type = type;
					}
					if (_type == -3)
					{
						break;
					}
					if (_type != -2)
					{
						if (_token == null)
						{
							Emit();
						}
						return _token;
					}
				}
			}
			EmitEOF();
			return _token;
		}
		finally
		{
			_input.Release(marker);
		}
	}

	public virtual void Skip()
	{
		_type = -3;
	}

	public virtual void More()
	{
		_type = -2;
	}

	public virtual void Mode(int m)
	{
		_mode = m;
	}

	public virtual void PushMode(int m)
	{
		_modeStack.Push(_mode);
		Mode(m);
	}

	public virtual int PopMode()
	{
		if (_modeStack.Count == 0)
		{
			throw new InvalidOperationException();
		}
		int m = _modeStack.Pop();
		Mode(m);
		return _mode;
	}

	public virtual void SetInputStream(ICharStream input)
	{
		_input = null;
		_tokenFactorySourcePair = Tuple.Create((ITokenSource)this, _input);
		Reset();
		_input = input;
		_tokenFactorySourcePair = Tuple.Create((ITokenSource)this, _input);
	}

	public virtual void Emit(IToken token)
	{
		_token = token;
	}

	public virtual IToken Emit()
	{
		IToken token = _factory.Create(_tokenFactorySourcePair, _type, _text, _channel, _tokenStartCharIndex, CharIndex - 1, _tokenStartLine, _tokenStartColumn);
		Emit(token);
		return token;
	}

	public virtual IToken EmitEOF()
	{
		int column = Column;
		int line = Line;
		IToken token = _factory.Create(_tokenFactorySourcePair, -1, null, 0, _input.Index, _input.Index - 1, line, column);
		Emit(token);
		return token;
	}

	public virtual IList<IToken> GetAllTokens()
	{
		IList<IToken> list = new List<IToken>();
		IToken token = NextToken();
		while (token.Type != -1)
		{
			list.Add(token);
			token = NextToken();
		}
		return list;
	}

	public virtual void Recover(LexerNoViableAltException e)
	{
		if (_input.La(1) != -1)
		{
			Interpreter.Consume(_input);
		}
	}

	public virtual void NotifyListeners(LexerNoViableAltException e)
	{
		string text = _input.GetText(Interval.Of(_tokenStartCharIndex, _input.Index));
		string msg = "token recognition error at: '" + GetErrorDisplay(text) + "'";
		IAntlrErrorListener<int> errorListenerDispatch = ErrorListenerDispatch;
		errorListenerDispatch.SyntaxError(this, 0, _tokenStartLine, _tokenStartColumn, msg, e);
	}

	public virtual string GetErrorDisplay(string s)
	{
		StringBuilder stringBuilder = new StringBuilder();
		char[] array = s.ToCharArray();
		foreach (char c in array)
		{
			stringBuilder.Append(GetErrorDisplay(c));
		}
		return stringBuilder.ToString();
	}

	public virtual string GetErrorDisplay(int c)
	{
		string result = ((char)c).ToString();
		switch (c)
		{
		case -1:
			result = "<EOF>";
			break;
		case 10:
			result = "\\n";
			break;
		case 9:
			result = "\\t";
			break;
		case 13:
			result = "\\r";
			break;
		}
		return result;
	}

	public virtual string GetCharErrorDisplay(int c)
	{
		string errorDisplay = GetErrorDisplay(c);
		return "'" + errorDisplay + "'";
	}

	public virtual void Recover(RecognitionException re)
	{
		_input.Consume();
	}
}
