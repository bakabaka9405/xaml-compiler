using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class XmlnsPrefixAttribute : Attribute
{
	private string _xmlNamespace;

	private string _prefix;

	public string XmlNamespace => _xmlNamespace;

	public string Prefix => _prefix;

	public XmlnsPrefixAttribute(string xmlNamespace, string prefix)
	{
		if (xmlNamespace == null)
		{
			throw new ArgumentNullException("xmlNamespace");
		}
		if (prefix == null)
		{
			throw new ArgumentNullException("prefix");
		}
		_xmlNamespace = xmlNamespace;
		_prefix = prefix;
	}
}
