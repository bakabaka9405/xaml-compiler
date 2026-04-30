using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationDictionaryKeyError : XamlCompileError
{
	public XamlValidationDictionaryKeyError(XamlDomObject domObject)
		: base(ErrorCode.WMC0060, domObject)
	{
		string name = domObject.Type.Name;
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DictionaryItemsMustHaveKeys, name);
	}

	public XamlValidationDictionaryKeyError(XamlDomObject domObject, string keyText)
		: base(ErrorCode.WMC0065, domObject)
	{
		string name = domObject.Type.Name;
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DictionaryItemsHasDuplicateKey, name, keyText);
	}
}
