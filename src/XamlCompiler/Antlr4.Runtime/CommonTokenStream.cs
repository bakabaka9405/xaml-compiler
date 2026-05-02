namespace Antlr4.Runtime;

public class CommonTokenStream : BufferedTokenStream
{
	protected internal int channel;

	public CommonTokenStream(ITokenSource tokenSource)
		: base(tokenSource)
	{
	}

	public CommonTokenStream(ITokenSource tokenSource, int channel)
		: this(tokenSource)
	{
		this.channel = channel;
	}

	protected internal override int AdjustSeekIndex(int i)
	{
		return NextTokenOnChannel(i, channel);
	}

	protected internal override IToken Lb(int k)
	{
		if (k == 0 || p - k < 0)
		{
			return null;
		}
		int num = p;
		for (int i = 1; i <= k; i++)
		{
			num = PreviousTokenOnChannel(num - 1, channel);
		}
		if (num < 0)
		{
			return null;
		}
		return tokens[num];
	}

	public override IToken Lt(int k)
	{
		LazyInit();
		if (k == 0)
		{
			return null;
		}
		if (k < 0)
		{
			return Lb(-k);
		}
		int num = p;
		for (int i = 1; i < k; i++)
		{
			if (Sync(num + 1))
			{
				num = NextTokenOnChannel(num + 1, channel);
			}
		}
		return tokens[num];
	}

	public virtual int GetNumberOfOnChannelTokens()
	{
		int num = 0;
		Fill();
		for (int i = 0; i < tokens.Count; i++)
		{
			IToken token = tokens[i];
			if (token.Channel == channel)
			{
				num++;
			}
			if (token.Type == -1)
			{
				break;
			}
		}
		return num;
	}
}
