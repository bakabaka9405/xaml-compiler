namespace System.Reflection.Adds;

internal interface ITypeReference : ITypeProxy
{
	Token TypeRefToken { get; }

	string RawName { get; }

	Token ResolutionScope { get; }

	string FullName { get; }

	Module DeclaringScope { get; }
}
