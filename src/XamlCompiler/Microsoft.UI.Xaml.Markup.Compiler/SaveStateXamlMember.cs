using System.Xml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class SaveStateXamlMember
{
	private const string XMLNAME_Name = "Name";

	private const string XMLNAME_DeclaringTypeFullName = "DeclaringTypeFullName";

	public string Name { get; set; }

	public string DeclaringTypeFullName { get; set; }

	public override string ToString()
	{
		return $"{DeclaringTypeFullName}.{Name}";
	}

	public SaveStateXamlMember(BindAssignment bindAssignment)
	{
		Name = bindAssignment.MemberName;
		DeclaringTypeFullName = bindAssignment.MemberDeclaringType.UnderlyingType.FullName;
	}

	public SaveStateXamlMember(XmlNode node)
	{
		XmlNode namedItem = node.Attributes.GetNamedItem("Name");
		if (namedItem != null)
		{
			Name = namedItem.Value;
		}
		namedItem = node.Attributes.GetNamedItem("DeclaringTypeFullName");
		if (namedItem != null)
		{
			DeclaringTypeFullName = namedItem.Value;
		}
	}

	public void WriteXmlElement(XmlWriter writer, string elementName)
	{
		writer.WriteStartElement(elementName);
		writer.WriteAttributeString("Name", Name);
		writer.WriteAttributeString("DeclaringTypeFullName", DeclaringTypeFullName);
		writer.WriteEndElement();
	}
}
