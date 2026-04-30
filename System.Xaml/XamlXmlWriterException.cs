using System.Runtime.Serialization;

namespace System.Xaml;

[Serializable]
public class XamlXmlWriterException : XamlException
{
	public XamlXmlWriterException()
	{
	}

	public XamlXmlWriterException(string message)
		: base(message)
	{
	}

	public XamlXmlWriterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected XamlXmlWriterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
