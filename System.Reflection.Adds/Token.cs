using System.Diagnostics;
using System.Globalization;

namespace System.Reflection.Adds;

internal struct Token
{
	public static readonly Token Nil;

	private int value;

	public int Value => value;

	public TokenType TokenType => (TokenType)(value & 0xFF000000u);

	public int Index => value & 0xFFFFFF;

	public bool IsNil => Index == 0;

	[DebuggerStepThrough]
	public Token(int value)
	{
		this.value = value;
	}

	public Token(TokenType type, int rid)
	{
		value = (int)(type + rid);
	}

	[DebuggerStepThrough]
	public Token(uint value)
	{
		this.value = (int)value;
	}

	public static implicit operator int(Token token)
	{
		return token.value;
	}

	public static bool operator ==(Token token1, Token token2)
	{
		return token1.value == token2.value;
	}

	public static bool operator !=(Token token1, Token token2)
	{
		return !(token1 == token2);
	}

	public static bool operator ==(Token token1, int token2)
	{
		return token1.value == token2;
	}

	public static bool operator !=(Token token1, int token2)
	{
		return !(token1 == token2);
	}

	public static bool operator ==(int token1, Token token2)
	{
		return token1 == token2.value;
	}

	public static bool operator !=(int token1, Token token2)
	{
		return !(token1 == token2);
	}

	public static bool IsType(int token, params TokenType[] types)
	{
		for (int i = 0; i < types.Length; i++)
		{
			if ((TokenType)(token & 0xFF000000u) == types[i])
			{
				return true;
			}
		}
		return false;
	}

	public bool IsType(TokenType type)
	{
		return TokenType == type;
	}

	public override bool Equals(object obj)
	{
		if (obj is Token token)
		{
			return value == token.value;
		}
		if (obj is int)
		{
			return value == (int)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}(0x{1:x})", TokenType, Index);
	}

	static Token()
	{
		Nil = new Token(0);
	}
}
