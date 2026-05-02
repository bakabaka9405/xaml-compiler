using System;
using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal static class KS
{
	public static bool Eq(string a, string b)
	{
		return string.Equals(a, b, StringComparison.Ordinal);
	}

	public static bool EqIgnoreCase(string a, string b)
	{
		return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
	}

	public static int IndexOf(string src, string chars)
	{
		return src.IndexOf(chars, StringComparison.Ordinal);
	}

	public static bool StartsWith(string src, string target)
	{
		return src.StartsWith(target, StringComparison.Ordinal);
	}

	public static bool StartsWithIgnoreCase(string src, string target)
	{
		return src.StartsWith(target, StringComparison.OrdinalIgnoreCase);
	}

	public static bool ContainsString(IList<string> list, string s)
	{
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (string.Equals(list[i], s, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}
}
