using System.Collections.Generic;

namespace Antlr4.Runtime.Tree.Xpath;

public class XPathTokenElement : XPathElement
{
	protected internal int tokenType;

	public XPathTokenElement(string tokenName, int tokenType)
		: base(tokenName)
	{
		this.tokenType = tokenType;
	}

	public override ICollection<IParseTree> Evaluate(IParseTree t)
	{
		IList<IParseTree> list = new List<IParseTree>();
		foreach (ITree child in Trees.GetChildren(t))
		{
			if (child is ITerminalNode)
			{
				ITerminalNode terminalNode = (ITerminalNode)child;
				if ((terminalNode.Symbol.Type == tokenType && !invert) || (terminalNode.Symbol.Type != tokenType && invert))
				{
					list.Add(terminalNode);
				}
			}
		}
		return list;
	}
}
