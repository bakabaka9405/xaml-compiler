using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class xPropertyInfo
{
	public XamlDomObject xPropertiesNode;

	public XamlDomObject xPropertiesRoot;

	public List<xProperty> xProperties;
}
