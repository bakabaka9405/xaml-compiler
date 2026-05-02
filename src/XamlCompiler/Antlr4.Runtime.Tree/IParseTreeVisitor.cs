namespace Antlr4.Runtime.Tree;

public interface IParseTreeVisitor<out Result>
{
	Result Visit(IParseTree tree);

	Result VisitChildren(IRuleNode node);

	Result VisitTerminal(ITerminalNode node);

	Result VisitErrorNode(IErrorNode node);
}
