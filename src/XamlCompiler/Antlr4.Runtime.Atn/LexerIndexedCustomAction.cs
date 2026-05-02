using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class LexerIndexedCustomAction : ILexerAction
{
	private readonly int offset;

	private readonly ILexerAction action;

	public int Offset => offset;

	[NotNull]
	public ILexerAction Action => action;

	public LexerActionType ActionType => action.ActionType;

	public bool IsPositionDependent => true;

	public LexerIndexedCustomAction(int offset, ILexerAction action)
	{
		this.offset = offset;
		this.action = action;
	}

	public void Execute(Lexer lexer)
	{
		action.Execute(lexer);
	}

	public override int GetHashCode()
	{
		int hash = MurmurHash.Initialize();
		hash = MurmurHash.Update(hash, offset);
		hash = MurmurHash.Update(hash, action);
		return MurmurHash.Finish(hash, 2);
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is LexerIndexedCustomAction))
		{
			return false;
		}
		LexerIndexedCustomAction lexerIndexedCustomAction = (LexerIndexedCustomAction)obj;
		if (offset == lexerIndexedCustomAction.offset)
		{
			return action.Equals(lexerIndexedCustomAction.action);
		}
		return false;
	}
}
