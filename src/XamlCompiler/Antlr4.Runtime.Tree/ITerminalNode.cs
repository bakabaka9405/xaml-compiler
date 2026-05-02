namespace Antlr4.Runtime.Tree;

public interface ITerminalNode : IParseTree, ISyntaxTree, ITree
{
	IToken Symbol { get; }

	new IRuleNode Parent { get; }
}
