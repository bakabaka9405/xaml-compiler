namespace Antlr4.Runtime.Atn;

public sealed class ActionTransition : Transition
{
	public readonly int ruleIndex;

	public readonly int actionIndex;

	public readonly bool isCtxDependent;

	public override TransitionType TransitionType => TransitionType.Action;

	public override bool IsEpsilon => true;

	public ActionTransition(ATNState target, int ruleIndex)
		: this(target, ruleIndex, -1, isCtxDependent: false)
	{
	}

	public ActionTransition(ATNState target, int ruleIndex, int actionIndex, bool isCtxDependent)
		: base(target)
	{
		this.ruleIndex = ruleIndex;
		this.actionIndex = actionIndex;
		this.isCtxDependent = isCtxDependent;
	}

	public override bool Matches(int symbol, int minVocabSymbol, int maxVocabSymbol)
	{
		return false;
	}

	public override string ToString()
	{
		return "action_" + ruleIndex + ":" + actionIndex;
	}
}
