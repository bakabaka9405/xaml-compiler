namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal abstract class ParameterCodeGenerator<T> : CodeGeneratorBase<T>, IBindPathParameterCodeGen where T : Parameter
{
	public virtual ICodeGenOutput PathExpression { get; }
}
