using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

internal class XamlDomReaderSettings : XamlReaderSettings
{
	public bool DoNotReorderMembers { get; set; }
}
