namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MethodFilter
{
	public string Name { get; set; }

	public int GenericParameterCount { get; set; }

	public int ParameterCount { get; set; }

	public CorCallingConvention CallingConvention { get; set; }

	public MethodFilter(string name, int genericParameterCount, int parameterCount, CorCallingConvention callingConvention)
	{
		Name = name;
		GenericParameterCount = genericParameterCount;
		ParameterCount = parameterCount;
		CallingConvention = callingConvention;
	}
}
