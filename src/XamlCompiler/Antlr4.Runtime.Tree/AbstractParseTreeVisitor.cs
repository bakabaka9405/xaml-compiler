namespace Antlr4.Runtime.Tree;

public abstract class AbstractParseTreeVisitor<Result> : IParseTreeVisitor<Result>
{
	protected internal virtual Result DefaultResult => default(Result);

	public virtual Result Visit(IParseTree tree)
	{
		return tree.Accept(this);
	}

	public virtual Result VisitChildren(IRuleNode node)
	{
		Result val = DefaultResult;
		int childCount = node.ChildCount;
		for (int i = 0; i < childCount; i++)
		{
			if (!ShouldVisitNextChild(node, val))
			{
				break;
			}
			IParseTree child = node.GetChild(i);
			Result nextResult = child.Accept(this);
			val = AggregateResult(val, nextResult);
		}
		return val;
	}

	public virtual Result VisitTerminal(ITerminalNode node)
	{
		return DefaultResult;
	}

	public virtual Result VisitErrorNode(IErrorNode node)
	{
		return DefaultResult;
	}

	protected internal virtual Result AggregateResult(Result aggregate, Result nextResult)
	{
		return nextResult;
	}

	protected internal virtual bool ShouldVisitNextChild(IRuleNode node, Result currentResult)
	{
		return true;
	}
}
