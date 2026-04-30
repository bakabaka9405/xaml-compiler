namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class MarkupExtensionBracketCharactersAttribute : Attribute
{
	public char OpeningBracket { get; }

	public char ClosingBracket { get; }

	public MarkupExtensionBracketCharactersAttribute(char openingBracket, char closingBracket)
	{
		OpeningBracket = openingBracket;
		ClosingBracket = closingBracket;
	}
}
