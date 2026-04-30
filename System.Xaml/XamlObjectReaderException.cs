using System.Runtime.Serialization;

namespace System.Xaml;

[Serializable]
public class XamlObjectReaderException : XamlException
{
	public XamlObjectReaderException()
	{
	}

	public XamlObjectReaderException(string message)
		: base(message)
	{
	}

	public XamlObjectReaderException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected XamlObjectReaderException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
