using System.Collections.Generic;

namespace Antlr4.Runtime.Tree.Xpath;

public class XPathRuleAnywhereElement : XPathElement
{
	protected internal int ruleIndex;

	public XPathRuleAnywhereElement(string ruleName, int ruleIndex)
		: base(ruleName)
	{
		this.ruleIndex = ruleIndex;
	}

	public override ICollection<IParseTree> Evaluate(IParseTree t)
	{
		return Trees.FindAllRuleNodes(t, ruleIndex);
	}
}
