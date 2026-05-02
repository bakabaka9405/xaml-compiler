namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class FunctionNullValueParamCodeGenerator : ParameterCodeGenerator<FunctionNullValueParam>
{
	public override ICodeGenOutput PathExpression => LanguageSpecificString.Null;
}
