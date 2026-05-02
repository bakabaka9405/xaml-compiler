namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class AttachedPropertyStepCodeGenerator : DependencyPropertyStepCodeGenerator<AttachedPropertyStep>
{
	public override ICodeGenOutput PathExpression
	{
		get
		{
			ICodeGenOutput parentPathExpression = base.Instance.Parent.CodeGen().PathExpression;
			return new LanguageSpecificString(() => $"{base.Instance.OwnerType.CppCXName(IncludeHatIfApplicable: false)}::Get{base.Instance.PropertyName}({parentPathExpression.CppCXName()})", () => base.Instance.OwnerType.CppWinRTName() + "::Get" + base.Instance.PropertyName + "(" + parentPathExpression.CppWinRTName() + ")", () => $"{base.Instance.OwnerType.CSharpName()}.Get{base.Instance.PropertyName}({parentPathExpression.CSharpName()})", () => $"{base.Instance.OwnerType.VBName()}.Get{base.Instance.PropertyName}({parentPathExpression.VBName()})");
		}
	}

	public override ICodeGenOutput UpdateCallParam => new LanguageSpecificString(() => string.Format("{0}::Get{1}({2})", base.Instance.OwnerType.CppCXName(IncludeHatIfApplicable: false), base.Instance.PropertyName, "obj"), () => base.Instance.OwnerType.CppWinRTName() + "::Get" + base.Instance.PropertyName + "(obj)", () => string.Format("{0}.Get{1}({2})", base.Instance.OwnerType.CSharpName(), base.Instance.PropertyName, "obj"), () => string.Format("{0}.Get{1}({2})", base.Instance.OwnerType.VBName(), base.Instance.PropertyName, "obj"));

	public override ICodeGenOutput PathSetExpression(ICodeGenOutput value)
	{
		ICodeGenOutput parentPathExpression = base.Instance.Parent.CodeGen().PathExpression;
		return new LanguageSpecificString(() => base.Instance.OwnerType.CppCXName(IncludeHatIfApplicable: false) + "::Set" + base.Instance.PropertyName + "(" + parentPathExpression.CppCXName() + ", " + value.CppCXName() + ")", () => base.Instance.OwnerType.CppWinRTName() + "::Set" + base.Instance.PropertyName + "(" + parentPathExpression.CppWinRTName() + ", " + value.CppWinRTName() + ")", () => base.Instance.OwnerType.CSharpName() + ".Set" + base.Instance.PropertyName + "(" + parentPathExpression.CSharpName() + ", " + value.CSharpName() + ")", () => base.Instance.OwnerType.VBName() + ".Set" + base.Instance.PropertyName + "(" + parentPathExpression.VBName() + ", " + value.VBName() + ")");
	}
}
