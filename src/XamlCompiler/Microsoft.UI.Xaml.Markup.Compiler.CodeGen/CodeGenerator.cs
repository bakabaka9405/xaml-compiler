using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal abstract class CodeGenerator<T> : T4Base<T>
{
	protected virtual string GetPhaseCondition(BindPathStep bindStep)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(phase & (NOT_PHASED");
		if (bindStep.IsTrackingSource)
		{
			stringBuilder.Append(" | DATA_CHANGED");
		}
		foreach (int distinctPhase in bindStep.DistinctPhases)
		{
			stringBuilder.AppendFormat(" | (1 << {0})", distinctPhase);
		}
		stringBuilder.Append(")) != 0");
		return stringBuilder.ToString();
	}

	public virtual string GetDirectPhaseCondition(int phase, bool isTracking)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("(phase & ((1 << {0}) | NOT_PHASED ", phase);
		if (isTracking)
		{
			stringBuilder.Append("| DATA_CHANGED");
		}
		stringBuilder.Append(")) != 0");
		return stringBuilder.ToString();
	}
}
