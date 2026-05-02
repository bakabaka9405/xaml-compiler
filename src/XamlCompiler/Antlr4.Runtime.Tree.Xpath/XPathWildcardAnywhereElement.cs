using System.Collections.Generic;

namespace Antlr4.Runtime.Tree.Xpath;

public class XPathWildcardAnywhereElement : XPathElement
{
	public XPathWildcardAnywhereElement()
		: base("*")
	{
	}

	public override ICollection<IParseTree> Evaluate(IParseTree t)
	{
		if (invert)
		{
			return new List<IParseTree>();
		}
		return Trees.Descendants(t);
	}
}
