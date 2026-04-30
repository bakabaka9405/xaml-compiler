namespace System.Xaml;

public enum XamlNodeType : byte
{
	None,
	StartObject,
	GetObject,
	EndObject,
	StartMember,
	EndMember,
	Value,
	NamespaceDeclaration
}
