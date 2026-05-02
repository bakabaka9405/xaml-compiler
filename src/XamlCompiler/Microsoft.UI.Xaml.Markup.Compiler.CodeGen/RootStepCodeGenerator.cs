namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class RootStepCodeGenerator : BindPathStepCodeGenerator<RootStep>
{
	public override ICodeGenOutput PathExpression
	{
		get
		{
			if (base.Instance.IsElementRoot)
			{
				return new LanguageSpecificString(() => "this", () => "", () => "this", () => "Me");
			}
			return new LanguageSpecificString(() => "this->GetDataRoot()", () => "GetDataRoot()", () => "this.dataRoot", () => "Me.dataRoot");
		}
	}
}
