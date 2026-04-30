namespace Antlr4.Runtime;

public interface IToken
{
	string Text { get; }

	int Type { get; }

	int Line { get; }

	int Column { get; }

	int Channel { get; }

	int TokenIndex { get; }

	int StartIndex { get; }

	int StopIndex { get; }

	ITokenSource TokenSource { get; }

	ICharStream InputStream { get; }
}
