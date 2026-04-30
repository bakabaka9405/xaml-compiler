using System.Xaml;
using System.Xml;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

internal static class XamlDomServices
{
	public static XamlDomNode Load(XamlReader xamlReader, string sourceFilePath)
	{
		IXamlLineInfo xamlLineInfo = xamlReader as IXamlLineInfo;
		XamlDomWriter xamlDomWriter = new XamlDomWriter(xamlReader.SchemaContext, sourceFilePath);
		XamlServices.Transform(xamlReader, xamlDomWriter);
		return xamlDomWriter.RootNode;
	}

	public static void Save(XamlDomObject rootObjectNode, string fileName)
	{
		XamlSchemaContext schemaContext = rootObjectNode.Type.SchemaContext;
		XamlDomReader xamlReader = new XamlDomReader(rootObjectNode, schemaContext);
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Indent = true;
		using XmlWriter xmlWriter = XmlWriter.Create(fileName, xmlWriterSettings);
		XamlXmlWriter xamlWriter = new XamlXmlWriter(xmlWriter, schemaContext);
		XamlServices.Transform(xamlReader, xamlWriter);
	}
}
