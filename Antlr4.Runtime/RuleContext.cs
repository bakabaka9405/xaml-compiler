using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree;

namespace Antlr4.Runtime;

public class RuleContext : IRuleNode, IParseTree, ISyntaxTree, ITree
{
	private RuleContext _parent;

	public int invokingState = -1;

	public virtual bool IsEmpty => invokingState == -1;

	public virtual Interval SourceInterval => Interval.Invalid;

	RuleContext IRuleNode.RuleContext => this;

	public virtual RuleContext Parent
	{
		get
		{
			return _parent;
		}
		set
		{
			_parent = value;
		}
	}

	IRuleNode IRuleNode.Parent => Parent;

	IParseTree IParseTree.Parent => Parent;

	ITree ITree.Parent => Parent;

	public virtual RuleContext Payload => this;

	object ITree.Payload => Payload;

	public virtual int RuleIndex => -1;

	public virtual int ChildCount => 0;

	public RuleContext()
	{
	}

	public RuleContext(RuleContext parent, int invokingState)
	{
		_parent = parent;
		this.invokingState = invokingState;
	}

	public static RuleContext GetChildContext(RuleContext parent, int invokingState)
	{
		return new RuleContext(parent, invokingState);
	}

	public virtual int Depth()
	{
		int num = 0;
		RuleContext ruleContext = this;
		while (ruleContext != null)
		{
			ruleContext = ruleContext._parent;
			num++;
		}
		return num;
	}

	public virtual string GetText()
	{
		if (ChildCount == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < ChildCount; i++)
		{
			stringBuilder.Append(GetChild(i).GetText());
		}
		return stringBuilder.ToString();
	}

	public virtual IParseTree GetChild(int i)
	{
		return null;
	}

	ITree ITree.GetChild(int i)
	{
		return GetChild(i);
	}

	public virtual T Accept<T>(IParseTreeVisitor<T> visitor)
	{
		return visitor.VisitChildren(this);
	}

	public virtual string ToStringTree(Parser recog)
	{
		return Trees.ToStringTree(this, recog);
	}

	public virtual string ToStringTree(IList<string> ruleNames)
	{
		return Trees.ToStringTree(this, ruleNames);
	}

	public virtual string ToStringTree()
	{
		return ToStringTree((IList<string>)null);
	}

	public override string ToString()
	{
		return ToString((IList<string>)null, (RuleContext)null);
	}

	public string ToString(IRecognizer recog)
	{
		return ToString(recog, ParserRuleContext.EmptyContext);
	}

	public string ToString(IList<string> ruleNames)
	{
		return ToString(ruleNames, null);
	}

	public virtual string ToString(IRecognizer recog, RuleContext stop)
	{
		string[] array = recog?.RuleNames;
		IList<string> list = ((array != null) ? Arrays.AsList(array) : null);
		return ToString(list, stop);
	}

	public virtual string ToString(IList<string> ruleNames, RuleContext stop)
	{
		StringBuilder stringBuilder = new StringBuilder();
		RuleContext ruleContext = this;
		stringBuilder.Append("[");
		while (ruleContext != null && ruleContext != stop)
		{
			if (ruleNames == null)
			{
				if (!ruleContext.IsEmpty)
				{
					stringBuilder.Append(ruleContext.invokingState);
				}
			}
			else
			{
				int ruleIndex = ruleContext.RuleIndex;
				string value = ((ruleIndex >= 0 && ruleIndex < ruleNames.Count) ? ruleNames[ruleIndex] : ruleIndex.ToString());
				stringBuilder.Append(value);
			}
			if (ruleContext.Parent != null && (ruleNames != null || !ruleContext.Parent.IsEmpty))
			{
				stringBuilder.Append(" ");
			}
			ruleContext = ruleContext.Parent;
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}
}
