using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree;

public class Trees
{
	public static string ToStringTree(ITree t)
	{
		return ToStringTree(t, (IList<string>)null);
	}

	public static string ToStringTree(ITree t, Parser recog)
	{
		string[] array = recog?.RuleNames;
		IList<string> ruleNames = ((array != null) ? Arrays.AsList(array) : null);
		return ToStringTree(t, ruleNames);
	}

	public static string ToStringTree(ITree t, IList<string> ruleNames)
	{
		string result = Utils.EscapeWhitespace(GetNodeText(t, ruleNames), escapeSpaces: false);
		if (t.ChildCount == 0)
		{
			return result;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(");
		result = Utils.EscapeWhitespace(GetNodeText(t, ruleNames), escapeSpaces: false);
		stringBuilder.Append(result);
		stringBuilder.Append(' ');
		for (int i = 0; i < t.ChildCount; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(ToStringTree(t.GetChild(i), ruleNames));
		}
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	public static string GetNodeText(ITree t, Parser recog)
	{
		string[] array = recog?.RuleNames;
		IList<string> ruleNames = ((array != null) ? Arrays.AsList(array) : null);
		return GetNodeText(t, ruleNames);
	}

	public static string GetNodeText(ITree t, IList<string> ruleNames)
	{
		if (ruleNames != null)
		{
			if (t is IRuleNode)
			{
				int ruleIndex = ((IRuleNode)t).RuleContext.RuleIndex;
				return ruleNames[ruleIndex];
			}
			if (t is IErrorNode)
			{
				return t.ToString();
			}
			if (t is ITerminalNode)
			{
				IToken symbol = ((ITerminalNode)t).Symbol;
				if (symbol != null)
				{
					return symbol.Text;
				}
			}
		}
		object payload = t.Payload;
		if (payload is IToken)
		{
			return ((IToken)payload).Text;
		}
		return t.Payload.ToString();
	}

	public static IList<ITree> GetChildren(ITree t)
	{
		IList<ITree> list = new List<ITree>();
		for (int i = 0; i < t.ChildCount; i++)
		{
			list.Add(t.GetChild(i));
		}
		return list;
	}

	[return: NotNull]
	public static IList<ITree> GetAncestors(ITree t)
	{
		if (t.Parent == null)
		{
			return Collections.EmptyList<ITree>();
		}
		IList<ITree> list = new List<ITree>();
		for (t = t.Parent; t != null; t = t.Parent)
		{
			list.Insert(0, t);
		}
		return list;
	}

	public static ICollection<IParseTree> FindAllTokenNodes(IParseTree t, int ttype)
	{
		return FindAllNodes(t, ttype, findTokens: true);
	}

	public static ICollection<IParseTree> FindAllRuleNodes(IParseTree t, int ruleIndex)
	{
		return FindAllNodes(t, ruleIndex, findTokens: false);
	}

	public static IList<IParseTree> FindAllNodes(IParseTree t, int index, bool findTokens)
	{
		IList<IParseTree> list = new List<IParseTree>();
		_findAllNodes(t, index, findTokens, list);
		return list;
	}

	private static void _findAllNodes(IParseTree t, int index, bool findTokens, IList<IParseTree> nodes)
	{
		if (findTokens && t is ITerminalNode)
		{
			ITerminalNode terminalNode = (ITerminalNode)t;
			if (terminalNode.Symbol.Type == index)
			{
				nodes.Add(t);
			}
		}
		else if (!findTokens && t is ParserRuleContext)
		{
			ParserRuleContext parserRuleContext = (ParserRuleContext)t;
			if (parserRuleContext.RuleIndex == index)
			{
				nodes.Add(t);
			}
		}
		for (int i = 0; i < t.ChildCount; i++)
		{
			_findAllNodes(t.GetChild(i), index, findTokens, nodes);
		}
	}

	public static IList<IParseTree> Descendants(IParseTree t)
	{
		List<IParseTree> list = new List<IParseTree>();
		list.Add(t);
		int childCount = t.ChildCount;
		for (int i = 0; i < childCount; i++)
		{
			list.AddRange(Descendants(t.GetChild(i)));
		}
		return list;
	}

	private Trees()
	{
	}
}
