using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal static class PropertyPathParser
{
	public static bool Parse(string input, out List<string> qualifiedProperties, out List<string> names)
	{
		qualifiedProperties = null;
		names = null;
		if (!RemoveBrackets(input, out input))
		{
			return false;
		}
		while (input != null)
		{
			string before = null;
			string inside = null;
			string after = null;
			if (!SplitOnParens(input, out before, out inside, out after))
			{
				return false;
			}
			if (before != null)
			{
				string[] array = before.Split('.');
				if (names == null)
				{
					names = new List<string>();
				}
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (!string.IsNullOrWhiteSpace(text))
					{
						names.Add(text);
					}
				}
			}
			if (inside != null)
			{
				if (qualifiedProperties == null)
				{
					qualifiedProperties = new List<string>();
				}
				qualifiedProperties.Add(inside);
			}
			input = after;
		}
		return true;
	}

	private static bool SplitOnParens(string input, out string before, out string inside, out string after)
	{
		before = null;
		inside = null;
		after = null;
		int num = input.IndexOf('(');
		if (num == -1)
		{
			before = input;
			return true;
		}
		int num2 = num;
		if (num2 > 0)
		{
			before = input.Substring(0, num2);
		}
		int num3 = input.IndexOf(')');
		if (num3 == -1)
		{
			return false;
		}
		num2 = num3 - (num + 1);
		if (num2 > 0)
		{
			inside = input.Substring(num + 1, num2);
			int num4 = inside.IndexOf('.');
			if (num4 == -1 || num4 != inside.LastIndexOf('.'))
			{
				return false;
			}
		}
		num2 = input.Length - (num3 + 1);
		if (num2 > 0)
		{
			after = input.Substring(num3 + 1, num2);
		}
		return true;
	}

	private static bool RemoveBrackets(string input, out string result)
	{
		string text = input;
		result = string.Empty;
		while (true)
		{
			int num = text.IndexOf('[');
			int num2 = text.IndexOf(']');
			if (num == -1)
			{
				if (num2 == -1)
				{
					result += text;
					return true;
				}
				return false;
			}
			if (num2 == -1)
			{
				break;
			}
			result += text.Substring(0, num);
			text = text.Substring(num2 + 1);
		}
		return false;
	}
}
