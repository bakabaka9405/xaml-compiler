namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class PropertyStepCodeGenerator<T> : BindPathStepCodeGenerator<T> where T : PropertyStep
{
	public override ICodeGenOutput PathExpression
	{
		get
		{
			ICodeGenOutput parentPathExpression = base.Instance.Parent.CodeGen().PathExpression;
			ICodeGenOutput parentMemberAccessOperator = base.Instance.Parent.CodeGen().MemberAccessOperator;
			return new LanguageSpecificString(() => parentPathExpression.CppCXName() + parentMemberAccessOperator.CppCXName() + base.Instance.PropertyName, () => parentPathExpression.CppWinRTName() + parentMemberAccessOperator.CppWinRTName() + base.Instance.PropertyName + "()", () => parentPathExpression.CSharpName() + "." + base.Instance.PropertyName, () => parentPathExpression.VBName() + "." + base.Instance.PropertyName);
		}
	}

	public override ICodeGenOutput UpdateCallParam
	{
		get
		{
			ICodeGenOutput parentPathExpression = base.Instance.Parent.CodeGen().PathExpression;
			ICodeGenOutput parentMemberAccessOperator = base.Instance.Parent.CodeGen().MemberAccessOperator;
			return new LanguageSpecificString(() => ((base.Instance.Parent is StaticRootStep) ? parentPathExpression.CppCXName() : "obj") + parentMemberAccessOperator.CppCXName() + base.Instance.PropertyName, () => ((base.Instance.Parent is StaticRootStep) ? parentPathExpression.CppWinRTName() : "obj") + parentMemberAccessOperator.CppWinRTName() + base.Instance.PropertyName + "()", () => ((base.Instance.Parent is StaticRootStep) ? parentPathExpression.CSharpName() : "obj") + "." + base.Instance.PropertyName, () => ((base.Instance.Parent is StaticRootStep) ? parentPathExpression.VBName() : "obj") + "." + base.Instance.PropertyName);
		}
	}
}
