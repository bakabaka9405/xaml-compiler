using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime.Tree.Pattern;

public class ParseTreeMatch
{
	private readonly IParseTree tree;

	private readonly ParseTreePattern pattern;

	private readonly MultiMap<string, IParseTree> labels;

	private readonly IParseTree mismatchedNode;

	[NotNull]
	public virtual MultiMap<string, IParseTree> Labels => labels;

	[Nullable]
	public virtual IParseTree MismatchedNode => mismatchedNode;

	public virtual bool Succeeded => mismatchedNode == null;

	[NotNull]
	public virtual ParseTreePattern Pattern => pattern;

	[NotNull]
	public virtual IParseTree Tree => tree;

	public ParseTreeMatch(IParseTree tree, ParseTreePattern pattern, MultiMap<string, IParseTree> labels, IParseTree mismatchedNode)
	{
		if (tree == null)
		{
			throw new ArgumentException("tree cannot be null");
		}
		if (pattern == null)
		{
			throw new ArgumentException("pattern cannot be null");
		}
		if (labels == null)
		{
			throw new ArgumentException("labels cannot be null");
		}
		this.tree = tree;
		this.pattern = pattern;
		this.labels = labels;
		this.mismatchedNode = mismatchedNode;
	}

	[return: Nullable]
	public virtual IParseTree Get(string label)
	{
		IList<IParseTree> list = labels.Get(label);
		if (list == null || list.Count == 0)
		{
			return null;
		}
		return list[list.Count - 1];
	}

	[return: NotNull]
	public virtual IList<IParseTree> GetAll(string label)
	{
		IList<IParseTree> list = labels.Get(label);
		if (list == null)
		{
			return Collections.EmptyList<IParseTree>();
		}
		return list;
	}

	public override string ToString()
	{
		return string.Format("Match {0}; found {1} labels", Succeeded ? "succeeded" : "failed", Labels.Count);
	}
}
