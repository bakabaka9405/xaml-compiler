namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class FileNameAndChecksumPair
{
	public string FileName { get; private set; }

	public string Checksum { get; private set; }

	public FileNameAndChecksumPair(string fileName, string contents)
	{
		FileName = fileName;
		Checksum = contents;
	}
}
