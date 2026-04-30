using System;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Tree.Pattern;

[Serializable]
public class TokenTagToken : CommonToken
{
	[NotNull]
	private readonly string tokenName;

	[Nullable]
	private readonly string label;

	[NotNull]
	public string TokenName => tokenName;

	[Nullable]
	public string Label => label;

	public override string Text
	{
		get
		{
			if (label != null)
			{
				return "<" + label + ":" + tokenName + ">";
			}
			return "<" + tokenName + ">";
		}
	}

	public TokenTagToken(string tokenName, int type)
		: this(tokenName, type, null)
	{
	}

	public TokenTagToken(string tokenName, int type, string label)
		: base(type)
	{
		this.tokenName = tokenName;
		this.label = label;
	}

	public override string ToString()
	{
		return tokenName + ":" + Type;
	}
}
