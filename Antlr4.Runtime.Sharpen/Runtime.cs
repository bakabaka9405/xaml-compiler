using System;

namespace Antlr4.Runtime.Sharpen;

internal static class Runtime
{
	public static string Substring(string str, int beginOffset, int endOffset)
	{
		if (str == null)
		{
			throw new ArgumentNullException("str");
		}
		return str.Substring(beginOffset, endOffset - beginOffset);
	}
}
