namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class FunctionPathParamCodeGenerator : ParameterCodeGenerator<FunctionPathParam>
{
	public override ICodeGenOutput PathExpression => base.Instance.Path.CodeGen().PathExpression;
}
