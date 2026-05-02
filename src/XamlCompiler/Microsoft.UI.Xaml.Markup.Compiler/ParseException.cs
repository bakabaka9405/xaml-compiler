using System;
using System.Globalization;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class ParseException : Exception
{
	public ParseException(string unformatted, params object[] args)
		: base(FormatMessage(unformatted, args))
	{
	}

	private static string FormatMessage(string unformatted, params object[] args)
	{
		if (args != null && args.Length != 0)
		{
			return string.Format(CultureInfo.CurrentCulture, unformatted, args);
		}
		return unformatted;
	}
}
