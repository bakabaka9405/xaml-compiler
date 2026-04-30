using System;
using System.Collections.Generic;
using System.Text;
using System.Xaml;
using System.Xaml.MS.Impl;
using System.Xaml.Schema;
using MS.Internal.Xaml.Context;

namespace MS.Internal.Xaml.Parser;

internal class MeScanner
{
	private enum StringState
	{
		Value,
		Type,
		Property
	}

	public const char Space = ' ';

	public const char OpenCurlie = '{';

	public const char CloseCurlie = '}';

	public const char Comma = ',';

	public const char EqualSign = '=';

	public const char Quote1 = '\'';

	public const char Quote2 = '"';

	public const char Backslash = '\\';

	public const char NullChar = '\0';

	private XamlParserContext _context;

	private string _inputText;

	private int _idx;

	private MeTokenType _token;

	private XamlType _tokenXamlType;

	private XamlMember _tokenProperty;

	private string _tokenNamespace;

	private string _tokenText;

	private StringState _state;

	private bool _hasTrailingWhitespace;

	private int _lineNumber;

	private int _startPosition;

	private string _currentParameterName;

	private SpecialBracketCharacters _currentSpecialBracketCharacters;

	public int LineNumber => _lineNumber;

	public int LinePosition
	{
		get
		{
			int num = ((_idx >= 0) ? _idx : 0);
			return _startPosition + num;
		}
	}

	public string Namespace => _tokenNamespace;

	public MeTokenType Token => _token;

	public XamlType TokenType => _tokenXamlType;

	public XamlMember TokenProperty => _tokenProperty;

	public string TokenText => _tokenText;

	public bool IsAtEndOfInput => _idx >= _inputText.Length;

	public bool HasTrailingWhitespace => _hasTrailingWhitespace;

	private char CurrentChar => _inputText[_idx];

	private char NextChar
	{
		get
		{
			if (_idx + 1 < _inputText.Length)
			{
				return _inputText[_idx + 1];
			}
			return '\0';
		}
	}

	public MeScanner(XamlParserContext context, string text, int lineNumber, int linePosition)
	{
		_context = context;
		_inputText = text;
		_lineNumber = lineNumber;
		_startPosition = linePosition;
		_idx = -1;
		_state = StringState.Value;
		_currentParameterName = null;
		_currentSpecialBracketCharacters = null;
	}

	public void Read()
	{
		bool flag = false;
		bool flag2 = false;
		_tokenText = string.Empty;
		_tokenXamlType = null;
		_tokenProperty = null;
		_tokenNamespace = null;
		Advance();
		AdvanceOverWhitespace();
		if (IsAtEndOfInput)
		{
			_token = MeTokenType.None;
			return;
		}
		switch (CurrentChar)
		{
		case '{':
			if (NextChar == '}')
			{
				_token = MeTokenType.String;
				_state = StringState.Value;
				flag2 = true;
			}
			else
			{
				_token = MeTokenType.Open;
				_state = StringState.Type;
			}
			break;
		case '"':
		case '\'':
			if (NextChar == '{')
			{
				Advance();
				if (NextChar != '}')
				{
					flag = true;
				}
				PushBack();
			}
			flag2 = true;
			break;
		case '}':
			_token = MeTokenType.Close;
			_state = StringState.Value;
			break;
		case '=':
			_token = MeTokenType.EqualSign;
			_state = StringState.Value;
			_context.CurrentBracketModeParseParameters.IsConstructorParsingMode = false;
			break;
		case ',':
			_token = MeTokenType.Comma;
			_state = StringState.Value;
			if (_context.CurrentBracketModeParseParameters.IsConstructorParsingMode)
			{
				_context.CurrentBracketModeParseParameters.IsConstructorParsingMode = ++_context.CurrentBracketModeParseParameters.CurrentConstructorParam < _context.CurrentBracketModeParseParameters.MaxConstructorParams;
			}
			break;
		default:
			flag2 = true;
			break;
		}
		if (flag2)
		{
			if (_context.CurrentType.IsMarkupExtension && _context.CurrentBracketModeParseParameters != null && _context.CurrentBracketModeParseParameters.IsConstructorParsingMode)
			{
				int currentConstructorParam = _context.CurrentBracketModeParseParameters.CurrentConstructorParam;
				_currentParameterName = _context.CurrentLongestConstructorOfMarkupExtension[currentConstructorParam].Name;
				_currentSpecialBracketCharacters = GetBracketCharacterForProperty(_currentParameterName);
			}
			string text = ReadString();
			_token = (flag ? MeTokenType.QuotedMarkupExtension : MeTokenType.String);
			switch (_state)
			{
			case StringState.Type:
				_token = MeTokenType.TypeName;
				ResolveTypeName(text);
				break;
			case StringState.Property:
				_token = MeTokenType.PropertyName;
				ResolvePropertyName(text);
				break;
			}
			_state = StringState.Value;
			_tokenText = RemoveEscapes(text);
		}
	}

