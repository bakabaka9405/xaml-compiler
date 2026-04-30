namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class CastStepCodeGenerator : BindPathStepCodeGenerator<CastStep>
{
	public override ICodeGenOutput PathExpression
	{
		get
		{
			ICodeGenOutput parentPathExpression = base.Instance.Parent.CodeGen().PathExpression;
			return new LanguageSpecificString(() => $"(safe_cast<{base.Instance.ValueType.CppCXName()}>({parentPathExpression.CppCXName()}))", () => base.Instance.Parent.ValueType.CppWinRTCast(base.Instance.ValueType, parentPathExpression.CppWinRTName()), () => $"(({base.Instance.ValueType.CSharpName()}){parentPathExpression.CSharpName()})", () => string.Format("{2}({1}, {0})", base.Instance.ValueType.VBName(), parentPathExpression.VBName(), base.Instance.Parent.ValueType.GetVBCastName(base.Instance.ValueType.UnderlyingType)));
		}
	}

	public override ICodeGenOutput UpdateCallParam
	{
		get
		{
			if (base.Instance.ValueType.IsString())
			{
				return new LanguageSpecificString(() => string.Format("{0}->ToString()", "obj"), () => string.Format("::winrt::to_hstring({0})", "obj"), () => string.Format("{0}.ToString()", "obj"), () => string.Format("{0}.ToString()", "obj"));
			}
			return base.Instance.Parent.ValueType.GetInlineConversionExpression(base.Instance.ValueType, new LanguageSpecificString(() => "obj"));
		}
	}
}
