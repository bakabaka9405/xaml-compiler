namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MethodSignatureDescriptor
{
	public CorCallingConvention CallingConvention { get; set; }

	public int GenericParameterCount { get; set; }

	public TypeSignatureDescriptor ReturnParameter { get; set; }

	public TypeSignatureDescriptor[] Parameters { get; set; }
}
