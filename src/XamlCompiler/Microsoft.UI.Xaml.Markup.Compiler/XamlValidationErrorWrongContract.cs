using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlValidationErrorWrongContract : XamlCompileWarning
{
	public XamlValidationErrorWrongContract(XamlDomObject domObject, string typeName, string contractName, string runtimeVer, string parseVer)
		: base(ErrorCode.WMC0151, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_WrongTypeContract, typeName, contractName, runtimeVer, parseVer);
	}

	public XamlValidationErrorWrongContract(XamlDomMember domMember, string typeName, string contractName, string runtimeVer, string parseVer)
		: base(ErrorCode.WMC0151, domMember)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_WrongMemberContract, typeName, contractName, runtimeVer, parseVer, domMember.Member.Name);
	}
}
