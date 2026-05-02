using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class LexerPushModeAction : ILexerAction
{
	private readonly int mode;

	public int Mode => mode;

	public LexerActionType ActionType => LexerActionType.PushMode;

	public bool IsPositionDependent => false;

	public LexerPushModeAction(int mode)
	{
		this.mode = mode;
	}

	public void Execute(Lexer lexer)
	{
		lexer.PushMode(mode);
	}

	public override int GetHashCode()
	{
		int hash = MurmurHash.Initialize();
		hash = MurmurHash.Update(hash, (int)ActionType);
		hash = MurmurHash.Update(hash, mode);
		return MurmurHash.Finish(hash, 2);
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is LexerPushModeAction))
		{
			return false;
		}
		return mode == ((LexerPushModeAction)obj).mode;
	}

	public override string ToString()
	{
		return $"pushMode({mode})";
	}
}
