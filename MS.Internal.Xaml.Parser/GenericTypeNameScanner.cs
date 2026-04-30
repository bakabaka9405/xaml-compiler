namespace MS.Internal.Xaml.Parser;

internal class GenericTypeNameScanner : Sample_StringParserBase
{
	internal enum State
	{
		START,
		INNAME,
		INSUBSCRIPT
	}

	public const char Space = ' ';

	public const char OpenParen = '(';

	public const char CloseParen = ')';

	public const char Comma = ',';

	public const char OpenBracket = '[';

	public const char CloseBracket = ']';

	public const char Colon = ':';

	private GenericTypeNameScannerToken _token;

	private string _tokenText;

	private State _state;

	private GenericTypeNameScannerToken _pushedBackSymbol;

	private int _multiCharTokenStartIdx;

	private int _multiCharTokenLength;

	private char _lastChar;

	public GenericTypeNameScannerToken Token => _token;

	public string MultiCharTokenText => _tokenText;

	public char ErrorCurrentChar => _lastChar;

	public GenericTypeNameScanner(string text)
		: base(text)
	{
		_state = State.START;
		_pushedBackSymbol = GenericTypeNameScannerToken.NONE;
	}

	public void Read()
	{
		if (_pushedBackSymbol != GenericTypeNameScannerToken.NONE)
		{
			_token = _pushedBackSymbol;
			_pushedBackSymbol = GenericTypeNameScannerToken.NONE;
			return;
		}
		_token = GenericTypeNameScannerToken.NONE;
		_tokenText = string.Empty;
		_multiCharTokenStartIdx = -1;
		_multiCharTokenLength = 0;
		while (_token == GenericTypeNameScannerToken.NONE)
		{
			if (base.IsAtEndOfInput)
			{
				if (_state == State.INNAME)
				{
					_token = GenericTypeNameScannerToken.NAME;
					_state = State.START;
				}
				if (_state == State.INSUBSCRIPT)
				{
					_token = GenericTypeNameScannerToken.ERROR;
					_state = State.START;
				}
				break;
			}
			switch (_state)
			{
			case State.START:
				State_Start();
				break;
			case State.INNAME:
				State_InName();
				break;
			case State.INSUBSCRIPT:
				State_InSubscript();
				break;
			}
		}
		if (_token == GenericTypeNameScannerToken.NAME || _token == GenericTypeNameScannerToken.SUBSCRIPT)
		{
			_tokenText = CollectMultiCharToken();
		}
	}

	internal static int ParseSubscriptSegment(string subscript, ref int pos)
	{
		bool flag = false;
		int num = 1;
		do
		{
			switch (subscript[pos])
			{
			case '[':
				if (flag)
				{
					return 0;
				}
				flag = true;
				break;
			case ',':
				if (!flag)
				{
					return 0;
				}
				num++;
				break;
			case ']':
				if (!flag)
				{
					return 0;
				}
				pos++;
				return num;
			default:
				if (!Sample_StringParserBase.IsWhitespaceChar(subscript[pos]))
				{
					return 0;
				}
				break;
			}
			pos++;
		}
		while (pos < subscript.Length);
		return 0;
	}

	internal static string StripSubscript(string typeName, out string subscript)
	{
		int num = typeName.IndexOf('[');
		if (num < 0)
		{
			subscript = null;
			return typeName;
		}
		subscript = typeName.Substring(num);
		return typeName.Substring(0, num);
	}

	private void State_Start()
	{
		AdvanceOverWhitespace();
		if (base.IsAtEndOfInput)
		{
			_token = GenericTypeNameScannerToken.NONE;
			return;
		}
		switch (base.CurrentChar)
		{
		case '(':
			_token = GenericTypeNameScannerToken.OPEN;
			break;
		case ')':
			_token = GenericTypeNameScannerToken.CLOSE;
			break;
		case ',':
			_token = GenericTypeNameScannerToken.COMMA;
			break;
		case ':':
			_token = GenericTypeNameScannerToken.COLON;
			break;
		case '[':
			StartMultiCharToken();
			_state = State.INSUBSCRIPT;
			break;
		default:
			if (XamlName.IsValidNameStartChar(base.CurrentChar))
			{
				StartMultiCharToken();
				_state = State.INNAME;
			}
			else
			{
				_token = GenericTypeNameScannerToken.ERROR;
			}
			break;
		}
		_lastChar = base.CurrentChar;
		Advance();
	}

	private void State_InName()
	{
		if (base.IsAtEndOfInput || Sample_StringParserBase.IsWhitespaceChar(base.CurrentChar) || base.CurrentChar == '[')
		{
			_token = GenericTypeNameScannerToken.NAME;
			_state = State.START;
			return;
		}
		switch (base.CurrentChar)
		{
		case '(':
			_pushedBackSymbol = GenericTypeNameScannerToken.OPEN;
			_token = GenericTypeNameScannerToken.NAME;
			_state = State.START;
			break;
		case ')':
			_pushedBackSymbol = GenericTypeNameScannerToken.CLOSE;
			_token = GenericTypeNameScannerToken.NAME;
			_state = State.START;
			break;
		case ',':
			_pushedBackSymbol = GenericTypeNameScannerToken.COMMA;
			_token = GenericTypeNameScannerToken.NAME;
			_state = State.START;
			break;
		case ':':
			_pushedBackSymbol = GenericTypeNameScannerToken.COLON;
			_token = GenericTypeNameScannerToken.NAME;
			_state = State.START;
			break;
		default:
			if (XamlName.IsValidQualifiedNameChar(base.CurrentChar))
			{
				AddToMultiCharToken();
			}
			else
			{
				_token = GenericTypeNameScannerToken.ERROR;
			}
			break;
		}
		_lastChar = base.CurrentChar;
		Advance();
	}

	private void State_InSubscript()
	{
		if (base.IsAtEndOfInput)
		{
			_token = GenericTypeNameScannerToken.ERROR;
			_state = State.START;
			return;
		}
		switch (base.CurrentChar)
		{
		case ',':
			AddToMultiCharToken();
			break;
		case ']':
			AddToMultiCharToken();
			_token = GenericTypeNameScannerToken.SUBSCRIPT;
			_state = State.START;
			break;
		default:
			if (Sample_StringParserBase.IsWhitespaceChar(base.CurrentChar))
			{
				AddToMultiCharToken();
			}
			else
			{
				_token = GenericTypeNameScannerToken.ERROR;
			}
			break;
		}
		_lastChar = base.CurrentChar;
		Advance();
	}

	private void StartMultiCharToken()
	{
		_multiCharTokenStartIdx = _idx;
		_multiCharTokenLength = 1;
	}

	private void AddToMultiCharToken()
	{
		_multiCharTokenLength++;
	}

	private string CollectMultiCharToken()
	{
		if (_multiCharTokenStartIdx == 0 && _multiCharTokenLength == _inputText.Length)
		{
			return _inputText;
		}
		return _inputText.Substring(_multiCharTokenStartIdx, _multiCharTokenLength);
	}
}
