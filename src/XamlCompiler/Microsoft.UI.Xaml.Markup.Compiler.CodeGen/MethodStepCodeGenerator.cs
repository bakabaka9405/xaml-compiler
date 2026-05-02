namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class MethodStepCodeGenerator : BindPathStepCodeGenerator<MethodStep>
{
	public override ICodeGenOutput PathExpression
	{
		get
		{
			ICodeGenOutput parentPathExpression = base.Instance.Parent.CodeGen().PathExpression;
			ICodeGenOutput parentMemberAccessOperator = base.Instance.Parent.CodeGen().MemberAccessOperator;
			string paramList = base.Instance.Parameters.ForCall();
			return new LanguageSpecificString(() => parentPathExpression.CppCXName() + parentMemberAccessOperator.CppCXName() + base.Instance.MethodName + "(" + paramList + ")", () => parentPathExpression.CppWinRTName() + parentMemberAccessOperator.CppWinRTName() + base.Instance.MethodName + "(" + paramList + ")", () => parentPathExpression.CSharpName() + "." + base.Instance.MethodName + "(" + paramList + ")", () => parentPathExpression.VBName() + "." + base.Instance.MethodName + "(" + paramList + ")");
		}
	}
}
