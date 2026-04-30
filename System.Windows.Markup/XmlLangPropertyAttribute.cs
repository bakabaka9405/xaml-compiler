using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class XmlLangPropertyAttribute : Attribute
{
	private string _name;

	public string Name => _name;

	public XmlLangPropertyAttribute(string name)
	{
		_name = name;
	}
}
