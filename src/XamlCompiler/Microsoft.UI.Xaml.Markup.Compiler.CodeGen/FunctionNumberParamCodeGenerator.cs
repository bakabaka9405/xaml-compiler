namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class FunctionNumberParamCodeGenerator : ParameterCodeGenerator<FunctionNumberParam>
{
	public override ICodeGenOutput PathExpression => new LanguageSpecificString(delegate
	{
		string text = "";
		if (base.Instance.ValueType.IsFloat())
		{
			text = "F";
		}
		return base.Instance.Value + text;
	});
}
