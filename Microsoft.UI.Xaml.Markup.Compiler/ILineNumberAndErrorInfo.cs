namespace Microsoft.UI.Xaml.Markup.Compiler;

internal interface ILineNumberAndErrorInfo
{
	LineNumberInfo LineNumberInfo { get; }

	XamlCompileError GetAttributeProcessingError();
}
