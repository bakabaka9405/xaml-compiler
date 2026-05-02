using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree.Xpath;

public class XPath
{
	private sealed class _XPathLexer_87 : XPathLexer
	{
		public _XPathLexer_87(ICharStream baseArg1)
			: base(baseArg1)
		{
		}

		public override void Recover(LexerNoViableAltException e)
		{
			throw e;
		}
	}

	public const string Wildcard = "*";

	public const string Not = "!";

	protected internal string path;

	protected internal XPathElement[] elements;

	protected internal Parser parser;

	public XPath(Parser parser, string path)
	{
		this.parser = parser;
		this.path = path;
		elements = Split(path);
	}

	public virtual XPathElement[] Split(string path)
	{
		AntlrInputStream baseArg;
		try
		{
			baseArg = new AntlrInputStream(new StringReader(path));
		}
		catch (IOException innerException)
		{
			throw new ArgumentException("Could not read path: " + path, innerException);
		}
		XPathLexer xPathLexer = new _XPathLexer_87(baseArg);
		xPathLexer.RemoveErrorListeners();
		xPathLexer.AddErrorListener(new XPathLexerErrorListener());
		CommonTokenStream commonTokenStream = new CommonTokenStream(xPathLexer);
		try
		{
			commonTokenStream.Fill();
		}
		catch (LexerNoViableAltException innerException2)
		{
			int column = xPathLexer.Column;
			string message = "Invalid tokens or characters at index " + column + " in path '" + path + "'";
			throw new ArgumentException(message, innerException2);
		}
		IList<IToken> tokens = commonTokenStream.GetTokens();
		IList<XPathElement> list = new List<XPathElement>();
		int count = tokens.Count;
		int num = 0;
		while (num < count)
		{
			IToken token = tokens[num];
			IToken token2 = null;
			switch (token.Type)
			{
			case 3:
			case 4:
			{
				bool anywhere = token.Type == 3;
				num++;
				token2 = tokens[num];
				bool flag = token2.Type == 6;
				if (flag)
				{
					num++;
					token2 = tokens[num];
				}
				XPathElement xPathElement = GetXPathElement(token2, anywhere);
				xPathElement.invert = flag;
				list.Add(xPathElement);
				num++;
				continue;
			}
			case 1:
			case 2:
			case 5:
				list.Add(GetXPathElement(token, anywhere: false));
				num++;
				continue;
			default:
				throw new ArgumentException("Unknowth path element " + token);
			case -1:
				break;
			}
			break;
		}
		return list.ToArray();
	}

	protected internal virtual XPathElement GetXPathElement(IToken wordToken, bool anywhere)
	{
		if (wordToken.Type == -1)
		{
			throw new ArgumentException("Missing path element at end of path");
		}
		string text = wordToken.Text;
		int tokenType = parser.GetTokenType(text);
		int ruleIndex = parser.GetRuleIndex(text);
		switch (wordToken.Type)
		{
		case 5:
			if (!anywhere)
			{
				return new XPathWildcardElement();
			}
			return new XPathWildcardAnywhereElement();
		case 1:
		case 8:
			if (tokenType == 0)
			{
				throw new ArgumentException(text + " at index " + wordToken.StartIndex + " isn't a valid token name");
			}
			if (!anywhere)
			{
				return new XPathTokenElement(text, tokenType);
			}
			return new XPathTokenAnywhereElement(text, tokenType);
		default:
			if (ruleIndex == -1)
			{
				throw new ArgumentException(text + " at index " + wordToken.StartIndex + " isn't a valid rule name");
			}
			if (!anywhere)
			{
				return new XPathRuleElement(text, ruleIndex);
			}
			return new XPathRuleAnywhereElement(text, ruleIndex);
		}
	}

	public static ICollection<IParseTree> FindAll(IParseTree tree, string xpath, Parser parser)
	{
		XPath xPath = new XPath(parser, xpath);
		return xPath.Evaluate(tree);
	}

	public virtual ICollection<IParseTree> Evaluate(IParseTree t)
	{
		ParserRuleContext parserRuleContext = new ParserRuleContext();
		parserRuleContext.children = Collections.SingletonList(t);
		ICollection<IParseTree> collection = new ParserRuleContext[1] { parserRuleContext };
		int num = 0;
		while (num < elements.Length)
		{
			HashSet<IParseTree> hashSet = new HashSet<IParseTree>();
			ICollection<IParseTree> collection2 = new List<IParseTree>();
			foreach (IParseTree item in collection)
			{
				if (item.ChildCount <= 0)
				{
					continue;
				}
				ICollection<IParseTree> collection3 = elements[num].Evaluate(item);
				foreach (IParseTree item2 in collection3)
				{
					if (hashSet.Add(item2))
					{
						collection2.Add(item2);
					}
				}
			}
			num++;
			collection = collection2;
		}
		return collection;
	}
}
