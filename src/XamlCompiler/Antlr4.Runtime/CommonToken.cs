using System;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

[Serializable]
public class CommonToken : IWritableToken, IToken
{
	private const long serialVersionUID = -6708843461296520577L;

	protected internal static readonly Tuple<ITokenSource, ICharStream> EmptySource = Tuple.Create<ITokenSource, ICharStream>(null, null);

	private int _type;

	private int _line;

	protected internal int charPositionInLine = -1;

	private int _channel;

	[NotNull]
	protected internal Tuple<ITokenSource, ICharStream> source;

	private string _text;

	protected internal int index = -1;

	protected internal int start;

	protected internal int stop;

	public virtual int Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	public virtual int Line
	{
		get
		{
			return _line;
		}
		set
		{
			_line = value;
		}
	}

	public virtual string Text
	{
		get
		{
			if (_text != null)
			{
				return _text;
			}
			ICharStream inputStream = InputStream;
			if (inputStream == null)
			{
				return null;
			}
			int size = inputStream.Size;
			if (start < size && stop < size)
			{
				return inputStream.GetText(Interval.Of(start, stop));
			}
			return "<EOF>";
		}
		set
		{
			_text = value;
		}
	}

	public virtual int Column
	{
		get
		{
			return charPositionInLine;
		}
		set
		{
			charPositionInLine = value;
		}
	}

	public virtual int Channel
	{
		get
		{
			return _channel;
		}
		set
		{
			_channel = value;
		}
	}

	public virtual int StartIndex
	{
		get
		{
			return start;
		}
		set
		{
			start = value;
		}
	}

	public virtual int StopIndex
	{
		get
		{
			return stop;
		}
		set
		{
			stop = value;
		}
	}

	public virtual int TokenIndex
	{
		get
		{
			return index;
		}
		set
		{
			index = value;
		}
	}

	public virtual ITokenSource TokenSource => source.Item1;

	public virtual ICharStream InputStream => source.Item2;

	public CommonToken(int type)
	{
		_type = type;
		source = EmptySource;
	}

	public CommonToken(Tuple<ITokenSource, ICharStream> source, int type, int channel, int start, int stop)
	{
		this.source = source;
		_type = type;
		_channel = channel;
		this.start = start;
		this.stop = stop;
		if (source.Item1 != null)
		{
			_line = source.Item1.Line;
			charPositionInLine = source.Item1.Column;
		}
	}

	public CommonToken(int type, string text)
	{
		_type = type;
		_channel = 0;
		_text = text;
		source = EmptySource;
	}

	public CommonToken(IToken oldToken)
	{
		_type = oldToken.Type;
		_line = oldToken.Line;
		index = oldToken.TokenIndex;
		charPositionInLine = oldToken.Column;
		_channel = oldToken.Channel;
		start = oldToken.StartIndex;
		stop = oldToken.StopIndex;
		if (oldToken is CommonToken)
		{
			_text = ((CommonToken)oldToken)._text;
			source = ((CommonToken)oldToken).source;
		}
		else
		{
			_text = oldToken.Text;
			source = Tuple.Create(oldToken.TokenSource, oldToken.InputStream);
		}
	}

	public override string ToString()
	{
		string text = string.Empty;
		if (_channel > 0)
		{
			text = ",channel=" + _channel;
		}
		string text2 = Text;
		if (text2 != null)
		{
			text2 = text2.Replace("\n", "\\n");
			text2 = text2.Replace("\r", "\\r");
			text2 = text2.Replace("\t", "\\t");
		}
		else
		{
			text2 = "<no text>";
		}
		return "[@" + TokenIndex + "," + start + ":" + stop + "='" + text2 + "',<" + _type + ">" + text + "," + _line + ":" + Column + "]";
	}
}
