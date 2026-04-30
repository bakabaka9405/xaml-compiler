using System.Diagnostics;
using System.Text;
using System.Xaml;

namespace MS.Internal.Xaml.Parser;

[DebuggerDisplay("{Text}")]
internal class XamlText
{
	private struct CodePointRange(int min, int max)
	{
		public readonly int Min = min;

		public readonly int Max = max;
	}

	private const char SPACE = ' ';

	private const char NEWLINE = '\n';

	private const char RETURN = '\r';

	private const char TAB = '\t';

	private const char OPENCURLIE = '{';

	private const char CLOSECURLIE = '}';

	private const string ME_ESCAPE = "{}";

	private const string RETURN_STRING = "\r";

	private StringBuilder _sb;

	private readonly bool _isSpacePreserve;

	private bool _isWhiteSpaceOnly;

	private static CodePointRange[] EastAsianCodePointRanges = new CodePointRange[10]
	{
		new CodePointRange(4352, 4607),
		new CodePointRange(11904, 12245),
		new CodePointRange(12272, 12283),
		new CodePointRange(12352, 12703),
		new CodePointRange(12784, 42191),
		new CodePointRange(44032, 55203),
		new CodePointRange(63744, 64255),
		new CodePointRange(65280, 65519),
		new CodePointRange(131072, 173782),
		new CodePointRange(194560, 195101)
	};

	public bool IsEmpty => _sb.Length == 0;

	public string Text => _sb.ToString();

	public string AttributeText
	{
		get
		{
			string text = Text;
			if (text.StartsWith("{}", ignoreCase: false, TypeConverterHelper.InvariantEnglishUS))
			{
				return text.Remove(0, "{}".Length);
			}
			return text;
		}
	}

	public bool IsSpacePreserved => _isSpacePreserve;

	public bool IsWhiteSpaceOnly => _isWhiteSpaceOnly;

	public bool LooksLikeAMarkupExtension
	{
		get
		{
			int length = _sb.Length;
			if (length > 0 && _sb[0] == '{')
			{
				if (length > 1 && _sb[1] == '}')
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public XamlText(bool spacePreserve)
	{
		_sb = new StringBuilder();
		_isSpacePreserve = spacePreserve;
		_isWhiteSpaceOnly = true;
	}

	public void Paste(string text, bool trimLeadingWhitespace, bool convertCRLFtoLF = true)
	{
		bool flag = IsWhitespace(text);
		if (_isSpacePreserve)
		{
			if (convertCRLFtoLF)
			{
				text = text.Replace("\r", "");
			}
			_sb.Append(text);
		}
		else if (flag)
		{
			if (IsEmpty && !trimLeadingWhitespace)
			{
				_sb.Append(' ');
			}
		}
		else
		{
			bool flag2 = IsWhitespaceChar(text[0]);
			bool flag3 = IsWhitespaceChar(text[text.Length - 1]);
			bool flag4 = false;
			string value = CollapseWhitespace(text);
			if (_sb.Length > 0)
			{
				if (_isWhiteSpaceOnly)
				{
					_sb = new StringBuilder();
				}
				else if (IsWhitespaceChar(_sb[_sb.Length - 1]))
				{
					flag4 = true;
				}
			}
			if (flag2 && !trimLeadingWhitespace && !flag4)
			{
				_sb.Append(' ');
			}
			_sb.Append(value);
			if (flag3)
			{
				_sb.Append(' ');
			}
		}
		_isWhiteSpaceOnly &= flag;
	}

	private static bool IsWhitespace(string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (!IsWhitespaceChar(text[i]))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsWhitespaceChar(char ch)
	{
		if (ch != ' ' && ch != '\t' && ch != '\n')
		{
			return ch == '\r';
		}
		return true;
	}

	private static string CollapseWhitespace(string text)
	{
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		int num = 0;
		while (num < text.Length)
		{
			char c = text[num];
			if (!IsWhitespaceChar(c))
			{
				stringBuilder.Append(c);
				num++;
				continue;
			}
			int num2 = num;
			while (++num2 < text.Length && IsWhitespaceChar(text[num2]))
			{
			}
			if (num != 0 && num2 != text.Length)
			{
				bool flag = false;
				if (c == '\n' && num2 - num == 2 && text[num - 1] >= 'ᄀ' && HasSurroundingEastAsianChars(num, num2, text))
				{
					flag = true;
				}
				if (!flag)
				{
					stringBuilder.Append(' ');
				}
			}
			num = num2;
		}
		return stringBuilder.ToString();
	}

	public static string TrimLeadingWhitespace(string source)
	{
		return source.TrimStart(' ', '\t', '\n');
	}

	public static string TrimTrailingWhitespace(string source)
	{
		return source.TrimEnd(' ', '\t', '\n');
	}

	private static bool HasSurroundingEastAsianChars(int start, int end, string text)
	{
		int unicodeScalarValue = ((start - 2 >= 0) ? ComputeUnicodeScalarValue(start - 1, start - 2, text) : text[0]);
		if (IsEastAsianCodePoint(unicodeScalarValue))
		{
			int unicodeScalarValue2 = ((end + 1 < text.Length) ? ComputeUnicodeScalarValue(end, end, text) : text[end]);
			if (IsEastAsianCodePoint(unicodeScalarValue2))
			{
				return true;
			}
		}
		return false;
	}

	private static int ComputeUnicodeScalarValue(int takeOneIdx, int takeTwoIdx, string text)
	{
		int result = 0;
		bool flag = false;
		char c = text[takeTwoIdx];
		if (char.IsHighSurrogate(c))
		{
			char c2 = text[takeTwoIdx + 1];
			if (char.IsLowSurrogate(c2))
			{
				flag = true;
				result = (((c & 0x3FF) << 10) | (c2 & 0x3FF)) + 4096;
			}
		}
		if (!flag)
		{
			result = text[takeOneIdx];
		}
		return result;
	}

	private static bool IsEastAsianCodePoint(int unicodeScalarValue)
	{
		for (int i = 0; i < EastAsianCodePointRanges.Length; i++)
		{
			if (unicodeScalarValue >= EastAsianCodePointRanges[i].Min && unicodeScalarValue <= EastAsianCodePointRanges[i].Max)
			{
				return true;
			}
		}
		return false;
	}
}
