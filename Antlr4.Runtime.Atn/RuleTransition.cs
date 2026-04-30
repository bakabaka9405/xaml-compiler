using System;
using Antlr4.Runtime.Misc;

namespace Antlr4.Runtime.Atn;

public sealed class RuleTransition : Transition
{
	public readonly int ruleIndex;

	public readonly int precedence;

	[NotNull]
	public ATNState followState;

	public bool tailCall;

	public bool optimizedTailCall;

	public override TransitionType TransitionType => TransitionType.Rule;

	public override bool IsEpsilon => true;

	[Obsolete("UseRuleTransition(RuleStartState, int, int, ATNState) instead.")]
	public RuleTransition(RuleStartState ruleStart, int ruleIndex, ATNState followState)
		: this(ruleStart, ruleIndex, 0, followState)
	{
	}

	public RuleTransition(RuleStartState ruleStart, int ruleIndex, int precedence, ATNState followState)
		: base(ruleStart)
	{
		this.ruleIndex = ruleIndex;
		this.precedence = precedence;
		this.followState = followState;
	}

	public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
	{
		return false;
	}
}
