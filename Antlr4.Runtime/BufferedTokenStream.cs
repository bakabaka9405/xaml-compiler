using System;
using System.Collections.Generic;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime;

public class BufferedTokenStream : ITokenStream, IIntStream
{
	[NotNull]
	private ITokenSource _tokenSource;

	protected internal IList<IToken> tokens = new List<IToken>(100);

	protected internal int p = -1;

	protected internal bool fetchedEOF;

	public virtual ITokenSource TokenSource => _tokenSource;

	public virtual int Index => p;

	public virtual int Size => tokens.Count;

	public virtual string SourceName => _tokenSource.SourceName;

	public BufferedTokenStream(ITokenSource tokenSource)
	{
		if (tokenSource == null)
		{
			throw new ArgumentNullException("tokenSource cannot be null");
		}
		_tokenSource = tokenSource;
	}

	public virtual int Mark()
	{
		return 0;
	}

	public virtual void Release(int marker)
	{
	}

	public virtual void Reset()
	{
		Seek(0);
	}

	public virtual void Seek(int index)
	{
		LazyInit();
		p = AdjustSeekIndex(index);
	}

	public virtual void Consume()
	{
		bool flag = p >= 0 && ((!fetchedEOF) ? (p < tokens.Count) : (p < tokens.Count - 1));
		if (!flag && La(1) == -1)
		{
			throw new InvalidOperationException("cannot consume EOF");
		}
		if (Sync(p + 1))
		{
			p = AdjustSeekIndex(p + 1);
		}
	}

	protected internal virtual bool Sync(int i)
	{
		int num = i - tokens.Count + 1;
		if (num > 0)
		{
			int num2 = Fetch(num);
			return num2 >= num;
		}
		return true;
	}

	protected internal virtual int Fetch(int n)
	{
		if (fetchedEOF)
		{
			return 0;
		}
		for (int i = 0; i < n; i++)
		{
			IToken token = _tokenSource.NextToken();
			if (token is IWritableToken)
			{
				((IWritableToken)token).TokenIndex = tokens.Count;
			}
			tokens.Add(token);
			if (token.Type == -1)
			{
				fetchedEOF = true;
				return i + 1;
			}
		}
		return n;
	}

	public virtual IToken Get(int i)
	{
		if (i < 0 || i >= tokens.Count)
		{
			throw new ArgumentOutOfRangeException("token index " + i + " out of range 0.." + (tokens.Count - 1));
		}
		return tokens[i];
	}

	public virtual IList<IToken> Get(int start, int stop)
	{
		if (start < 0 || stop < 0)
		{
			return null;
		}
		LazyInit();
		IList<IToken> list = new List<IToken>();
		if (stop >= tokens.Count)
		{
			stop = tokens.Count - 1;
		}
		for (int i = start; i <= stop; i++)
		{
			IToken token = tokens[i];
			if (token.Type == -1)
			{
				break;
			}
			list.Add(token);
		}
		return list;
	}

	public virtual int La(int i)
	{
		return Lt(i).Type;
	}

	protected internal virtual IToken Lb(int k)
	{
		if (p - k < 0)
		{
			return null;
		}
		return tokens[p - k];
	}

