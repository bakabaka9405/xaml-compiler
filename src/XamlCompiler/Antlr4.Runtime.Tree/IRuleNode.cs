namespace Antlr4.Runtime.Tree;

public interface IRuleNode : IParseTree, ISyntaxTree, ITree
{
	RuleContext RuleContext { get; }

	new IRuleNode Parent { get; }
}
