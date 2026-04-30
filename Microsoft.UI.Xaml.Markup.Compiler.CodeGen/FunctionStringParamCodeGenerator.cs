namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class FunctionStringParamCodeGenerator : ParameterCodeGenerator<FunctionStringParam>
{
	public override ICodeGenOutput PathExpression => new LanguageSpecificString(() => base.Instance.Value.Quotenate(), () => "L" + base.Instance.Value.Quotenate(), () => base.Instance.Value.Quotenate(), () => base.Instance.Value.Quotenate());
}
