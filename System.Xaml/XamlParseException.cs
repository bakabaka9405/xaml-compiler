using System.Runtime.Serialization;
using MS.Internal.Xaml.Parser;

namespace System.Xaml;

[Serializable]
public class XamlParseException : XamlException
{
	internal XamlParseException(MeScanner meScanner, string message)
		: base(message, null, meScanner.LineNumber, meScanner.LinePosition)
	{
	}

	internal XamlParseException(XamlScanner xamlScanner, string message)
		: base(message, null, xamlScanner.LineNumber, xamlScanner.LinePosition)
	{
	}

	internal XamlParseException(int lineNumber, int linePosition, string message)
		: base(message, null, lineNumber, linePosition)
	{
	}

	public XamlParseException()
	{
	}

	public XamlParseException(string message)
		: base(message)
	{
	}

	public XamlParseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected XamlParseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
