using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Tree;

public interface ISyntaxTree : ITree
{
	Interval SourceInterval { get; }
}
