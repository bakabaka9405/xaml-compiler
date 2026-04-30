using System;
using System.IO;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public class AntlrInputStream : ICharStream, IIntStream
{
	public const int ReadBufferSize = 1024;

	public const int InitialBufferSize = 1024;

	protected internal char[] data;

	protected internal int n;

	protected internal int p;

	public string name;

	public virtual int Index => p;

	public virtual int Size => n;

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

	public AntlrInputStream()
	{
	}

	public AntlrInputStream(string input)
	{
		data = input.ToCharArray();
		n = input.Length;
	}

	public AntlrInputStream(char[] data, int numberOfActualCharsInArray)
	{
		this.data = data;
		n = numberOfActualCharsInArray;
	}

	public AntlrInputStream(TextReader r)
		: this(r, 1024, 1024)
	{
	}

	public AntlrInputStream(TextReader r, int initialSize)
		: this(r, initialSize, 1024)
	{
	}

	public AntlrInputStream(TextReader r, int initialSize, int readChunkSize)
	{
		Load(r, initialSize, readChunkSize);
	}

	public AntlrInputStream(Stream input)
		: this(new StreamReader(input), 1024)
	{
	}

	public AntlrInputStream(Stream input, int initialSize)
		: this(new StreamReader(input), initialSize)
	{
	}

	public AntlrInputStream(Stream input, int initialSize, int readChunkSize)
		: this(new StreamReader(input), initialSize, readChunkSize)
	{
	}

	public virtual void Load(TextReader r, int size, int readChunkSize)
	{
		if (r != null)
		{
			data = r.ReadToEnd().ToCharArray();
			n = data.Length;
		}
	}

	public virtual void Reset()
	{
		p = 0;
	}

	public virtual void Consume()
	{
		if (p >= n)
		{
			throw new InvalidOperationException("cannot consume EOF");
		}
		if (p < n)
		{
			p++;
		}
	}

	public virtual int La(int i)
	{
		if (i == 0)
		{
			return 0;
		}
		if (i < 0)
		{
			i++;
			if (p + i - 1 < 0)
			{
				return -1;
			}
		}
		if (p + i - 1 >= n)
		{
			return -1;
		}
		return data[p + i - 1];
	}

	public virtual int Lt(int i)
	{
		return La(i);
	}

	public virtual int Mark()
	{
		return -1;
	}

	public virtual void Release(int marker)
	{
	}

	public virtual void Seek(int index)
	{
		if (index <= p)
		{
			p = index;
			return;
		}
		index = Math.Min(index, n);
		while (p < index)
		{
			Consume();
		}
	}

	public virtual string GetText(Interval interval)
	{
		int a = interval.a;
		int num = interval.b;
		if (num >= n)
		{
			num = n - 1;
		}
		int length = num - a + 1;
		if (a >= n)
		{
			return string.Empty;
		}
		return new string(data, a, length);
	}

	public override string ToString()
	{
		return new string(data);
	}
}
