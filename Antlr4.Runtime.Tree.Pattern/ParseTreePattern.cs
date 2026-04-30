using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree.Xpath;

namespace Antlr4.Runtime.Tree.Pattern;

public class ParseTreePattern
{
	private readonly int patternRuleIndex;

	[NotNull]
	private readonly string pattern;

	[NotNull]
	private readonly IParseTree patternTree;

	[NotNull]
	private readonly ParseTreePatternMatcher matcher;

	[NotNull]
	public virtual ParseTreePatternMatcher Matcher => matcher;

	[NotNull]
	public virtual string Pattern => pattern;

	public virtual int PatternRuleIndex => patternRuleIndex;

	[NotNull]
	public virtual IParseTree PatternTree => patternTree;

	public ParseTreePattern(ParseTreePatternMatcher matcher, string pattern, int patternRuleIndex, IParseTree patternTree)
	{
		this.matcher = matcher;
		this.patternRuleIndex = patternRuleIndex;
		this.pattern = pattern;
		this.patternTree = patternTree;
	}

	[return: NotNull]
	public virtual ParseTreeMatch Match(IParseTree tree)
	{
		return matcher.Match(tree, this);
	}

	public virtual bool Matches(IParseTree tree)
	{
		return matcher.Match(tree, this).Succeeded;
	}

	[return: NotNull]
	public virtual IList<ParseTreeMatch> FindAll(IParseTree tree, string xpath)
	{
		ICollection<IParseTree> collection = XPath.FindAll(tree, xpath, matcher.Parser);
		IList<ParseTreeMatch> list = new List<ParseTreeMatch>();
		foreach (IParseTree item in collection)
		{
			ParseTreeMatch parseTreeMatch = Match(item);
			if (parseTreeMatch.Succeeded)
			{
				list.Add(parseTreeMatch);
			}
		}
		return list;
	}
}
