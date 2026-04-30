using System.Runtime.Serialization;
using System.Security;

namespace System.Xaml;

[Serializable]
public class XamlException : Exception
{
	public override string Message
	{
		get
		{
			if (LineNumber != 0)
			{
				if (LinePosition != 0)
				{
					return SR.Get("LineNumberAndPosition", base.Message, LineNumber, LinePosition);
				}
				return SR.Get("LineNumberOnly", base.Message, LineNumber);
			}
			return base.Message;
		}
	}

	public int LineNumber { get; protected set; }

	public int LinePosition { get; protected set; }

	public XamlException(string message, Exception innerException, int lineNumber, int linePosition)
		: base(message, innerException)
	{
		LineNumber = lineNumber;
		LinePosition = linePosition;
	}

	public XamlException(string message, Exception innerException)
		: base(message, innerException)
	{
		if (innerException is XamlException ex)
		{
			LineNumber = ex.LineNumber;
			LinePosition = ex.LinePosition;
		}
	}

	internal void SetLineInfo(int lineNumber, int linePosition)
	{
		LineNumber = lineNumber;
		LinePosition = linePosition;
	}

	public XamlException()
	{
	}

	public XamlException(string message)
		: base(message)
	{
	}

	protected XamlException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		LineNumber = info.GetInt32("Line");
		LinePosition = info.GetInt32("Offset");
	}

	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (info == null)
		{
			throw new ArgumentNullException("info");
		}
		info.AddValue("Line", LineNumber);
		info.AddValue("Offset", LinePosition);
		base.GetObjectData(info, context);
	}
}
