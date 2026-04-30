using Antlr4.Runtime.Atn;

namespace Antlr4.Runtime;

public interface IRecognizer
{
	IVocabulary Vocabulary { get; }

	string[] RuleNames { get; }

	string GrammarFileName { get; }

	ATN Atn { get; }

	int State { get; }

	IIntStream InputStream { get; }
}
