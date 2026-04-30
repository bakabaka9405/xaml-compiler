using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class LexerPopModeAction : ILexerAction
{
	public static readonly LexerPopModeAction Instance = new LexerPopModeAction();

	public LexerActionType ActionType => LexerActionType.PopMode;

	public bool IsPositionDependent => false;

	private LexerPopModeAction()
	{
	}

	public void Execute(Lexer lexer)
	{
		lexer.PopMode();
	}

	public override int GetHashCode()
	{
		int hash = MurmurHash.Initialize();
		hash = MurmurHash.Update(hash, (int)ActionType);
		return MurmurHash.Finish(hash, 1);
	}

	public override bool Equals(object obj)
	{
		return obj == this;
	}

	public override string ToString()
	{
		return "popMode";
	}
}
