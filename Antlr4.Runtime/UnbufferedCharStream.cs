using System;
using System.IO;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Sharpen;

namespace Antlr4.Runtime;

public class UnbufferedCharStream : ICharStream, IIntStream
{
	protected internal char[] data;

	protected internal int n;

	protected internal int p;

	protected internal int numMarkers;

	protected internal int lastChar = -1;

	protected internal int lastCharBufferStart;

	protected internal int currentCharIndex;

	protected internal TextReader input;

	public string name;

	public virtual int Index => currentCharIndex;

	public virtual int Size
	{
		get
		{
			throw new NotSupportedException("Unbuffered stream cannot know its size");
		}
	}

	public virtual string SourceName
	{
		get
		{
			if (string.IsNullOrEmpty(name))
			{
				return "<unknown>";
			}
			return name;
		}
	}

	protected internal int BufferStartIndex => currentCharIndex - p;

	public UnbufferedCharStream()
		: this(256)
	{
	}

	public UnbufferedCharStream(int bufferSize)
	{
		n = 0;
		data = new char[bufferSize];
	}

	public UnbufferedCharStream(Stream input)
		: this(input, 256)
	{
	}

	public UnbufferedCharStream(TextReader input)
		: this(input, 256)
	{
	}

	public UnbufferedCharStream(Stream input, int bufferSize)
		: this(bufferSize)
	{
		this.input = new StreamReader(input);
		Fill(1);
	}

	public UnbufferedCharStream(TextReader input, int bufferSize)
		: this(bufferSize)
	{
		this.input = input;
		Fill(1);
	}

	public virtual void Consume()
	{
		if (La(1) == -1)
		{
			throw new InvalidOperationException("cannot consume EOF");
		}
		lastChar = data[p];
		if (p == n - 1 && numMarkers == 0)
		{
			n = 0;
			p = -1;
			lastCharBufferStart = lastChar;
		}
		p++;
		currentCharIndex++;
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
			if (this.n > 0 && data[this.n - 1] == '\uffff')
			{
				return i;
			}
			int c = NextChar();
			Add(c);
		}
		return n;
	}

	protected internal virtual int NextChar()
	{
		return input.Read();
	}

	protected internal virtual void Add(int c)
	{
		if (n >= data.Length)
		{
			data = Arrays.CopyOf(data, data.Length * 2);
		}
		data[n++] = (char)c;
	}

	public virtual int La(int i)
	{
		if (i == -1)
		{
			return lastChar;
		}
		Sync(i);
		int num = p + i - 1;
		if (num < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (num >= n)
		{
			return -1;
		}
		char c = data[num];
		if (c == '\uffff')
		{
			return -1;
		}
		return c;
	}

	public virtual int Mark()
	{
		if (numMarkers == 0)
		{
			lastCharBufferStart = lastChar;
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
		if (numMarkers == 0 && p > 0)
		{
			Array.Copy(data, p, data, 0, n - p);
			n -= p;
			p = 0;
			lastCharBufferStart = lastChar;
		}
	}

	public virtual void Seek(int index)
	{
		if (index != currentCharIndex)
		{
			if (index > currentCharIndex)
			{
				Sync(index - currentCharIndex);
				index = Math.Min(index, BufferStartIndex + n - 1);
			}
			int num = index - BufferStartIndex;
			if (num < 0)
			{
				throw new ArgumentException("cannot seek to negative index " + index);
			}
			if (num >= n)
			{
				throw new NotSupportedException("seek to index outside buffer: " + index + " not in " + BufferStartIndex + ".." + (BufferStartIndex + n));
			}
			p = num;
			currentCharIndex = index;
			if (p == 0)
			{
				lastChar = lastCharBufferStart;
			}
			else
			{
				lastChar = data[p - 1];
			}
		}
	}

	public virtual string GetText(Interval interval)
	{
		if (interval.a < 0 || interval.b < interval.a - 1)
		{
			throw new ArgumentException("invalid interval");
		}
		int bufferStartIndex = BufferStartIndex;
		if (n > 0 && data[n - 1] == '\uffff' && interval.a + interval.Length > bufferStartIndex + n)
		{
			throw new ArgumentException("the interval extends past the end of the stream");
		}
		if (interval.a < bufferStartIndex || interval.b >= bufferStartIndex + n)
		{
			string[] obj = new string[6] { "interval ", null, null, null, null, null };
			Interval interval2 = interval;
			obj[1] = interval2.ToString();
			obj[2] = " outside buffer: ";
			obj[3] = bufferStartIndex.ToString();
			obj[4] = "..";
			obj[5] = (bufferStartIndex + n - 1).ToString();
			throw new NotSupportedException(string.Concat(obj));
		}
		int startIndex = interval.a - bufferStartIndex;
		return new string(data, startIndex, interval.Length);
	}
}
