using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class FunctionNullValueParam : FunctionParam
{
	public override string UniqueName => "Null";

	protected override void ValidateParameter(Parameter paramInfo)
	{
		if (paramInfo.ParameterType.IsValueType)
		{
			throw new ArgumentException("Argument must be nullable");
		}
	}
}
