namespace Antlr4.Runtime.Tree;

public class ErrorNodeImpl : TerminalNodeImpl, IErrorNode, ITerminalNode, IParseTree, ISyntaxTree, ITree
{
	public ErrorNodeImpl(IToken token)
		: base(token)
	{
	}

	public override T Accept<T>(IParseTreeVisitor<T> visitor)
	{
		return visitor.VisitErrorNode(this);
	}
}
