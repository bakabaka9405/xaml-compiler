namespace MS.Internal.Xaml.Context;

internal enum FixupType
{
	MarkupExtensionFirstRun,
	MarkupExtensionRerun,
	PropertyValue,
	ObjectInitializationValue,
	UnresolvedChildren
}
