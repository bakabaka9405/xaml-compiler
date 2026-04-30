using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class DependsOnAttribute : Attribute
{
	private string _name;

	public override object TypeId => this;

	public string Name => _name;

	public DependsOnAttribute(string name)
	{
		_name = name;
	}
}
