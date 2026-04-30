using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorAssignment : XamlCompileError
{
	public XamlValidationErrorAssignment(XamlDomObject domChildObject, XamlMember property, XamlType propertyItemType)
		: base(ErrorCode.WMC0015, domChildObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CantAssign, domChildObject.Type.Name, property.Name, propertyItemType.Name);
	}
}