	[return: NotNull]
	public virtual IToken Lt(int k)
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
		int num = p + k - 1;
		Sync(num);
		if (num >= tokens.Count)
		{
			return tokens[tokens.Count - 1];
		}
		return tokens[num];
	}

	protected internal virtual int AdjustSeekIndex(int i)
	{
		return i;
	}

	protected internal void LazyInit()
	{
		if (p == -1)
		{
			Setup();
		}
	}

	protected internal virtual void Setup()
	{
		Sync(0);
		p = AdjustSeekIndex(0);
	}

	public virtual void SetTokenSource(ITokenSource tokenSource)
	{
		_tokenSource = tokenSource;
		tokens.Clear();
		p = -1;
	}

	public virtual IList<IToken> GetTokens()
	{
		return tokens;
	}

	public virtual IList<IToken> GetTokens(int start, int stop)
	{
		return GetTokens(start, stop, null);
	}

	public virtual IList<IToken> GetTokens(int start, int stop, BitSet types)
	{
		LazyInit();
		if (start < 0 || stop >= tokens.Count || stop < 0 || start >= tokens.Count)
		{
			throw new ArgumentOutOfRangeException("start " + start + " or stop " + stop + " not in 0.." + (tokens.Count - 1));
		}
		if (start > stop)
		{
			return null;
		}
		IList<IToken> list = new List<IToken>();
		for (int i = start; i <= stop; i++)
		{
			IToken token = tokens[i];
			if (types == null || types.Get(token.Type))
			{
				list.Add(token);
			}
		}
		if (list.Count == 0)
		{
			list = null;
		}
		return list;
	}

	public virtual IList<IToken> GetTokens(int start, int stop, int ttype)
	{
		BitSet bitSet = new BitSet(ttype);
		bitSet.Set(ttype);
		return GetTokens(start, stop, bitSet);
	}

	protected internal virtual int NextTokenOnChannel(int i, int channel)
	{
		Sync(i);
		if (i >= Size)
		{
			return Size - 1;
		}
		IToken token = tokens[i];
		while (token.Channel != channel)
		{
			if (token.Type == -1)
			{
				return i;
			}
			i++;
			Sync(i);
			token = tokens[i];
		}
		return i;
	}

	protected internal virtual int PreviousTokenOnChannel(int i, int channel)
	{
		Sync(i);
		if (i >= Size)
		{
			return Size - 1;
		}
		while (i >= 0)
		{
			IToken token = tokens[i];
			if (token.Type == -1 || token.Channel == channel)
			{
				return i;
			}
			i--;
		}
		return i;
	}

	public virtual IList<IToken> GetHiddenTokensToRight(int tokenIndex, int channel)
	{
		LazyInit();
		if (tokenIndex < 0 || tokenIndex >= tokens.Count)
		{
			throw new ArgumentOutOfRangeException(tokenIndex + " not in 0.." + (tokens.Count - 1));
		}
		int num = NextTokenOnChannel(tokenIndex + 1, 0);
		int num2 = tokenIndex + 1;
		int to = ((num != -1) ? num : (Size - 1));
		return FilterForChannel(num2, to, channel);
	}

	public virtual IList<IToken> GetHiddenTokensToRight(int tokenIndex)
	{
		return GetHiddenTokensToRight(tokenIndex, -1);
	}

	public virtual IList<IToken> GetHiddenTokensToLeft(int tokenIndex, int channel)
	{
		LazyInit();
		if (tokenIndex < 0 || tokenIndex >= tokens.Count)
		{
			throw new ArgumentOutOfRangeException(tokenIndex + " not in 0.." + (tokens.Count - 1));
		}
		if (tokenIndex == 0)
		{
			return null;
		}
		int num = PreviousTokenOnChannel(tokenIndex - 1, 0);
		if (num == tokenIndex - 1)
		{
			return null;
		}
		int num2 = num + 1;
		int to = tokenIndex - 1;
		return FilterForChannel(num2, to, channel);
	}

	public virtual IList<IToken> GetHiddenTokensToLeft(int tokenIndex)
	{
		return GetHiddenTokensToLeft(tokenIndex, -1);
	}

	protected internal virtual IList<IToken> FilterForChannel(int from, int to, int channel)
	{
		IList<IToken> list = new List<IToken>();
		for (int i = from; i <= to; i++)
		{
			IToken token = tokens[i];
			if (channel == -1)
			{
				if (token.Channel != 0)
				{
					list.Add(token);
				}
			}
			else if (token.Channel == channel)
			{
				list.Add(token);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		return list;
	}

	[return: NotNull]
	public virtual string GetText()
	{
		Fill();
		return GetText(Interval.Of(0, Size - 1));
	}

	[return: NotNull]
	public virtual string GetText(Interval interval)
	{
		int a = interval.a;
		int num = interval.b;
		if (a < 0 || num < 0)
		{
			return string.Empty;
		}
		LazyInit();
		if (num >= tokens.Count)
		{
			num = tokens.Count - 1;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = a; i <= num; i++)
		{
			IToken token = tokens[i];
			if (token.Type == -1)
			{
				break;
			}
			stringBuilder.Append(token.Text);
		}
		return stringBuilder.ToString();
	}

	[return: NotNull]
	public virtual string GetText(RuleContext ctx)
	{
		return GetText(ctx.SourceInterval);
	}

	[return: NotNull]
	public virtual string GetText(IToken start, IToken stop)
	{
		if (start != null && stop != null)
		{
			return GetText(Interval.Of(start.TokenIndex, stop.TokenIndex));
		}
		return string.Empty;
	}

	public virtual void Fill()
	{
		LazyInit();
		int num = 1000;
		int num2;
		do
		{
			num2 = Fetch(num);
		}
		while (num2 >= num);
	}
}
