using System.IO;

namespace Microsoft.UI.Xaml.Markup.Compiler.FileIO;

internal class StreamXamlInput : StreamImpl, IXamlStream
{
	public StreamType StreamType => StreamType.Input;

	public StreamXamlInput(string filePath)
	{
		_underlyingStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
	}
}
