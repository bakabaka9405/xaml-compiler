using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

public class XamlDomReaderSettings : XamlReaderSettings
{
	public bool DoNotReorderMembers { get; set; }
}
