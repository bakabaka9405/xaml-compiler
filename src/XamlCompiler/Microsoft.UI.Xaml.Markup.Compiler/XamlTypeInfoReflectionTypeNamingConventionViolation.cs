using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlTypeInfoReflectionTypeNamingConventionViolation : XamlCompileWarning
{
	public XamlTypeInfoReflectionTypeNamingConventionViolation(string typeName, string asmName)
		: base(ErrorCode.WMC1005)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.TypeInfoReflection_TypeViolatesNamingConvention, typeName, asmName);
	}
}
