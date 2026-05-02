using System;
using System.IO;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class BuildTaskFileService
{
	private string langExtension;

	public virtual bool HasIdeHost => false;

	public virtual bool IsRealBuild => true;

	public BuildTaskFileService(string languageExtension)
	{
		langExtension = languageExtension;
	}

	public virtual TextReader GetFileContents(string srcFile)
	{
		return new StreamReader(File.OpenRead(srcFile));
	}

	public virtual DateTime GetLastChangeTime(string srcFile)
	{
		return File.GetLastWriteTime(srcFile);
	}

	public bool FileExists(string srcFile)
	{
		return File.Exists(srcFile);
	}

	public virtual void DeleteFile(string srcFile)
	{
		if (File.Exists(srcFile))
		{
			File.Delete(srcFile);
		}
	}

	public void WriteFile(string fileContents, string destinationFile)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
		using StreamWriter streamWriter = new StreamWriter(destinationFile, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
		streamWriter.WriteLine(fileContents);
	}
}
