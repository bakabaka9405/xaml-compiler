using System;

namespace Antlr4.Runtime.Misc;

public static class Args
{
	public static void NotNull(string parameterName, object value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(parameterName);
		}
	}
}
