namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class FunctionBoolParamCodeGenerator : ParameterCodeGenerator<FunctionBoolParam>
{
	public override ICodeGenOutput PathExpression => new LanguageSpecificString(() => (!base.Instance.Value) ? "false" : "true", () => (!base.Instance.Value) ? "false" : "true", () => (!base.Instance.Value) ? "false" : "true", () => (!base.Instance.Value) ? "False" : "True");
}
