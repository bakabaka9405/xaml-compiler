namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class ApiInformationMethodCodeGenerator : CodeGeneratorBase<ApiInformationMethod>, IApiInformationCodeGen
{
	public ICodeGenOutput CallExpression => new LanguageSpecificString(() => string.Format("{1}::Windows::Foundation::Metadata::ApiInformation::{0}", base.Instance.MethodName, base.Instance.Condition ? "" : "!"), () => string.Format("{1}::winrt::Windows::Foundation::Metadata::ApiInformation::{0}", base.Instance.MethodName, base.Instance.Condition ? "" : "!"), () => string.Format("{1}global::Windows.Foundation.Metadata.ApiInformation.{0}", base.Instance.MethodName, base.Instance.Condition ? "" : "!"), () => string.Format("{1}Global.Windows.Foundation.Metadata.ApiInformation.{0}", base.Instance.MethodName, base.Instance.Condition ? "" : "Not "));
}
