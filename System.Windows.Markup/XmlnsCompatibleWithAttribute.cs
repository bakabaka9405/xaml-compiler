using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class XmlnsCompatibleWithAttribute : Attribute
{
	private string _oldNamespace;

	private string _newNamespace;

	public string OldNamespace => _oldNamespace;

	public string NewNamespace => _newNamespace;

	public XmlnsCompatibleWithAttribute(string oldNamespace, string newNamespace)
	{
		if (oldNamespace == null)
		{
			throw new ArgumentNullException("oldNamespace");
		}
		if (newNamespace == null)
		{
			throw new ArgumentNullException("newNamespace");
		}
		_oldNamespace = oldNamespace;
		_newNamespace = newNamespace;
	}
}
