using System;
using System.Globalization;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class FunctionNumberParam : FunctionParam
{
	public string Value { get; }

	public override string UniqueName => Value;

	public FunctionNumberParam(string value)
	{
		Value = value;
	}

	protected override void ValidateParameter(Parameter paramInfo)
	{
		bool flag = false;
		if (paramInfo.ParameterType.FullName == typeof(short).FullName)
		{
			flag = short.TryParse(Value, out var _);
		}
		else if (paramInfo.ParameterType.FullName == typeof(ushort).FullName)
		{
			flag = ushort.TryParse(Value, out var _);
		}
		else if (paramInfo.ParameterType.FullName == typeof(int).FullName)
		{
			flag = int.TryParse(Value, out var _);
		}
		else if (paramInfo.ParameterType.FullName == typeof(uint).FullName)
		{
			flag = uint.TryParse(Value, out var _);
		}
		else if (paramInfo.ParameterType.FullName == typeof(long).FullName)
		{
			flag = long.TryParse(Value, out var _);
		}
		else if (paramInfo.ParameterType.FullName == typeof(ulong).FullName)
		{
			flag = ulong.TryParse(Value, out var _);
		}
		else if (paramInfo.ParameterType.FullName == typeof(float).FullName)
		{
			flag = float.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
		}
		else if (paramInfo.ParameterType.FullName == typeof(double).FullName)
		{
			flag = double.TryParse(Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var _);
		}
		if (!flag)
		{
			throw new ArgumentException();
		}
	}
}
