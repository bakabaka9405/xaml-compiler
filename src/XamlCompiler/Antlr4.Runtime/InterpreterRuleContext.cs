namespace Antlr4.Runtime;

public class InterpreterRuleContext : ParserRuleContext
{
	private readonly int ruleIndex;

	public override int RuleIndex => ruleIndex;

	public InterpreterRuleContext(ParserRuleContext parent, int invokingStateNumber, int ruleIndex)
		: base(parent, invokingStateNumber)
	{
		this.ruleIndex = ruleIndex;
	}
}
