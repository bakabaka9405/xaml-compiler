namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class FixedSourceInfo
{
	public SourcePos StartOpeningTag = new SourcePos();

	public SourcePos StartClosingTag = new SourcePos();

	public SourcePos EndOpeningTag = new SourcePos();

	public SourcePos EndClosingTag = new SourcePos();

	public bool SelfClosing;

	public string UnprocessedType;
}
