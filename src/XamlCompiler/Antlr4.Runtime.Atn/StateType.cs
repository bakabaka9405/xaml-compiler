namespace Antlr4.Runtime.Atn;

public enum StateType
{
	InvalidType,
	Basic,
	RuleStart,
	BlockStart,
	PlusBlockStart,
	StarBlockStart,
	TokenStart,
	RuleStop,
	BlockEnd,
	StarLoopBack,
	StarLoopEntry,
	PlusLoopBack,
	LoopEnd
}
