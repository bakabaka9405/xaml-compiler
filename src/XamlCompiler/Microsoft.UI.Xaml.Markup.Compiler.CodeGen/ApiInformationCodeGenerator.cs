using System.Linq;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class ApiInformationCodeGenerator : CodeGeneratorBase<ApiInformation>, IApiInformationCodeGen
{
	public ICodeGenOutput CallExpression
	{
		get
		{
			ICodeGenOutput callExpression = base.Instance.Method.CodeGen().CallExpression;
			return new LanguageSpecificString(() => string.Format("{0}({1})", callExpression.CppCXName(), string.Join(", ", base.Instance.Parameters.Select((ApiInformationParameter p) => p.CodeGen().CallExpression.CppCXName()))), () => string.Format("{0}({1})", callExpression.CppWinRTName(), string.Join(", ", base.Instance.Parameters.Select((ApiInformationParameter p) => p.CodeGen().CallExpression.CppWinRTName()))), () => string.Format("{0}({1})", callExpression.CSharpName(), string.Join(", ", base.Instance.Parameters.Select((ApiInformationParameter p) => p.CodeGen().CallExpression.CSharpName()))), () => string.Format("{0}({1})", callExpression.VBName(), string.Join(", ", base.Instance.Parameters.Select((ApiInformationParameter p) => p.CodeGen().CallExpression.VBName()))));
		}
	}
}
