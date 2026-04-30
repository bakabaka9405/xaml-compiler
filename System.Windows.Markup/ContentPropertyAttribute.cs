using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class ContentPropertyAttribute : Attribute
{
	private string _name;

	public string Name => _name;

	public ContentPropertyAttribute()
	{
	}

	public ContentPropertyAttribute(string name)
	{
		_name = name;
	}
}
