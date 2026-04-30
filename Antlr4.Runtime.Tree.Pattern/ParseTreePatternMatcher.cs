using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree.Pattern;

public class ParseTreePatternMatcher
{
	[Serializable]
	public class CannotInvokeStartRule : Exception
	{
		public CannotInvokeStartRule(Exception e)
			: base(e.Message, e)
		{
		}
	}

	[Serializable]
	public class StartRuleDoesNotConsumeFullPattern : Exception
	{
	}

	private readonly Lexer lexer;

	private readonly Parser parser;

	protected internal string start = "<";

	protected internal string stop = ">";

	protected internal string escape = "\\";

	[NotNull]
	public virtual Lexer Lexer => lexer;

	[NotNull]
	public virtual Parser Parser => parser;

	public ParseTreePatternMatcher(Lexer lexer, Parser parser)
	{
		this.lexer = lexer;
		this.parser = parser;
	}

	public virtual void SetDelimiters(string start, string stop, string escapeLeft)
	{
		if (string.IsNullOrEmpty(start))
		{
			throw new ArgumentException("start cannot be null or empty");
		}
		if (string.IsNullOrEmpty(stop))
		{
			throw new ArgumentException("stop cannot be null or empty");
		}
		this.start = start;
		this.stop = stop;
		escape = escapeLeft;
	}

	public virtual bool Matches(IParseTree tree, string pattern, int patternRuleIndex)
	{
		ParseTreePattern pattern2 = Compile(pattern, patternRuleIndex);
		return Matches(tree, pattern2);
	}

	public virtual bool Matches(IParseTree tree, ParseTreePattern pattern)
	{
		MultiMap<string, IParseTree> labels = new MultiMap<string, IParseTree>();
		IParseTree parseTree = MatchImpl(tree, pattern.PatternTree, labels);
		return parseTree == null;
	}

	public virtual ParseTreeMatch Match(IParseTree tree, string pattern, int patternRuleIndex)
	{
		ParseTreePattern pattern2 = Compile(pattern, patternRuleIndex);
		return Match(tree, pattern2);
	}

	[return: NotNull]
	public virtual ParseTreeMatch Match(IParseTree tree, ParseTreePattern pattern)
	{
		MultiMap<string, IParseTree> labels = new MultiMap<string, IParseTree>();
		IParseTree mismatchedNode = MatchImpl(tree, pattern.PatternTree, labels);
		return new ParseTreeMatch(tree, pattern, labels, mismatchedNode);
	}

	public virtual ParseTreePattern Compile(string pattern, int patternRuleIndex)
	{
		IList<IToken> tokens = Tokenize(pattern);
		ListTokenSource tokenSource = new ListTokenSource(tokens);
		CommonTokenStream commonTokenStream = new CommonTokenStream(tokenSource);
		ParserInterpreter parserInterpreter = new ParserInterpreter(parser.GrammarFileName, parser.Vocabulary, Arrays.AsList(parser.RuleNames), parser.GetATNWithBypassAlts(), commonTokenStream);
		IParseTree parseTree = null;
		try
		{
			parserInterpreter.ErrorHandler = new BailErrorStrategy();
			parseTree = parserInterpreter.Parse(patternRuleIndex);
		}
		catch (ParseCanceledException ex)
		{
			throw (RecognitionException)ex.InnerException;
		}
		catch (RecognitionException)
		{
			throw;
		}
		catch (Exception e)
		{
			throw new CannotInvokeStartRule(e);
		}
		if (commonTokenStream.La(1) != -1)
		{
			throw new StartRuleDoesNotConsumeFullPattern();
		}
		return new ParseTreePattern(this, pattern, patternRuleIndex, parseTree);
	}

