using System;
using System.Collections.Generic;

namespace Antlr4.Runtime;

public class ListTokenSource : ITokenSource
{
	protected internal readonly IList<IToken> tokens;

	private readonly string sourceName;

	protected internal int i;

	protected internal IToken eofToken;

	private ITokenFactory _factory = CommonTokenFactory.Default;

	public virtual int Column
	{
		get
		{
			if (i < tokens.Count)
			{
				return tokens[i].Column;
			}
			if (eofToken != null)
			{
				return eofToken.Column;
			}
			if (tokens.Count > 0)
			{
				IToken token = tokens[tokens.Count - 1];
				string text = token.Text;
				if (text != null)
				{
					int num = text.LastIndexOf('\n');
					if (num >= 0)
					{
						return text.Length - num - 1;
					}
				}
				return token.Column + token.StopIndex - token.StartIndex + 1;
			}
			return 0;
		}
	}

	public virtual int Line
	{
		get
		{
			if (this.i < tokens.Count)
			{
				return tokens[this.i].Line;
			}
			if (eofToken != null)
			{
				return eofToken.Line;
			}
			if (tokens.Count > 0)
			{
				IToken token = tokens[tokens.Count - 1];
				int num = token.Line;
				string text = token.Text;
				if (text != null)
				{
					for (int i = 0; i < text.Length; i++)
					{
						if (text[i] == '\n')
						{
							num++;
						}
					}
				}
				return num;
			}
			return 1;
		}
	}

	public virtual ICharStream InputStream
	{
		get
		{
			if (i < tokens.Count)
			{
				return tokens[i].InputStream;
			}
			if (eofToken != null)
			{
				return eofToken.InputStream;
			}
			if (tokens.Count > 0)
			{
				return tokens[tokens.Count - 1].InputStream;
			}
			return null;
		}
	}

	public virtual string SourceName
	{
		get
		{
			if (sourceName != null)
			{
				return sourceName;
			}
			ICharStream inputStream = InputStream;
			if (inputStream != null)
			{
				return inputStream.SourceName;
			}
			return "List";
		}
	}

	public virtual ITokenFactory TokenFactory
	{
		get
		{
			return _factory;
		}
		set
		{
			_factory = value;
		}
	}

	public ListTokenSource(IList<IToken> tokens)
		: this(tokens, null)
	{
	}

	public ListTokenSource(IList<IToken> tokens, string sourceName)
	{
		if (tokens == null)
		{
			throw new ArgumentNullException("tokens cannot be null");
		}
		this.tokens = tokens;
		this.sourceName = sourceName;
	}

	public virtual IToken NextToken()
	{
		if (i >= tokens.Count)
		{
			if (eofToken == null)
			{
				int num = -1;
				if (tokens.Count > 0)
				{
					int stopIndex = tokens[tokens.Count - 1].StopIndex;
					if (stopIndex != -1)
					{
						num = stopIndex + 1;
					}
				}
				int stop = Math.Max(-1, num - 1);
				eofToken = _factory.Create(Tuple.Create((ITokenSource)this, InputStream), -1, "EOF", 0, num, stop, Line, Column);
			}
			return eofToken;
		}
		IToken token = tokens[i];
		if (i == tokens.Count - 1 && token.Type == -1)
		{
			eofToken = token;
		}
		i++;
		return token;
	}
}
