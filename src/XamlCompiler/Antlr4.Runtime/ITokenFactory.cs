using System;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public interface ITokenFactory
{
	[return: NotNull]
	IToken Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start, int stop, int line, int charPositionInLine);

	[return: NotNull]
	IToken Create(int type, string text);
}
