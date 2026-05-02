using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal abstract class BindPathStepCodeGenerator<T> : CodeGeneratorBase<T>, IBindPathStepCodeGen where T : BindPathStep
{
	public virtual ICodeGenOutput MemberAccessOperator => new LanguageSpecificString(() => (!base.Instance.ValueType.UnderlyingType.IsValueType) ? "->" : ".", () => ".", () => ".", () => ".");

	public abstract ICodeGenOutput PathExpression { get; }

	public virtual ICodeGenOutput UpdateCallParam
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public virtual ICodeGenOutput PathSetExpression(ICodeGenOutput value)
	{
		ICodeGenOutput pathExpression = base.Instance.CodeGen().PathExpression;
		string cppWinrtNameNoParen = pathExpression.CppWinRTName().TrimEnd(')', '(');
		return new LanguageSpecificString(() => pathExpression.CppCXName() + " = " + value.CppCXName(), () => cppWinrtNameNoParen + "(" + value.CppWinRTName() + ")", () => pathExpression.CSharpName() + " = " + value.CSharpName(), () => pathExpression.VBName() + " = " + value.VBName());
	}
}
