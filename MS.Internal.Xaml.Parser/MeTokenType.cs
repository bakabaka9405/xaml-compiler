namespace MS.Internal.Xaml.Parser;

internal enum MeTokenType
{
	None = 0,
	Open = 123,
	Close = 125,
	EqualSign = 61,
	Comma = 44,
	TypeName = 45,
	PropertyName = 46,
	String = 47,
	QuotedMarkupExtension = 48
}
