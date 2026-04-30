namespace System.Xaml;

[Flags]
internal enum BoolTypeBits
{
	Constructible = 1,
	XmlData = 2,
	MarkupExtension = 4,
	Nullable = 8,
	NameScope = 0x10,
	ConstructionRequiresArguments = 0x20,
	Public = 0x40,
	Unknown = 0x100,
	TrimSurroundingWhitespace = 0x1000,
	WhitespaceSignificantCollection = 0x2000,
	UsableDuringInitialization = 0x4000,
	Ambient = 0x8000,
	Default = 0x49,
	AllValid = -65536
}
