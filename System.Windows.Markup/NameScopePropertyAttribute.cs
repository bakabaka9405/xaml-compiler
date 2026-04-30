using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class NameScopePropertyAttribute : Attribute
{
	private string _name;

	private Type _type;

	public string Name => _name;

	public Type Type => _type;

	public NameScopePropertyAttribute(string name)
	{
		_name = name;
	}

	public NameScopePropertyAttribute(string name, Type type)
	{
		_name = name;
		_type = type;
	}
}