	private static string RemoveEscapes(string value)
	{
		if (value.StartsWith("{}", StringComparison.OrdinalIgnoreCase))
		{
			value = value.Substring(2);
		}
		if (!value.Contains("\\"))
		{
			return value;
		}
		StringBuilder stringBuilder = new StringBuilder(value.Length);
		int num = 0;
		do
		{
			int num2 = value.IndexOf('\\', num);
			if (num2 < 0)
			{
				stringBuilder.Append(value.Substring(num));
				break;
			}
			int length = num2 - num;
			stringBuilder.Append(value.Substring(num, length));
			if (num2 + 1 < value.Length)
			{
				stringBuilder.Append(value[num2 + 1]);
			}
			num = num2 + 2;
		}
		while (num < value.Length);
		return stringBuilder.ToString();
	}

	private void ResolveTypeName(string longName)
	{
		string error;
		XamlTypeName xamlTypeName = XamlTypeName.ParseInternal(longName, _context.FindNamespaceByPrefix, out error);
		if (xamlTypeName == null)
		{
			throw new XamlParseException(this, error);
		}
		string name = xamlTypeName.Name;
		xamlTypeName.Name += "Extension";
		XamlType xamlType = _context.GetXamlType(xamlTypeName, returnUnknownTypesOnFailure: false);
		if (xamlType == null || (xamlType.UnderlyingType != null && KS.Eq(xamlType.UnderlyingType.Name, xamlTypeName.Name + "Extension")))
		{
			xamlTypeName.Name = name;
			xamlType = _context.GetXamlType(xamlTypeName, returnUnknownTypesOnFailure: true);
		}
		_tokenXamlType = xamlType;
		_tokenNamespace = xamlTypeName.Namespace;
	}

	private void ResolvePropertyName(string longName)
	{
		XamlPropertyName xamlPropertyName = XamlPropertyName.Parse(longName);
		if (xamlPropertyName == null)
		{
			throw new ArgumentException(SR.Get("MalformedPropertyName"));
		}
		XamlMember xamlMember = null;
		XamlType currentType = _context.CurrentType;
		string currentTypeNamespace = _context.CurrentTypeNamespace;
		if (xamlPropertyName.IsDotted)
		{
			xamlMember = _context.GetDottedProperty(currentType, currentTypeNamespace, xamlPropertyName, tagIsRoot: false);
		}
		else
		{
			string attributeNamespace = _context.GetAttributeNamespace(xamlPropertyName, _tokenNamespace);
			XamlType currentType2 = _context.CurrentType;
			xamlMember = _context.GetNoDotAttributeProperty(currentType2, xamlPropertyName, _tokenNamespace, attributeNamespace, tagIsRoot: false);
		}
		_tokenProperty = xamlMember;
	}

