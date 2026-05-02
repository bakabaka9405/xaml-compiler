using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Tree;

public class TerminalNodeImpl : ITerminalNode, IParseTree, ISyntaxTree, ITree
{
	private IToken _symbol;

	private IRuleNode _parent;

	public virtual IToken Symbol => _symbol;

	public virtual IRuleNode Parent
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

	IParseTree IParseTree.Parent => Parent;

	ITree ITree.Parent => Parent;

	public virtual IToken Payload => Symbol;

	object ITree.Payload => Payload;

	public virtual Interval SourceInterval
	{
		get
		{
			if (Symbol != null)
			{
				int tokenIndex = Symbol.TokenIndex;
				return new Interval(tokenIndex, tokenIndex);
			}
			return Interval.Invalid;
		}
	}

	public virtual int ChildCount => 0;

	public TerminalNodeImpl(IToken symbol)
	{
		_symbol = symbol;
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
		return visitor.VisitTerminal(this);
	}

	public virtual string GetText()
	{
		if (Symbol != null)
		{
			return Symbol.Text;
		}
		return null;
	}

	public virtual string ToStringTree(Parser parser)
	{
		return ToString();
	}

	public override string ToString()
	{
		if (Symbol != null)
		{
			if (Symbol.Type == -1)
			{
				return "<EOF>";
			}
			return Symbol.Text;
		}
		return "<null>";
	}

	public virtual string ToStringTree()
	{
		return ToString();
	}
}