	[return: Nullable]
	protected internal virtual IParseTree MatchImpl(IParseTree tree, IParseTree patternTree, MultiMap<string, IParseTree> labels)
	{
		if (tree == null)
		{
			throw new ArgumentException("tree cannot be null");
		}
		if (patternTree == null)
		{
			throw new ArgumentException("patternTree cannot be null");
		}
		if (tree is ITerminalNode && patternTree is ITerminalNode)
		{
			ITerminalNode terminalNode = (ITerminalNode)tree;
			ITerminalNode terminalNode2 = (ITerminalNode)patternTree;
			IParseTree parseTree = null;
			if (terminalNode.Symbol.Type == terminalNode2.Symbol.Type)
			{
				if (terminalNode2.Symbol is TokenTagToken)
				{
					TokenTagToken tokenTagToken = (TokenTagToken)terminalNode2.Symbol;
					labels.Map(tokenTagToken.TokenName, tree);
					if (tokenTagToken.Label != null)
					{
						labels.Map(tokenTagToken.Label, tree);
					}
				}
				else if (!terminalNode.GetText().Equals(terminalNode2.GetText(), StringComparison.Ordinal) && parseTree == null)
				{
					parseTree = terminalNode;
				}
			}
			else if (parseTree == null)
			{
				parseTree = terminalNode;
			}
			return parseTree;
		}
		if (tree is ParserRuleContext && patternTree is ParserRuleContext)
		{
			ParserRuleContext parserRuleContext = (ParserRuleContext)tree;
			ParserRuleContext parserRuleContext2 = (ParserRuleContext)patternTree;
			IParseTree parseTree2 = null;
			RuleTagToken ruleTagToken = GetRuleTagToken(parserRuleContext2);
			if (ruleTagToken != null)
			{
				if (parserRuleContext.RuleIndex == parserRuleContext2.RuleIndex)
				{
					labels.Map(ruleTagToken.RuleName, tree);
					if (ruleTagToken.Label != null)
					{
						labels.Map(ruleTagToken.Label, tree);
					}
				}
				else if (parseTree2 == null)
				{
					parseTree2 = parserRuleContext;
				}
				return parseTree2;
			}
			if (parserRuleContext.ChildCount != parserRuleContext2.ChildCount)
			{
				if (parseTree2 == null)
				{
					parseTree2 = parserRuleContext;
				}
				return parseTree2;
			}
			int childCount = parserRuleContext.ChildCount;
			for (int i = 0; i < childCount; i++)
			{
				IParseTree parseTree3 = MatchImpl(parserRuleContext.GetChild(i), patternTree.GetChild(i), labels);
				if (parseTree3 != null)
				{
					return parseTree3;
				}
			}
			return parseTree2;
		}
		return tree;
	}

	protected internal virtual RuleTagToken GetRuleTagToken(IParseTree t)
	{
		if (t is IRuleNode)
		{
			IRuleNode ruleNode = (IRuleNode)t;
			if (ruleNode.ChildCount == 1 && ruleNode.GetChild(0) is ITerminalNode)
			{
				ITerminalNode terminalNode = (ITerminalNode)ruleNode.GetChild(0);
				if (terminalNode.Symbol is RuleTagToken)
				{
					return (RuleTagToken)terminalNode.Symbol;
				}
			}
		}
		return null;
	}

	public virtual IList<IToken> Tokenize(string pattern)
	{
		IList<Chunk> list = Split(pattern);
		IList<IToken> list2 = new List<IToken>();
		foreach (Chunk item2 in list)
		{
			if (item2 is TagChunk)
			{
				TagChunk tagChunk = (TagChunk)item2;
				if (char.IsUpper(tagChunk.Tag[0]))
				{
					int tokenType = parser.GetTokenType(tagChunk.Tag);
					if (tokenType == 0)
					{
						throw new ArgumentException("Unknown token " + tagChunk.Tag + " in pattern: " + pattern);
					}
					TokenTagToken item = new TokenTagToken(tagChunk.Tag, tokenType, tagChunk.Label);
					list2.Add(item);
					continue;
				}
				if (!char.IsLower(tagChunk.Tag[0]))
				{
					throw new ArgumentException("invalid tag: " + tagChunk.Tag + " in pattern: " + pattern);
				}
				int ruleIndex = parser.GetRuleIndex(tagChunk.Tag);
				if (ruleIndex == -1)
				{
					throw new ArgumentException("Unknown rule " + tagChunk.Tag + " in pattern: " + pattern);
				}
				int bypassTokenType = parser.GetATNWithBypassAlts().ruleToTokenType[ruleIndex];
				list2.Add(new RuleTagToken(tagChunk.Tag, bypassTokenType, tagChunk.Label));
			}
			else
			{
				TextChunk textChunk = (TextChunk)item2;
				AntlrInputStream inputStream = new AntlrInputStream(textChunk.Text);
				lexer.SetInputStream(inputStream);
				IToken token = lexer.NextToken();
				while (token.Type != -1)
				{
					list2.Add(token);
					token = lexer.NextToken();
				}
			}
		}
		return list2;
	}

