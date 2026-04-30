using System.Collections.Generic;
using MS.Internal.Xaml.Context;

namespace MS.Internal.Xaml.Parser;

internal class BracketModeParseParameters
{
	internal int CurrentConstructorParam { get; set; }

	internal int MaxConstructorParams { get; set; }

	internal bool IsConstructorParsingMode { get; set; }

	internal bool IsBracketEscapeMode { get; set; }

	internal Stack<char> BracketCharacterStack { get; set; }

	internal BracketModeParseParameters(XamlParserContext context)
	{
		CurrentConstructorParam = 0;
		IsBracketEscapeMode = false;
		BracketCharacterStack = new Stack<char>();
		if (context.CurrentLongestConstructorOfMarkupExtension != null)
		{
			IsConstructorParsingMode = context.CurrentLongestConstructorOfMarkupExtension.Length != 0;
			MaxConstructorParams = context.CurrentLongestConstructorOfMarkupExtension.Length;
		}
		else
		{
			IsConstructorParsingMode = false;
			MaxConstructorParams = 0;
		}
	}
}
