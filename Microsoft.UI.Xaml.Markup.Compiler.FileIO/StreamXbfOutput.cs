using System;
using System.IO;

namespace Microsoft.UI.Xaml.Markup.Compiler.FileIO;

internal class StreamXbfOutput : StreamImpl, IXamlStream
{
	private string _filePath;

	private MemoryStream _memoryStream;

	public StreamType StreamType => StreamType.Output;

	public StreamXbfOutput(string filePath)
	{
		_filePath = filePath;
		_underlyingStream = (_memoryStream = new MemoryStream());
	}

	private bool ContentChanged()
	{
		FileInfo fileInfo = new FileInfo(_filePath);
		if (!fileInfo.Exists || fileInfo.Length != _memoryStream.Length)
		{
			return true;
		}
		ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(File.ReadAllBytes(_filePath));
		ReadOnlySpan<byte> other = new ReadOnlySpan<byte>(_memoryStream.GetBuffer(), 0, (int)_memoryStream.Length);
		return !span.SequenceEqual(other);
	}

	protected override void Dispose(bool disposing)
	{
		if (_underlyingStream != null && ContentChanged())
		{
			using FileStream stream = new FileStream(_filePath, FileMode.Create, FileAccess.Write);
			_memoryStream.WriteTo(stream);
		}
		base.Dispose(disposing);
	}
}
