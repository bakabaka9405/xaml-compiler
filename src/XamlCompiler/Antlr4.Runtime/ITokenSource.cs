using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public interface ITokenSource
{
	int Line { get; }

	int Column { get; }

	ICharStream InputStream { get; }

	string SourceName { get; }

	ITokenFactory TokenFactory { get; set; }

	[return: NotNull]
	IToken NextToken();
}
