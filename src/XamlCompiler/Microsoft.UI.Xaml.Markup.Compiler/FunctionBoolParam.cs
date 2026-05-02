using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class FunctionBoolParam : FunctionParam
{
	public bool Value { get; }

	public override string UniqueName
	{
		get
		{
			if (!Value)
			{
				return "False";
			}
			return "True";
		}
	}

	public FunctionBoolParam(bool value)
	{
		Value = value;
	}

	protected override void ValidateParameter(Parameter paramInfo)
	{
		if (paramInfo.ParameterType.FullName != typeof(bool).FullName)
		{
			throw new ArgumentException();
		}
	}
}
