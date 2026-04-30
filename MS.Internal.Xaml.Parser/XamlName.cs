using System.Globalization;

namespace MS.Internal.Xaml.Parser;

internal abstract class XamlName
{
	public const char PlusSign = '+';

	public const char UnderScore = '_';

	public const char Dot = '.';

	protected string _prefix;

	protected string _namespace;

	public string Name { get; protected set; }

	public abstract string ScopedName { get; }

	public string Prefix => _prefix;

	public string Namespace => _namespace;

	protected XamlName()
		: this(string.Empty)
	{
	}

	public XamlName(string name)
	{
		Name = name;
	}

	public XamlName(string prefix, string name)
	{
		Name = name;
		_prefix = prefix ?? string.Empty;
	}

	public static bool ContainsDot(string name)
	{
		return name.Contains(".");
	}

	public static bool IsValidXamlName(string name)
	{
		if (name.Length == 0)
		{
			return false;
		}
		if (!IsValidNameStartChar(name[0]))
		{
			return false;
		}
		for (int i = 1; i < name.Length; i++)
		{
			if (!IsValidNameChar(name[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsValidNameStartChar(char ch)
	{
		if (!char.IsLetter(ch))
		{
			return ch == '_';
		}
		return true;
	}

	public static bool IsValidNameChar(char ch)
	{
		if (IsValidNameStartChar(ch) || char.IsDigit(ch))
		{
			return true;
		}
		UnicodeCategory unicodeCategory = char.GetUnicodeCategory(ch);
		if (unicodeCategory == UnicodeCategory.NonSpacingMark || unicodeCategory == UnicodeCategory.SpacingCombiningMark)
		{
			return true;
		}
		return false;
	}

	public static bool IsValidQualifiedNameChar(char ch)
	{
		if (ch != '.')
		{
			return IsValidNameChar(ch);
		}
		return true;
	}

	public static bool IsValidQualifiedNameCharPlus(char ch)
	{
		if (!IsValidQualifiedNameChar(ch))
		{
			return ch == '+';
		}
		return true;
	}
}
