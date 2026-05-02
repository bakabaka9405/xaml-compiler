using System.Xaml;
using System.Xml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class SaveStateXamlType
{
	private const string XMLNAME_FullName = "FullName";

	public string FullName { get; set; }

	public override string ToString()
	{
		return FullName;
	}

	public SaveStateXamlType(XamlType type)
	{
		FullName = type.UnderlyingType.FullName;
	}

	public SaveStateXamlType(XmlNode node)
	{
		XmlNode namedItem = node.Attributes.GetNamedItem("FullName");
		if (namedItem != null)
		{
			FullName = namedItem.Value;
		}
	}

	public void WriteXmlElement(XmlWriter writer, string elementName)
	{
		writer.WriteStartElement(elementName);
		writer.WriteAttributeString("FullName", FullName);
		writer.WriteEndElement();
	}
}
