using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public class LexerTypeAction : ILexerAction
{
	private readonly int type;

	public virtual int Type => type;

	public virtual LexerActionType ActionType => LexerActionType.Type;

	public virtual bool IsPositionDependent => false;

	public LexerTypeAction(int type)
	{
		this.type = type;
	}

	public virtual void Execute(Lexer lexer)
	{
		lexer.Type = type;
	}

	public override int GetHashCode()
	{
		int hash = MurmurHash.Initialize();
		hash = MurmurHash.Update(hash, (int)ActionType);
		hash = MurmurHash.Update(hash, type);
		return MurmurHash.Finish(hash, 2);
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is LexerTypeAction))
		{
			return false;
		}
		return type == ((LexerTypeAction)obj).type;
	}

	public override string ToString()
	{
		return $"type({type})";
	}
}
