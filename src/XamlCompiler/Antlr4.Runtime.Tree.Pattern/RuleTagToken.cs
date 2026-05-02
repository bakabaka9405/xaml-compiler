using System;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Tree.Pattern;

public class RuleTagToken : IToken
{
	private readonly string ruleName;

	private readonly int bypassTokenType;

	private readonly string label;

	[NotNull]
	public string RuleName => ruleName;

	[Nullable]
	public string Label => label;

	public virtual int Channel => 0;

	public virtual string Text
	{
		get
		{
			if (label != null)
			{
				return "<" + label + ":" + ruleName + ">";
			}
			return "<" + ruleName + ">";
		}
	}

	public virtual int Type => bypassTokenType;

	public virtual int Line => 0;

	public virtual int Column => -1;

	public virtual int TokenIndex => -1;

	public virtual int StartIndex => -1;

	public virtual int StopIndex => -1;

	public virtual ITokenSource TokenSource => null;

	public virtual ICharStream InputStream => null;

	public RuleTagToken(string ruleName, int bypassTokenType)
		: this(ruleName, bypassTokenType, null)
	{
	}

	public RuleTagToken(string ruleName, int bypassTokenType, string label)
	{
		if (string.IsNullOrEmpty(ruleName))
		{
			throw new ArgumentException("ruleName cannot be null or empty.");
		}
		this.ruleName = ruleName;
		this.bypassTokenType = bypassTokenType;
		this.label = label;
	}

	public override string ToString()
	{
		return ruleName + ":" + bypassTokenType;
	}
}
