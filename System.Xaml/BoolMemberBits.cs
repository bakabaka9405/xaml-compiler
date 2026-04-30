namespace System.Xaml;

internal enum BoolMemberBits
{
	ReadOnly = 1,
	WriteOnly = 2,
	Event = 4,
	Unknown = 8,
	Ambient = 16,
	ReadPublic = 32,
	WritePublic = 64,
	Default = 96,
	Directive = 96,
	AllValid = -65536
}
