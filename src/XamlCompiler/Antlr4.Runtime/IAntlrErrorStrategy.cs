using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public interface IAntlrErrorStrategy
{
	void Reset(Parser recognizer);

	[return: NotNull]
	IToken RecoverInline(Parser recognizer);

	void Recover(Parser recognizer, RecognitionException e);

	void Sync(Parser recognizer);

	bool InErrorRecoveryMode(Parser recognizer);

	void ReportMatch(Parser recognizer);

	void ReportError(Parser recognizer, RecognitionException e);
}
