namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

internal interface IXamlDomNode
{
	string SourceFilePath { get; }

	int EndLineNumber { get; set; }

	int EndLinePosition { get; set; }

	bool IsSealed { get; }

	int StartLineNumber { get; set; }

	int StartLinePosition { get; set; }

	void Seal();
}
