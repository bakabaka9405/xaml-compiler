namespace Microsoft.UI.Xaml.Markup.Compiler;

public interface ICodeGenOutput
{
	CodeGenDelegate CppCXName { get; }

	CodeGenDelegate CppWinRTName { get; }

	CodeGenDelegate CSharpName { get; }

	CodeGenDelegate VBName { get; }
}
