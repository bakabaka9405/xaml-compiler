using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class CompiledBindingException : Exception
{
	public int StartCharacterPosition { get; }

	public CompiledBindingException(string exceptionMessage, int startCharacterPosition)
		: base(exceptionMessage)
	{
		StartCharacterPosition = startCharacterPosition;
	}
}
