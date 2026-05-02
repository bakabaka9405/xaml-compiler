using System;

namespace Antlr4.Runtime.Misc;

[Serializable]
public class ParseCanceledException : OperationCanceledException
{
	public ParseCanceledException()
	{
	}

	public ParseCanceledException(string message)
		: base(message)
	{
	}

	public ParseCanceledException(Exception cause)
		: base("The parse operation was cancelled.", cause)
	{
	}

	public ParseCanceledException(string message, Exception cause)
		: base(message, cause)
	{
	}
}
