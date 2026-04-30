using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public interface ITokenStream : IIntStream
{
	ITokenSource TokenSource { get; }

	[return: NotNull]
	IToken Lt(int k);

	[return: NotNull]
	IToken Get(int i);

	[return: NotNull]
	string GetText(Interval interval);

	[return: NotNull]
	string GetText();

	[return: NotNull]
	string GetText(RuleContext ctx);

	[return: NotNull]
	string GetText(IToken start, IToken stop);
}
