namespace Antlr4.Runtime.Tree;

public class ParseTreeWalker
{
	public static readonly ParseTreeWalker Default = new ParseTreeWalker();

	public virtual void Walk(IParseTreeListener listener, IParseTree t)
	{
		if (t is IErrorNode)
		{
			listener.VisitErrorNode((IErrorNode)t);
			return;
		}
		if (t is ITerminalNode)
		{
			listener.VisitTerminal((ITerminalNode)t);
			return;
		}
		IRuleNode ruleNode = (IRuleNode)t;
		EnterRule(listener, ruleNode);
		int childCount = ruleNode.ChildCount;
		for (int i = 0; i < childCount; i++)
		{
			Walk(listener, ruleNode.GetChild(i));
		}
		ExitRule(listener, ruleNode);
	}

	protected internal virtual void EnterRule(IParseTreeListener listener, IRuleNode r)
	{
		ParserRuleContext parserRuleContext = (ParserRuleContext)r.RuleContext;
		listener.EnterEveryRule(parserRuleContext);
		parserRuleContext.EnterRule(listener);
	}

	protected internal virtual void ExitRule(IParseTreeListener listener, IRuleNode r)
	{
		ParserRuleContext parserRuleContext = (ParserRuleContext)r.RuleContext;
		parserRuleContext.ExitRule(listener);
		listener.ExitEveryRule(parserRuleContext);
	}
}
