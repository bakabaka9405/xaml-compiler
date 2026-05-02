using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorCollectionAdd : XamlCompileError
{
	public XamlValidationErrorCollectionAdd(XamlDomItem domChildItem, XamlType itemType, XamlDomObject collectionObject, XamlDomMember collectionMember)
		: base(ErrorCode.WMC0020, domChildItem)
	{
		XamlDomValue xamlDomValue = domChildItem as XamlDomValue;
		XamlDomObject xamlDomObject = domChildItem as XamlDomObject;
		string text = ((xamlDomValue != null) ? (xamlDomValue.Value as string) : xamlDomObject.Type.Name);
		if (collectionObject.IsGetObject)
		{
			base.Code = ErrorCode.WMC0020;
			base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAddToCollectionProperty, text, collectionMember.Member.Name, itemType.Name);
		}
		else
		{
			base.Code = ErrorCode.WMC0021;
			base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAddToCollectionObject, text, collectionObject.Type.Name, itemType.Name);
		}
	}
}
