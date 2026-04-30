using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public interface ILexerAction
{
	[NotNull]
	LexerActionType ActionType { get; }

	bool IsPositionDependent { get; }

	void Execute(Lexer lexer);
}
