using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class xPropertyInfo
{
	public XamlDomObject xPropertiesNode;

	public XamlDomObject xPropertiesRoot;

	public List<xProperty> xProperties;
}
