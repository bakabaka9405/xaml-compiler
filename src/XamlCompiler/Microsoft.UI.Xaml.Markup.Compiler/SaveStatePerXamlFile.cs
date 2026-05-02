using System.Collections.Generic;
using System.Xml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class SaveStatePerXamlFile
{
	private const string XMLNAME_XamlFileName = "XamlFileName";

	private const string XMLNAME_XamlFileTimeAtLastCompileInTicks = "XamlFileTimeAtLastCompileInTicks";

	private const string XMLNAME_ClassFullName = "ClassFullName";

	private const string XMLNAME_GeneratedCodePathPrefix = "GeneratedCodePathPrefix";

	private const string XMLNAME_BindingObservableVectorTypes = "BindingObservableVectorTypes";

	private const string XMLNAME_BindingObservableMapTypes = "BindingObservableMapTypes";

	private const string XMLNAME_XamlType = "XamlType";

	private const string XMLNAME_BindingSetters = "BindingSetters";

	private const string XMLNAME_Member = "XamlMember";

	private const string XMLNAME_HasBoundEventAssignments = "HasBoundEventAssignments";

	public string FileName { get; private set; }

	public long XamlFileTimeAtLastCompile { get; set; }

	public string ClassFullName { get; set; }

	public Dictionary<string, SaveStateXamlMember> BindingSetters { get; set; }

	public Dictionary<string, SaveStateXamlType> BindingObservableVectorTypes { get; set; }

	public Dictionary<string, SaveStateXamlType> BindingObservableMapTypes { get; set; }

	public bool HasBoundEventAssignments { get; set; }

	public string GeneratedCodeFilePathPrefix { get; set; }

	private SaveStatePerXamlFile()
	{
		BindingSetters = new Dictionary<string, SaveStateXamlMember>();
		BindingObservableVectorTypes = new Dictionary<string, SaveStateXamlType>();
		BindingObservableMapTypes = new Dictionary<string, SaveStateXamlType>();
	}

	public SaveStatePerXamlFile(string fileName)
		: this()
	{
		FileName = fileName;
	}

	public SaveStatePerXamlFile(XmlNode node)
		: this()
	{
		XmlNode namedItem = node.Attributes.GetNamedItem("XamlFileName");
		if (namedItem != null)
		{
			FileName = namedItem.Value;
		}
		namedItem = node.Attributes.GetNamedItem("XamlFileTimeAtLastCompileInTicks");
		if (namedItem != null)
		{
			long result = 0L;
			if (long.TryParse(namedItem.Value, out result))
			{
				XamlFileTimeAtLastCompile = result;
			}
			else
			{
				XamlFileTimeAtLastCompile = 0L;
			}
		}
		namedItem = node.Attributes.GetNamedItem("ClassFullName");
		if (namedItem != null)
		{
			ClassFullName = namedItem.Value;
		}
		namedItem = node.Attributes.GetNamedItem("GeneratedCodePathPrefix");
		if (namedItem != null)
		{
			GeneratedCodeFilePathPrefix = namedItem.Value;
		}
		namedItem = node.Attributes.GetNamedItem("HasBoundEventAssignments");
		if (namedItem != null)
		{
			bool result2 = false;
			if (bool.TryParse(namedItem.Value, out result2))
			{
				HasBoundEventAssignments = result2;
			}
			else
			{
				HasBoundEventAssignments = false;
			}
		}
		foreach (XmlNode childNode in node.ChildNodes)
		{
			switch (childNode.Name)
			{
			case "BindingObservableVectorTypes":
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					SaveStateXamlType saveStateXamlType2 = new SaveStateXamlType(childNode2);
					BindingObservableVectorTypes.Add(saveStateXamlType2.ToString(), saveStateXamlType2);
				}
				break;
			case "BindingObservableMapTypes":
				foreach (XmlNode childNode3 in childNode.ChildNodes)
				{
					SaveStateXamlType saveStateXamlType = new SaveStateXamlType(childNode3);
					BindingObservableMapTypes.Add(saveStateXamlType.ToString(), saveStateXamlType);
				}
				break;
			case "BindingSetters":
				foreach (XmlNode childNode4 in childNode.ChildNodes)
				{
					SaveStateXamlMember saveStateXamlMember = new SaveStateXamlMember(childNode4);
					BindingSetters.Add(saveStateXamlMember.ToString(), saveStateXamlMember);
				}
				break;
			}
		}
	}

	public void WriteXmlElement(XmlWriter writer, string elementName)
	{
		writer.WriteStartElement(elementName);
		writer.WriteAttributeString("XamlFileName", FileName);
		writer.WriteAttributeString("ClassFullName", ClassFullName);
		writer.WriteAttributeString("GeneratedCodePathPrefix", GeneratedCodeFilePathPrefix);
		writer.WriteAttributeString("XamlFileTimeAtLastCompileInTicks", XamlFileTimeAtLastCompile.ToString());
		writer.WriteAttributeString("HasBoundEventAssignments", HasBoundEventAssignments.ToString());
		if (BindingObservableVectorTypes.Values.Count > 0)
		{
			writer.WriteStartElement("BindingObservableVectorTypes");
			foreach (SaveStateXamlType value in BindingObservableVectorTypes.Values)
			{
				value.WriteXmlElement(writer, "XamlType");
			}
			writer.WriteEndElement();
		}
		if (BindingObservableMapTypes.Values.Count > 0)
		{
			writer.WriteStartElement("BindingObservableMapTypes");
			foreach (SaveStateXamlType value2 in BindingObservableMapTypes.Values)
			{
				value2.WriteXmlElement(writer, "XamlType");
			}
			writer.WriteEndElement();
		}
		if (BindingSetters.Values.Count > 0)
		{
			writer.WriteStartElement("BindingSetters");
			foreach (SaveStateXamlMember value3 in BindingSetters.Values)
			{
				value3.WriteXmlElement(writer, "XamlMember");
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}
}
