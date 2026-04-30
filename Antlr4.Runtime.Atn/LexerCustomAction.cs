using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class LexerCustomAction : ILexerAction
{
	private readonly int ruleIndex;

	private readonly int actionIndex;

	public int RuleIndex => ruleIndex;

	public int ActionIndex => actionIndex;

	public LexerActionType ActionType => LexerActionType.Custom;

	public bool IsPositionDependent => true;

	public LexerCustomAction(int ruleIndex, int actionIndex)
	{
		this.ruleIndex = ruleIndex;
		this.actionIndex = actionIndex;
	}

	public void Execute(Lexer lexer)
	{
		lexer.Action(null, ruleIndex, actionIndex);
	}

	public override int GetHashCode()
	{
		int hash = MurmurHash.Initialize();
		hash = MurmurHash.Update(hash, (int)ActionType);
		hash = MurmurHash.Update(hash, ruleIndex);
		hash = MurmurHash.Update(hash, actionIndex);
		return MurmurHash.Finish(hash, 3);
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (!(obj is LexerCustomAction))
		{
			return false;
		}
		LexerCustomAction lexerCustomAction = (LexerCustomAction)obj;
		if (ruleIndex == lexerCustomAction.ruleIndex)
		{
			return actionIndex == lexerCustomAction.actionIndex;
		}
		return false;
	}
}
