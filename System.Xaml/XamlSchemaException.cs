using System.Runtime.Serialization;

namespace System.Xaml;

[Serializable]
public class XamlSchemaException : XamlException
{
	public XamlSchemaException()
	{
	}

	public XamlSchemaException(string message)
		: base(message, null)
	{
	}

	public XamlSchemaException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected XamlSchemaException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
