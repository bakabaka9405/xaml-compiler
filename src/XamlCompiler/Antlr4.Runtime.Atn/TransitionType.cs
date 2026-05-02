namespace Antlr4.Runtime.Atn;

public enum TransitionType
{
	Invalid,
	Epsilon,
	Range,
	Rule,
	Predicate,
	Atom,
	Action,
	Set,
	NotSet,
	Wildcard,
	Precedence
}
