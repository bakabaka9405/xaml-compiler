using System.IO;
using System.Text;

namespace Antlr4.Runtime;

public class AntlrFileStream : AntlrInputStream
{
	protected internal string fileName;

	public override string SourceName => fileName;

	public AntlrFileStream(string fileName)
		: this(fileName, null)
	{
	}

	public AntlrFileStream(string fileName, Encoding encoding)
	{
		this.fileName = fileName;
		Load(fileName, encoding);
	}

	public virtual void Load(string fileName, Encoding encoding)
	{
		if (fileName != null)
		{
			string text = ((encoding == null) ? File.ReadAllText(fileName) : File.ReadAllText(fileName, encoding));
			data = text.ToCharArray();
			n = data.Length;
		}
	}
}
