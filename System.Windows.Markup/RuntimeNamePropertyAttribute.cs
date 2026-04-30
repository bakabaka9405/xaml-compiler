using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Class)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class RuntimeNamePropertyAttribute : Attribute
{
	private string _name;

	public string Name => _name;

	public RuntimeNamePropertyAttribute(string name)
	{
		_name = name;
	}
}
