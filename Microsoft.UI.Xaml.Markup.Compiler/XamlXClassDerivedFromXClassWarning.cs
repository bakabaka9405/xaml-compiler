using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlXClassDerivedFromXClassWarning : XamlCompileWarning
{
	public XamlXClassDerivedFromXClassWarning(XamlDomObject domObject, string derivedClass, string baseClass)
		: base(ErrorCode.WMC1508, domObject)
	{
		base.Message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XClassDerivesFromXClass, derivedClass, baseClass);
	}
}
