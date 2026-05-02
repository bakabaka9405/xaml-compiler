using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class FunctionStringParam : FunctionParam
{
	public string Value { get; }

	public override string UniqueName => Value;

	public FunctionStringParam(string value)
	{
		Value = value;
	}

	protected override void ValidateParameter(Parameter paramInfo)
	{
		if (paramInfo.ParameterType.FullName != typeof(string).FullName)
		{
			throw new ArgumentException();
		}
	}
}
