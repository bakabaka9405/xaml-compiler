using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

public class XamlDomNamespace : XamlDomNode
{
	private NamespaceDeclaration namespaceDeclaration;

	public NamespaceDeclaration NamespaceDeclaration
	{
		get
		{
			return namespaceDeclaration;
		}
		set
		{
			CheckSealed();
			namespaceDeclaration = value;
		}
	}

	public XamlDomNamespace(NamespaceDeclaration namespaceDeclaration, string sourceFilePath)
		: base(sourceFilePath)
	{
		NamespaceDeclaration = namespaceDeclaration;
	}
}
