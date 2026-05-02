namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal interface IBindPathStepCodeGen
{
	ICodeGenOutput PathExpression { get; }

	ICodeGenOutput UpdateCallParam { get; }

	ICodeGenOutput MemberAccessOperator { get; }

	ICodeGenOutput PathSetExpression(ICodeGenOutput input);
}
