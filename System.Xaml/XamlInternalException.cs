using System.Runtime.Serialization;

namespace System.Xaml;

[Serializable]
public class XamlInternalException : XamlException
{
	private const string MessagePrefix = "Internal XAML system error: ";

	public XamlInternalException()
		: base("Internal XAML system error: ")
	{
	}

	public XamlInternalException(string message)
		: base("Internal XAML system error: " + message, null)
	{
	}

	public XamlInternalException(string message, Exception innerException)
		: base("Internal XAML system error: " + message, innerException)
	{
	}

	protected XamlInternalException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
