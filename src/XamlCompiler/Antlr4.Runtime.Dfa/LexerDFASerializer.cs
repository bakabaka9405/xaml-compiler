using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Dfa;

public class LexerDFASerializer : DFASerializer
{
	public LexerDFASerializer(DFA dfa)
		: base(dfa, Vocabulary.EmptyVocabulary)
	{
	}

	[return: NotNull]
	protected internal override string GetEdgeLabel(int i)
	{
		return "'" + (char)i + "'";
	}
}
