using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class ForwardDeclaringNamespace
{
	public string Namespace { get; private set; }

	public List<string> ShortNameTypes { get; private set; }

	public ForwardDeclaringNamespace(string typePath, List<string> typeNames)
	{
		Namespace = typePath;
		ShortNameTypes = typeNames;
	}
}
