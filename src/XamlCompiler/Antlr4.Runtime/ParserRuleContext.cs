using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;
using Antlr4.Runtime.Tree;

namespace Antlr4.Runtime;

public class ParserRuleContext : RuleContext
{
	private static readonly ParserRuleContext Empty = new ParserRuleContext();

	public IList<IParseTree> children;

	private IToken _start;

	private IToken _stop;

	public RecognitionException exception;

	public static ParserRuleContext EmptyContext => Empty;

	public override int ChildCount
	{
		get
		{
			if (children == null)
			{
				return 0;
			}
			return children.Count;
		}
	}

	public override Interval SourceInterval
	{
		get
		{
			if (_start == null || _stop == null)
			{
				return Interval.Invalid;
			}
			return Interval.Of(_start.TokenIndex, _stop.TokenIndex);
		}
	}

	public virtual IToken Start
	{
		get
		{
			return _start;
		}
		set
		{
			_start = value;
		}
	}

	public virtual IToken Stop
	{
		get
		{
			return _stop;
		}
		set
		{
			_stop = value;
		}
	}

	public ParserRuleContext()
	{
	}

	public virtual void CopyFrom(ParserRuleContext ctx)
	{
		Parent = ctx.Parent;
		invokingState = ctx.invokingState;
		_start = ctx._start;
		_stop = ctx._stop;
	}

	public ParserRuleContext(ParserRuleContext parent, int invokingStateNumber)
		: base(parent, invokingStateNumber)
	{
	}

	public virtual void EnterRule(IParseTreeListener listener)
	{
	}

	public virtual void ExitRule(IParseTreeListener listener)
	{
	}

	public virtual void AddChild(ITerminalNode t)
	{
		if (children == null)
		{
			children = new List<IParseTree>();
		}
		children.Add(t);
	}

	public virtual void AddChild(RuleContext ruleInvocation)
	{
		if (children == null)
		{
			children = new List<IParseTree>();
		}
		children.Add(ruleInvocation);
	}

	public virtual void RemoveLastChild()
	{
		if (children != null)
		{
			children.RemoveAt(children.Count - 1);
		}
	}

	public virtual ITerminalNode AddChild(IToken matchedToken)
	{
		TerminalNodeImpl terminalNodeImpl = new TerminalNodeImpl(matchedToken);
		AddChild(terminalNodeImpl);
		terminalNodeImpl.Parent = this;
		return terminalNodeImpl;
	}

	public virtual IErrorNode AddErrorNode(IToken badToken)
	{
		ErrorNodeImpl errorNodeImpl = new ErrorNodeImpl(badToken);
		AddChild(errorNodeImpl);
		errorNodeImpl.Parent = this;
		return errorNodeImpl;
	}

	public override IParseTree GetChild(int i)
	{
		if (children == null || i < 0 || i >= children.Count)
		{
			return null;
		}
		return children[i];
	}

	public virtual T GetChild<T>(int i) where T : IParseTree
	{
		if (children == null || i < 0 || i >= children.Count)
		{
			return default(T);
		}
		int num = -1;
		foreach (IParseTree child in children)
		{
			if (child is T)
			{
				num++;
				if (num == i)
				{
					return (T)child;
				}
			}
		}
		return default(T);
	}

	public virtual ITerminalNode GetToken(int ttype, int i)
	{
		if (children == null || i < 0 || i >= children.Count)
		{
			return null;
		}
		int num = -1;
		foreach (IParseTree child in children)
		{
			if (!(child is ITerminalNode))
			{
				continue;
			}
			ITerminalNode terminalNode = (ITerminalNode)child;
			IToken symbol = terminalNode.Symbol;
			if (symbol.Type == ttype)
			{
				num++;
				if (num == i)
				{
					return terminalNode;
				}
			}
		}
		return null;
	}

	public virtual ITerminalNode[] GetTokens(int ttype)
	{
		if (children == null)
		{
			return Collections.EmptyList<ITerminalNode>();
		}
		List<ITerminalNode> list = null;
		foreach (IParseTree child in children)
		{
			if (!(child is ITerminalNode))
			{
				continue;
			}
			ITerminalNode terminalNode = (ITerminalNode)child;
			IToken symbol = terminalNode.Symbol;
			if (symbol.Type == ttype)
			{
				if (list == null)
				{
					list = new List<ITerminalNode>();
				}
				list.Add(terminalNode);
			}
		}
		if (list == null)
		{
			return Collections.EmptyList<ITerminalNode>();
		}
		return list.ToArray();
	}

	public virtual T GetRuleContext<T>(int i) where T : ParserRuleContext
	{
		return GetChild<T>(i);
	}

	public virtual T[] GetRuleContexts<T>() where T : ParserRuleContext
	{
		if (children == null)
		{
			return Collections.EmptyList<T>();
		}
		List<T> list = null;
		foreach (IParseTree child in children)
		{
			if (child is T)
			{
				if (list == null)
				{
					list = new List<T>();
				}
				list.Add((T)child);
			}
		}
		if (list == null)
		{
			return Collections.EmptyList<T>();
		}
		return list.ToArray();
	}

	public virtual string ToInfoString(Parser recognizer)
	{
		List<string> list = new List<string>(recognizer.GetRuleInvocationStack(this));
		list.Reverse();
		return "ParserRuleContext" + list?.ToString() + "{start=" + _start?.ToString() + ", stop=" + _stop?.ToString() + "}";
	}
}
