namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class FileNameAndContentPair
{
	public string FileName { get; private set; }

	public string Contents { get; private set; }

	public FileNameAndContentPair(string fileName, string contents)
	{
		FileName = fileName;
		Contents = contents;
	}
}
