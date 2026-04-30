using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class ConstructorArgumentAttribute : Attribute
{
	private string _argumentName;

	public string ArgumentName => _argumentName;

	public ConstructorArgumentAttribute(string argumentName)
	{
		_argumentName = argumentName;
	}
}
