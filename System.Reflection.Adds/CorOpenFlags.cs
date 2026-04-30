namespace System.Reflection.Adds;

[Flags]
public enum CorOpenFlags
{
	Read = 0,
	Write = 1,
	ReadWriteMask = 1,
	CopyMemory = 2,
	ReadOnly = 0x10,
	TakeOwnership = 0x20,
	NoTypeLib = 0x80,
	NoTransform = 0x1000
}
