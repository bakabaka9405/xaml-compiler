using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorContractDoesNotExist : XamlCompileWarning
{
	public XamlValidationErrorContractDoesNotExist(XamlDomObject domObject, string typeName, string contractName, string runtimeVer)
		: base(ErrorCode.WMC0152, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_TypeContractDoesNotExist, typeName, contractName, runtimeVer);
	}

	public XamlValidationErrorContractDoesNotExist(XamlDomMember domMember, string typeName, string contractName, string runtimeVer)
		: base(ErrorCode.WMC0152, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_MemberContractDoesNotExist, typeName, contractName, runtimeVer, domMember.Member.Name);
	}
}
