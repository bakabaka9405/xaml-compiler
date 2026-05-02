namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class FunctionStepCodeGenerator : BindPathStepCodeGenerator<FunctionStep>
{
	public override ICodeGenOutput PathExpression => base.Instance.Method.CodeGen().PathExpression;

	public override ICodeGenOutput UpdateCallParam => new LanguageSpecificString(() => string.Empty);
}
