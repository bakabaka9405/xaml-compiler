using System;
using System.Runtime.Serialization;

namespace Microsoft.UI.Xaml.Markup.Compiler;

[Serializable]
internal sealed class InternalErrorException : Exception
{
	internal InternalErrorException()
	{
	}

	internal InternalErrorException(string message)
		: base("Internal MSBuild Error: " + message)
	{
		ShowAssertDialog(showAssert: true);
	}

	internal InternalErrorException(string message, bool showAssert)
		: base("Internal MSBuild Error: " + message)
	{
		ShowAssertDialog(showAssert);
	}

	internal InternalErrorException(string message, Exception innerException)
		: base("Internal MSBuild Error: " + message, innerException)
	{
	}

	private InternalErrorException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private void ShowAssertDialog(bool showAssert)
	{
	}
}
