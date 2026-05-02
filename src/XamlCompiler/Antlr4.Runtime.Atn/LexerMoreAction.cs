using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class LexerMoreAction : ILexerAction
{
	public static readonly LexerMoreAction Instance = new LexerMoreAction();

	public LexerActionType ActionType => LexerActionType.More;

	public bool IsPositionDependent => false;

	private LexerMoreAction()
	{
	}

	public void Execute(Lexer lexer)
	{
		lexer.More();
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
		return "more";
	}
}
