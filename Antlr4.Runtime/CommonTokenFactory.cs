using System;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public class CommonTokenFactory : ITokenFactory
{
	public static readonly ITokenFactory Default = new CommonTokenFactory();

	protected internal readonly bool copyText;

	public CommonTokenFactory(bool copyText)
	{
		this.copyText = copyText;
	}

	public CommonTokenFactory()
		: this(copyText: false)
	{
	}

	public virtual CommonToken Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start, int stop, int line, int charPositionInLine)
	{
		CommonToken commonToken = new CommonToken(source, type, channel, start, stop);
		commonToken.Line = line;
		commonToken.Column = charPositionInLine;
		if (text != null)
		{
			commonToken.Text = text;
		}
		else if (copyText && source.Item2 != null)
		{
			commonToken.Text = source.Item2.GetText(Interval.Of(start, stop));
		}
		return commonToken;
	}

	IToken ITokenFactory.Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start, int stop, int line, int charPositionInLine)
	{
		return Create(source, type, text, channel, start, stop, line, charPositionInLine);
	}

	public virtual CommonToken Create(int type, string text)
	{
		return new CommonToken(type, text);
	}

	IToken ITokenFactory.Create(int type, string text)
	{
		return Create(type, text);
	}
}
