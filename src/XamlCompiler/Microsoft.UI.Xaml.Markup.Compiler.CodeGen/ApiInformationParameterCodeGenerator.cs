namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class ApiInformationParameterCodeGenerator : CodeGeneratorBase<ApiInformationParameter>, IApiInformationCodeGen
{
	public ICodeGenOutput CallExpression => new LanguageSpecificString(() => string.Format("{0}{1}{0}", (base.Instance.ParameterType == typeof(string)) ? "\"" : "", base.Instance.ParameterValue), () => string.Format("{0}{1}{2}", (base.Instance.ParameterType == typeof(string)) ? "L\"" : "", base.Instance.ParameterValue, (base.Instance.ParameterType == typeof(string)) ? "\"" : ""), () => string.Format("{0}{1}{0}", (base.Instance.ParameterType == typeof(string)) ? "\"" : "", base.Instance.ParameterValue), () => string.Format("{0}{1}{0}", (base.Instance.ParameterType == typeof(string)) ? "\"" : "", base.Instance.ParameterValue));
}
