using System;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class Parameter
{
	public int Position { get; protected set; }

	public Type ParameterType { get; protected set; }

	public bool IsOut { get; protected set; }

	public string Name => $"p{Position}";

	public Parameter()
	{
	}

	public Parameter(ParameterInfo parameterInfo)
	{
		Position = parameterInfo.Position;
		ParameterType = parameterInfo.ParameterType;
		IsOut = parameterInfo.IsOut;
	}

	public void CopyFrom(Parameter param)
	{
		Position = param.Position;
		ParameterType = param.ParameterType;
		IsOut = param.IsOut;
	}
}
