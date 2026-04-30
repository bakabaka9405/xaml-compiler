using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Assembly)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class RootNamespaceAttribute : Attribute
{
	private string _nameSpace;

	public string Namespace => _nameSpace;

	public RootNamespaceAttribute(string nameSpace)
	{
		_nameSpace = nameSpace;
	}
}
