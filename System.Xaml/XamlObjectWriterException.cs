using System.Runtime.Serialization;

namespace System.Xaml;

[Serializable]
public class XamlObjectWriterException : XamlException
{
	public XamlObjectWriterException()
	{
	}

	public XamlObjectWriterException(string message)
		: base(message)
	{
	}

	public XamlObjectWriterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected XamlObjectWriterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
