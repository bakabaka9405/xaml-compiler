using System;
using System.Text;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime;

public class UnbufferedTokenStream : ITokenStream, IIntStream
{
	private ITokenSource _tokenSource;

	protected internal IToken[] tokens;

	protected internal int n;

	protected internal int p;

	protected internal int numMarkers;

	protected internal IToken lastToken;

	protected internal IToken lastTokenBufferStart;

	protected internal int currentTokenIndex;

	public virtual ITokenSource TokenSource
	{
		get
		{
			return _tokenSource;
		}
		set
		{
			_tokenSource = value;
		}
	}

	public virtual int Index => currentTokenIndex;

	public virtual int Size
	{
		get
		{
			throw new NotSupportedException("Unbuffered stream cannot know its size");
		}
	}

	public virtual string SourceName => TokenSource.SourceName;

	public UnbufferedTokenStream(ITokenSource tokenSource)
		: this(tokenSource, 256)
	{
	}

	public UnbufferedTokenStream(ITokenSource tokenSource, int bufferSize)
	{
		TokenSource = tokenSource;
		tokens = new IToken[bufferSize];
		n = 0;
		Fill(1);
	}

	public virtual IToken Get(int i)
	{
		int bufferStartIndex = GetBufferStartIndex();
		if (i < bufferStartIndex || i >= bufferStartIndex + n)
		{
			throw new ArgumentOutOfRangeException("get(" + i + ") outside buffer: " + bufferStartIndex + ".." + (bufferStartIndex + n));
		}
		return tokens[i - bufferStartIndex];
	}

	public virtual IToken Lt(int i)
	{
		if (i == -1)
		{
			return lastToken;
		}
		Sync(i);
		int num = p + i - 1;
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException("LT(" + i + ") gives negative index");
		}
		if (num >= n)
		{
			return tokens[n - 1];
		}
		return tokens[num];
	}

	public virtual int La(int i)
	{
		return Lt(i).Type;
	}

	[return: NotNull]
	public virtual string GetText()
	{
		return string.Empty;
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
		throw new NotSupportedException("The specified start and stop symbols are not supported.");
	}

	public virtual void Consume()
	{
		if (La(1) == -1)
		{
			throw new InvalidOperationException("cannot consume EOF");
		}
		lastToken = tokens[p];
		if (p == n - 1 && numMarkers == 0)
		{
			n = 0;
			p = -1;
			lastTokenBufferStart = lastToken;
		}
		p++;
		currentTokenIndex++;
		Sync(1);
	}

	protected internal virtual void Sync(int want)
	{
		int num = p + want - 1 - n + 1;
		if (num > 0)
		{
			Fill(num);
		}
	}

	protected internal virtual int Fill(int n)
	{
		for (int i = 0; i < n; i++)
		{
			if (this.n > 0 && tokens[this.n - 1].Type == -1)
			{
				return i;
			}
			IToken t = TokenSource.NextToken();
			Add(t);
		}
		return n;
	}

	protected internal virtual void Add(IToken t)
	{
		if (n >= tokens.Length)
		{
			tokens = Arrays.CopyOf(tokens, tokens.Length * 2);
		}
		if (t is IWritableToken)
		{
			((IWritableToken)t).TokenIndex = GetBufferStartIndex() + n;
		}
		tokens[n++] = t;
	}

	public virtual int Mark()
	{
		if (numMarkers == 0)
		{
			lastTokenBufferStart = lastToken;
		}
		int result = -numMarkers - 1;
		numMarkers++;
		return result;
	}

	public virtual void Release(int marker)
	{
		int num = -numMarkers;
		if (marker != num)
		{
			throw new InvalidOperationException("release() called with an invalid marker.");
		}
		numMarkers--;
		if (numMarkers == 0)
		{
			if (p > 0)
			{
				Array.Copy(tokens, p, tokens, 0, n - p);
				n -= p;
				p = 0;
			}
			lastTokenBufferStart = lastToken;
		}
	}

	public virtual void Seek(int index)
	{
		if (index != currentTokenIndex)
		{
			if (index > currentTokenIndex)
			{
				Sync(index - currentTokenIndex);
				index = Math.Min(index, GetBufferStartIndex() + n - 1);
			}
			int bufferStartIndex = GetBufferStartIndex();
			int num = index - bufferStartIndex;
			if (num < 0)
			{
				throw new ArgumentException("cannot seek to negative index " + index);
			}
			if (num >= n)
			{
				throw new NotSupportedException("seek to index outside buffer: " + index + " not in " + bufferStartIndex + ".." + (bufferStartIndex + n));
			}
			p = num;
			currentTokenIndex = index;
			if (p == 0)
			{
				lastToken = lastTokenBufferStart;
			}
			else
			{
				lastToken = tokens[p - 1];
			}
		}
	}

	[return: NotNull]
	public virtual string GetText(Interval interval)
	{
		int bufferStartIndex = GetBufferStartIndex();
		int num = bufferStartIndex + tokens.Length - 1;
		int a = interval.a;
		int b = interval.b;
		if (a < bufferStartIndex || b > num)
		{
			string[] obj = new string[6] { "interval ", null, null, null, null, null };
			Interval interval2 = interval;
			obj[1] = interval2.ToString();
			obj[2] = " not in token buffer window: ";
			obj[3] = bufferStartIndex.ToString();
			obj[4] = "..";
			obj[5] = num.ToString();
			throw new NotSupportedException(string.Concat(obj));
		}
		int num2 = a - bufferStartIndex;
		int num3 = b - bufferStartIndex;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = num2; i <= num3; i++)
		{
			IToken token = tokens[i];
			stringBuilder.Append(token.Text);
		}
		return stringBuilder.ToString();
	}

	protected internal int GetBufferStartIndex()
	{
		return currentTokenIndex - p;
	}
}
