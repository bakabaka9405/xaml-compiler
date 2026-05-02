using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class ApiInformationParameter
{
	public string ParameterValue { get; }

	public Type ParameterType { get; set; }

	public string UniqueName => ParameterValue;

	public ApiInformationParameter(string value)
	{
		ParameterValue = value;
	}

	public ApiInformationParameter(Type type)
	{
		ParameterType = type;
	}
}
