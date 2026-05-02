namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class RootNamedElementStepCodeGenerator : BindPathStepCodeGenerator<RootNamedElementStep>
{
	public override ICodeGenOutput PathExpression
	{
		get
		{
			ICodeGenOutput parentPathExpression = base.Instance.Parent.CodeGen().PathExpression;
			ICodeGenOutput parentMemberAccessOperator = base.Instance.Parent.CodeGen().MemberAccessOperator;
			string fieldName = ((!string.IsNullOrEmpty(base.Instance.UpdateCallParamOverride)) ? base.Instance.UpdateCallParamOverride : base.Instance.FieldName);
			return new LanguageSpecificString(() => parentPathExpression.CppCXName() + parentMemberAccessOperator.CppCXName() + fieldName, () => parentPathExpression.CppWinRTName() + parentMemberAccessOperator.CppWinRTName() + fieldName + "()", () => parentPathExpression.CSharpName() + "." + fieldName, () => parentPathExpression.VBName() + "." + fieldName);
		}
	}

	public override ICodeGenOutput UpdateCallParam
	{
		get
		{
			if (!string.IsNullOrEmpty(base.Instance.UpdateCallParamOverride))
			{
				return new LanguageSpecificString(() => base.Instance.UpdateCallParamOverride ?? "", () => base.Instance.UpdateCallParamOverride ?? "", () => "bindings." + base.Instance.UpdateCallParamOverride, () => "bindings." + base.Instance.UpdateCallParamOverride);
			}
			return new LanguageSpecificString(() => "obj->" + base.Instance.FieldName, () => "::winrt::get_self<" + base.Instance.Parent.ValueType.CppWinRTLocalElseRef() + ">(obj)->" + base.Instance.FieldName + "()", () => "obj." + base.Instance.FieldName, () => "obj." + base.Instance.FieldName);
		}
	}
}
