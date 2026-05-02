using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class StaticRootStepCodeGenerator : BindPathStepCodeGenerator<StaticRootStep>
{
	public override ICodeGenOutput MemberAccessOperator => new LanguageSpecificString(() => "::", () => "::", () => ".", () => ".");

	public override ICodeGenOutput PathExpression
	{
		get
		{
			XamlType valueType = base.Instance.ValueType;
			return new LanguageSpecificString(() => valueType.CppCXName(IncludeHatIfApplicable: false), () => valueType.CppWinRTName(), valueType.CSharpName, valueType.VBName);
		}
	}
}