	internal virtual IList<Chunk> Split(string pattern)
	{
		int num = 0;
		int length = pattern.Length;
		IList<Chunk> list = new List<Chunk>();
		IList<int> list2 = new List<int>();
		IList<int> list3 = new List<int>();
		while (num < length)
		{
			if (num == pattern.IndexOf(escape + start, num))
			{
				num += escape.Length + start.Length;
			}
			else if (num == pattern.IndexOf(escape + stop, num))
			{
				num += escape.Length + stop.Length;
			}
			else if (num == pattern.IndexOf(start, num))
			{
				list2.Add(num);
				num += start.Length;
			}
			else if (num == pattern.IndexOf(stop, num))
			{
				list3.Add(num);
				num += stop.Length;
			}
			else
			{
				num++;
			}
		}
		if (list2.Count > list3.Count)
		{
			throw new ArgumentException("unterminated tag in pattern: " + pattern);
		}
		if (list2.Count < list3.Count)
		{
			throw new ArgumentException("missing start tag in pattern: " + pattern);
		}
		int count = list2.Count;
		for (int i = 0; i < count; i++)
		{
			if (list2[i] >= list3[i])
			{
				throw new ArgumentException("tag delimiters out of order in pattern: " + pattern);
			}
		}
		if (count == 0)
		{
			string text = Antlr4.Runtime.Sharpen.Runtime.Substring(pattern, 0, length);
			list.Add(new TextChunk(text));
		}
		if (count > 0 && list2[0] > 0)
		{
			string text2 = Antlr4.Runtime.Sharpen.Runtime.Substring(pattern, 0, list2[0]);
			list.Add(new TextChunk(text2));
		}
		for (int j = 0; j < count; j++)
		{
			string text3 = Antlr4.Runtime.Sharpen.Runtime.Substring(pattern, list2[j] + start.Length, list3[j]);
			string tag = text3;
			string label = null;
			int num2 = text3.IndexOf(':');
			if (num2 >= 0)
			{
				label = Antlr4.Runtime.Sharpen.Runtime.Substring(text3, 0, num2);
				tag = Antlr4.Runtime.Sharpen.Runtime.Substring(text3, num2 + 1, text3.Length);
			}
			list.Add(new TagChunk(label, tag));
			if (j + 1 < count)
			{
				string text4 = Antlr4.Runtime.Sharpen.Runtime.Substring(pattern, list3[j] + stop.Length, list2[j + 1]);
				list.Add(new TextChunk(text4));
			}
		}
		if (count > 0)
		{
			int num3 = list3[count - 1] + stop.Length;
			if (num3 < length)
			{
				string text5 = Antlr4.Runtime.Sharpen.Runtime.Substring(pattern, num3, length);
				list.Add(new TextChunk(text5));
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			Chunk chunk = list[k];
			if (chunk is TextChunk)
			{
				TextChunk textChunk = (TextChunk)chunk;
				string text6 = textChunk.Text.Replace(escape, string.Empty);
				if (text6.Length < textChunk.Text.Length)
				{
					list.Set(k, new TextChunk(text6));
				}
			}
		}
		return list;
	}
}
