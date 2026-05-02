using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorDictionaryAdd : XamlCompileError
{
	public XamlValidationErrorDictionaryAdd(XamlDomValue domChildValue)
		: base(ErrorCode.WMC0025, domChildValue)
	{
		string text = domChildValue.Value as string;
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_DictionaryItemsCannotBeText, text);
	}

	public XamlValidationErrorDictionaryAdd(XamlDomObject domChildObject, XamlType itemType, XamlDomObject domDictionaryObject, XamlDomMember domDictionaryProperty)
		: base(ErrorCode.WMC0026, domChildObject)
	{
		if (domDictionaryObject.IsGetObject)
		{
			base.Code = ErrorCode.WMC0026;
			base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAddToDictionaryProperty, domChildObject.Type.Name, domDictionaryProperty.Member.Name, itemType.Name);
		}
		else
		{
			base.Code = ErrorCode.WMC0027;
			base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAddToDictionaryObject, domChildObject.Type.Name, domDictionaryObject.Type.Name, itemType.Name);
		}
	}
}
