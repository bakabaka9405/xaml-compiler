using System.Collections.Generic;

namespace System.Xaml;

public interface IXamlNamespaceResolver
{
	string GetNamespace(string prefix);

	IEnumerable<NamespaceDeclaration> GetNamespacePrefixes();
}
