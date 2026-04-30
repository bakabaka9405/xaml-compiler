using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class XmlnsDefinitionAttribute : Attribute
{
	private string _xmlNamespace;

	private string _clrNamespace;

	private string _assemblyName;

	public string XmlNamespace => _xmlNamespace;

	public string ClrNamespace => _clrNamespace;

	public string AssemblyName
	{
		get
		{
			return _assemblyName;
		}
		set
		{
			_assemblyName = value;
		}
	}

	public XmlnsDefinitionAttribute(string xmlNamespace, string clrNamespace)
	{
		if (xmlNamespace == null)
		{
			throw new ArgumentNullException("xmlNamespace");
		}
		if (clrNamespace == null)
		{
			throw new ArgumentNullException("clrNamespace");
		}
		_xmlNamespace = xmlNamespace;
		_clrNamespace = clrNamespace;
	}
}
