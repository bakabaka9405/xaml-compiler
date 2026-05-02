namespace Antlr4.Runtime.Tree;

public interface IParseTreeListener
{
	void VisitTerminal(ITerminalNode node);

	void VisitErrorNode(IErrorNode node);

	void EnterEveryRule(ParserRuleContext ctx);

	void ExitEveryRule(ParserRuleContext ctx);
}