	private string ReadString()
	{
		bool flag = false;
		char c = '\0';
		bool flag2 = true;
		bool flag3 = false;
		uint num = 0u;
		StringBuilder stringBuilder = new StringBuilder();
		while (!IsAtEndOfInput)
		{
			char currentChar = CurrentChar;
			if (flag)
			{
				stringBuilder.Append('\\');
				stringBuilder.Append(currentChar);
				flag = false;
			}
			else if (c != 0)
			{
				if (currentChar == '\\')
				{
					flag = true;
				}
				else
				{
					if (currentChar == c)
					{
						currentChar = CurrentChar;
						c = '\0';
						break;
					}
					stringBuilder.Append(currentChar);
				}
			}
			else if (_context.CurrentBracketModeParseParameters != null && _context.CurrentBracketModeParseParameters.IsBracketEscapeMode)
			{
				Stack<char> bracketCharacterStack = _context.CurrentBracketModeParseParameters.BracketCharacterStack;
				if (_currentSpecialBracketCharacters.StartsEscapeSequence(currentChar))
				{
					bracketCharacterStack.Push(currentChar);
				}
				else if (_currentSpecialBracketCharacters.EndsEscapeSequence(currentChar))
				{
					if (!_currentSpecialBracketCharacters.Match(bracketCharacterStack.Peek(), currentChar))
					{
						throw new XamlParseException(this, SR.Get("InvalidClosingBracketCharacers", currentChar.ToString()));
					}
					bracketCharacterStack.Pop();
				}
				else if (currentChar == '\\')
				{
					flag = true;
				}
				if (bracketCharacterStack.Count == 0)
				{
					_context.CurrentBracketModeParseParameters.IsBracketEscapeMode = false;
				}
				if (!flag)
				{
					stringBuilder.Append(currentChar);
				}
			}
			else
			{
				bool flag4 = false;
				switch (currentChar)
				{
				case ' ':
					if (_state == StringState.Type)
					{
						flag4 = true;
					}
					else
					{
						stringBuilder.Append(currentChar);
					}
					break;
				case '{':
					num++;
					stringBuilder.Append(currentChar);
					break;
				case '}':
					if (num == 0)
					{
						flag4 = true;
						break;
					}
					num--;
					stringBuilder.Append(currentChar);
					break;
				case ',':
					flag4 = true;
					break;
				case '=':
					_state = StringState.Property;
					flag4 = true;
					break;
				case '\\':
					flag = true;
					break;
				case '"':
				case '\'':
					if (!flag2)
					{
						throw new XamlParseException(this, SR.Get("QuoteCharactersOutOfPlace"));
					}
					c = currentChar;
					flag3 = true;
					break;
				default:
					if (_currentSpecialBracketCharacters != null && _currentSpecialBracketCharacters.StartsEscapeSequence(currentChar))
					{
						Stack<char> bracketCharacterStack2 = _context.CurrentBracketModeParseParameters.BracketCharacterStack;
						bracketCharacterStack2.Clear();
						bracketCharacterStack2.Push(currentChar);
						_context.CurrentBracketModeParseParameters.IsBracketEscapeMode = true;
					}
					stringBuilder.Append(currentChar);
					break;
				}
				if (flag4)
				{
					if (num != 0)
					{
						throw new XamlParseException(this, SR.Get("UnexpectedTokenAfterME"));
					}
					BracketModeParseParameters currentBracketModeParseParameters = _context.CurrentBracketModeParseParameters;
					if (currentBracketModeParseParameters != null && currentBracketModeParseParameters.BracketCharacterStack.Count > 0)
					{
						throw new XamlParseException(this, SR.Get("MalformedBracketCharacters", currentChar.ToString()));
					}
					PushBack();
					break;
				}
			}
			flag2 = false;
			Advance();
		}
		if (c != 0)
		{
			throw new XamlParseException(this, SR.Get("UnclosedQuote"));
		}
		string text = stringBuilder.ToString();
		if (!flag3)
		{
			text = text.TrimEnd(KnownStrings.WhitespaceChars);
			text = text.TrimStart(KnownStrings.WhitespaceChars);
		}
		if (_state == StringState.Property)
		{
			_currentParameterName = text;
			_currentSpecialBracketCharacters = GetBracketCharacterForProperty(_currentParameterName);
		}
		return text;
	}

	private bool Advance()
	{
		_idx++;
		if (IsAtEndOfInput)
		{
			_idx = _inputText.Length;
			return false;
		}
		return true;
	}

	private static bool IsWhitespaceChar(char ch)
	{
		if (ch == KnownStrings.WhitespaceChars[0] || ch == KnownStrings.WhitespaceChars[1] || ch == KnownStrings.WhitespaceChars[2] || ch == KnownStrings.WhitespaceChars[3] || ch == KnownStrings.WhitespaceChars[4])
		{
			return true;
		}
		return false;
	}

	private void AdvanceOverWhitespace()
	{
		bool flag = false;
		while (!IsAtEndOfInput && IsWhitespaceChar(CurrentChar))
		{
			flag = true;
			Advance();
		}
		if (IsAtEndOfInput && flag)
		{
			_hasTrailingWhitespace = true;
		}
	}

	private void PushBack()
	{
		_idx--;
	}

	private SpecialBracketCharacters GetBracketCharacterForProperty(string propertyName)
	{
		SpecialBracketCharacters result = null;
		if (_context.CurrentEscapeCharacterMapForMarkupExtension != null && _context.CurrentEscapeCharacterMapForMarkupExtension.ContainsKey(propertyName))
		{
			result = _context.CurrentEscapeCharacterMapForMarkupExtension[propertyName];
		}
		return result;
	}
}
