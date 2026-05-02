using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime;

public interface IVocabulary
{
	[return: Nullable]
	string GetLiteralName(int tokenType);

	[return: Nullable]
	string GetSymbolicName(int tokenType);

	[return: NotNull]
	string GetDisplayName(int tokenType);
}
