using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlSchemaError_AmbiguousCollectionAdd : XamlCompileError
{
	public XamlSchemaError_AmbiguousCollectionAdd(string typeName, string methodName, int argumentCount)
		: base(ErrorCode.WMC0820)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.DuiSchema_AmbiguousCollectionAdd, typeName, methodName, argumentCount);
	}
}
