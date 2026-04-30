using System;

namespace Antlr4.Runtime;

[Flags]
public enum Dependents
{
	None = 0,
	Self = 1,
	Parents = 2,
	Children = 4,
	Ancestors = 8,
	Descendants = 0x10,
	Siblings = 0x20,
	PreceedingSiblings = 0x40,
	FollowingSiblings = 0x80,
	Preceeding = 0x100,
	Following = 0x200
}
