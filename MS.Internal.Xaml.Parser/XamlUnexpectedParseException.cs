using System;
using System.Runtime.Serialization;
using System.Xaml;

namespace MS.Internal.Xaml.Parser;

[Serializable]
internal class XamlUnexpectedParseException : XamlParseException
{
	public XamlUnexpectedParseException()
	{
	}

	public XamlUnexpectedParseException(XamlScanner xamlScanner, ScannerNodeType nodetype, string parseRule)
		: base(xamlScanner, SR.Get("UnexpectedNodeType", nodetype.ToString(), parseRule))
	{
	}

	protected XamlUnexpectedParseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
