using System.Collections.Generic;

namespace Antlr4.Runtime.Tree.Xpath;

public abstract class XPathElement
{
	protected internal string nodeName;

	protected internal bool invert;

	public XPathElement(string nodeName)
	{
		this.nodeName = nodeName;
	}

	public abstract ICollection<IParseTree> Evaluate(IParseTree t);

	public override string ToString()
	{
		string text = (invert ? "!" : string.Empty);
		return GetType().Name + "[" + text + nodeName + "]";
	}
}
