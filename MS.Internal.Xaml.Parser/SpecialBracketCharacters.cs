using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MS.Internal.Xaml.Parser;

internal class SpecialBracketCharacters : ISupportInitialize
{
	private string _startChars;

	private string _endChars;

	private static readonly ISet<char> _restrictedCharSet = new SortedSet<char>(new char[7] { '=', ',', '\'', '"', '{', '}', '\\' });

	private bool _initializing;

	private StringBuilder _startCharactersStringBuilder;

	private StringBuilder _endCharactersStringBuilder;

	internal string StartBracketCharacters => _startChars;

	internal string EndBracketCharacters => _endChars;

	internal SpecialBracketCharacters()
	{
		BeginInit();
	}

	internal SpecialBracketCharacters(IReadOnlyDictionary<char, char> attributeList)
	{
		BeginInit();
		if (attributeList != null && attributeList.Count > 0)
		{
			Tokenize(attributeList);
		}
	}

	internal void AddBracketCharacters(char openingBracket, char closingBracket)
	{
		if (_initializing)
		{
			_startCharactersStringBuilder.Append(openingBracket);
			_endCharactersStringBuilder.Append(closingBracket);
			return;
		}
		throw new InvalidOperationException();
	}

	private void Tokenize(IReadOnlyDictionary<char, char> attributeList)
	{
		if (!_initializing)
		{
			return;
		}
		foreach (char key in attributeList.Keys)
		{
			char c = attributeList[key];
			string empty = string.Empty;
			if (IsValidBracketCharacter(key, c))
			{
				_startCharactersStringBuilder.Append(key);
				_endCharactersStringBuilder.Append(c);
			}
		}
	}

	private bool IsValidBracketCharacter(char openingBracket, char closingBracket)
	{
		if (openingBracket == closingBracket)
		{
			throw new InvalidOperationException("Opening bracket character cannot be the same as closing bracket character.");
		}
		if (char.IsLetterOrDigit(openingBracket) || char.IsLetterOrDigit(closingBracket) || char.IsWhiteSpace(openingBracket) || char.IsWhiteSpace(closingBracket))
		{
			throw new InvalidOperationException("Bracket characters cannot be alpha-numeric or whitespace.");
		}
		if (_restrictedCharSet.Contains(openingBracket) || _restrictedCharSet.Contains(closingBracket))
		{
			throw new InvalidOperationException("Bracket characters cannot be one of the following: '=' , ',', ''', '\"', '{ ', ' }', '\\'");
		}
		return true;
	}

	internal bool IsSpecialCharacter(char ch)
	{
		if (!_startChars.Contains(ch.ToString()))
		{
			return _endChars.Contains(ch.ToString());
		}
		return true;
	}

	internal bool StartsEscapeSequence(char ch)
	{
		return _startChars.Contains(ch.ToString());
	}

	internal bool EndsEscapeSequence(char ch)
	{
		return _endChars.Contains(ch.ToString());
	}

	internal bool Match(char start, char end)
	{
		return _endChars.IndexOf(end.ToString()) == _startChars.IndexOf(start.ToString());
	}

	public void BeginInit()
	{
		_initializing = true;
		_startCharactersStringBuilder = new StringBuilder();
		_endCharactersStringBuilder = new StringBuilder();
	}

	public void EndInit()
	{
		_startChars = _startCharactersStringBuilder.ToString();
		_endChars = _endCharactersStringBuilder.ToString();
		_initializing = false;
	}
}
