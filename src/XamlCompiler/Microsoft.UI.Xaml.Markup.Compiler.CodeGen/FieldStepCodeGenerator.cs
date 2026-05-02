namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class FieldStepCodeGenerator : PropertyStepCodeGenerator<FieldStep>
{
	public override ICodeGenOutput PathExpression
	{
		get
		{
			ICodeGenOutput parentPathExpression = base.Instance.Parent.CodeGen().PathExpression;
			ICodeGenOutput parentMemberAccessOperator = base.Instance.Parent.CodeGen().MemberAccessOperator;
			return new LanguageSpecificString(() => parentPathExpression.CppCXName() + parentMemberAccessOperator.CppCXName() + base.Instance.FieldName, () => parentPathExpression.CppWinRTName() + parentMemberAccessOperator.CppWinRTName() + base.Instance.FieldName, () => parentPathExpression.CSharpName() + "." + base.Instance.FieldName, () => parentPathExpression.VBName() + "." + base.Instance.FieldName);
		}
	}

	public override ICodeGenOutput UpdateCallParam
	{
		get
		{
			ICodeGenOutput parentPathExpression = base.Instance.Parent.CodeGen().PathExpression;
			ICodeGenOutput parentMemberAccessOperator = base.Instance.Parent.CodeGen().MemberAccessOperator;
			return new LanguageSpecificString(() => ((base.Instance.Parent is StaticRootStep) ? parentPathExpression.CppCXName() : "obj") + parentMemberAccessOperator.CppCXName() + base.Instance.FieldName, () => ((base.Instance.Parent is StaticRootStep) ? parentPathExpression.CppWinRTName() : "obj") + parentMemberAccessOperator.CppWinRTName() + base.Instance.FieldName, () => ((base.Instance.Parent is StaticRootStep) ? parentPathExpression.CSharpName() : "obj") + "." + base.Instance.FieldName, () => ((base.Instance.Parent is StaticRootStep) ? parentPathExpression.VBName() : "obj") + "." + base.Instance.FieldName);
		}
	}
}
