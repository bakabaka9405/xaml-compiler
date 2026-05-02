using System.Collections.Generic;

namespace Antlr4.Runtime.Tree.Xpath;

public class XPathTokenAnywhereElement : XPathElement
{
	protected internal int tokenType;

	public XPathTokenAnywhereElement(string tokenName, int tokenType)
		: base(tokenName)
	{
		this.tokenType = tokenType;
	}

	public override ICollection<IParseTree> Evaluate(IParseTree t)
	{
		return Trees.FindAllTokenNodes(t, tokenType);
	}
}
