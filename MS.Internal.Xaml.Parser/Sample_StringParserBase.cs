using System.Xaml.MS.Impl;

namespace MS.Internal.Xaml.Parser;

internal class Sample_StringParserBase
{
	protected const char NullChar = '\0';

	protected string _inputText;

	protected int _idx;

	protected char CurrentChar => _inputText[_idx];

	public bool IsAtEndOfInput => _idx >= _inputText.Length;

	public Sample_StringParserBase(string text)
	{
		_inputText = text;
		_idx = 0;
	}

	protected bool Advance()
	{
		_idx++;
		if (IsAtEndOfInput)
		{
			_idx = _inputText.Length;
			return false;
		}
		return true;
	}

	protected static bool IsWhitespaceChar(char ch)
	{
		if (ch == KnownStrings.WhitespaceChars[0] || ch == KnownStrings.WhitespaceChars[1] || ch == KnownStrings.WhitespaceChars[2] || ch == KnownStrings.WhitespaceChars[3] || ch == KnownStrings.WhitespaceChars[4])
		{
			return true;
		}
		return false;
	}

	protected bool AdvanceOverWhitespace()
	{
		bool result = true;
		while (!IsAtEndOfInput && IsWhitespaceChar(CurrentChar))
		{
			result = true;
			Advance();
		}
		return result;
	}
}
