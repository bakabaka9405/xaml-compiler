using System.Collections.Generic;

namespace Antlr4.Runtime.Tree.Xpath;

public class XPathWildcardElement : XPathElement
{
	public XPathWildcardElement()
		: base("*")
	{
	}

	public override ICollection<IParseTree> Evaluate(IParseTree t)
	{
		if (invert)
		{
			return new List<IParseTree>();
		}
		IList<IParseTree> list = new List<IParseTree>();
		foreach (ITree child in Trees.GetChildren(t))
		{
			list.Add((IParseTree)child);
		}
		return list;
	}
}
