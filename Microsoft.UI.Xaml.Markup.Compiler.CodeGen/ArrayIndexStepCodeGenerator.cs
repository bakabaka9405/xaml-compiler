namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class ArrayIndexStepCodeGenerator : BindPathStepCodeGenerator<ArrayIndexStep>
{
	public override ICodeGenOutput PathExpression
	{
		get
		{
			ICodeGenOutput parentPathExpression = base.Instance.Parent.CodeGen().PathExpression;
			return new LanguageSpecificString(() => $"safe_cast<::Windows::Foundation::Collections::IVector<{base.Instance.ValueType.CppCXName()}>^>({parentPathExpression.CppCXName()})->GetAt({base.Instance.Index})", () => string.Format("{1}.as<::winrt::Windows::Foundation::Collections::IVector<{0}>>().GetAt({2})", base.Instance.ValueType.CppWinRTName(), parentPathExpression.CppWinRTName(), base.Instance.Index), () => $"{parentPathExpression.CSharpName()}[{base.Instance.Index}]", () => $"{parentPathExpression.VBName()}({base.Instance.Index})");
		}
	}

	public override ICodeGenOutput UpdateCallParam => new LanguageSpecificString(() => string.Format("safe_cast<::Windows::Foundation::Collections::IVector<{0}>^>({1})->GetAt({2})", base.Instance.ValueType.CppCXName(), "obj", base.Instance.Index), () => string.Format("{0}.as <::winrt::Windows::Foundation::Collections::IVector<{1}>>().GetAt({2})", "obj", base.Instance.ValueType.CppWinRTName(), base.Instance.Index), () => string.Format("{0}[{1}]", "obj", base.Instance.Index), () => string.Format("{0}({1})", "obj", base.Instance.Index));

	public override ICodeGenOutput PathSetExpression(ICodeGenOutput value)
	{
		ICodeGenOutput parentPathExpression = base.Instance.Parent.CodeGen().PathExpression;
		return new LanguageSpecificString(() => "safe_cast<::Windows::Foundation::Collections::IVector<" + base.Instance.ValueType.CppCXName() + ">^>" + $"({parentPathExpression.CppCXName()})->SetAt({base.Instance.Index}, {value.CppCXName()})", () => parentPathExpression.CppWinRTName() + ".as<::winrt::Windows::Foundation::Collections" + $"::IVector<{base.Instance.ValueType.CppWinRTName()}>>().SetAt({base.Instance.Index}, {value.CppWinRTName()})", () => PathExpression.CSharpName() + " = " + value.CSharpName(), () => PathExpression.VBName() + " = " + value.VBName());
	}
}
