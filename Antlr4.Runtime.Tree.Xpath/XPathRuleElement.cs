using System.Collections.Generic;

namespace Antlr4.Runtime.Tree.Xpath;

public class XPathRuleElement : XPathElement
{
	protected internal int ruleIndex;

	public XPathRuleElement(string ruleName, int ruleIndex)
		: base(ruleName)
	{
		this.ruleIndex = ruleIndex;
	}

	public override ICollection<IParseTree> Evaluate(IParseTree t)
	{
		IList<IParseTree> list = new List<IParseTree>();
		foreach (ITree child in Trees.GetChildren(t))
		{
			if (child is ParserRuleContext)
			{
				ParserRuleContext parserRuleContext = (ParserRuleContext)child;
				if ((parserRuleContext.RuleIndex == ruleIndex && !invert) || (parserRuleContext.RuleIndex != ruleIndex && invert))
				{
					list.Add(parserRuleContext);
				}
			}
		}
		return list;
	}
}
