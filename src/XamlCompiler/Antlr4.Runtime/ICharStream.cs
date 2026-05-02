using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public interface ICharStream : IIntStream
{
	[return: NotNull]
	string GetText(Interval interval);
}
