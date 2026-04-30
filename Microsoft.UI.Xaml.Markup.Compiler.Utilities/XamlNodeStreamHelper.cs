using System.IO;
using System.Xaml;
using System.Xml;

namespace Microsoft.UI.Xaml.Markup.Compiler.Utilities;

internal class XamlNodeStreamHelper
{
	public static string ReadXClassFromXamlFileStream(TextReader fileStream, XamlSchemaContext schemaContext)
	{
		using XmlReader xmlReader = XmlReader.Create(fileStream);
		XamlXmlReader xamlReader = new XamlXmlReader(xmlReader, schemaContext);
		return ReadXClassFromXamlReader(xamlReader);
	}

	public static string ReadXClassFromXamlReader(XamlXmlReader xamlReader)
	{
		int num = 0;
		bool flag = false;
		while (xamlReader.Read())
		{
			switch (xamlReader.NodeType)
			{
			case XamlNodeType.StartObject:
				if (!xamlReader.Type.IsMarkupExtension)
				{
					num++;
					if (num > 1)
					{
						return null;
					}
				}
				break;
			case XamlNodeType.StartMember:
				if (xamlReader.Member == XamlLanguage.Class)
				{
					flag = true;
				}
				break;
			case XamlNodeType.Value:
				if (flag)
				{
					return (string)xamlReader.Value;
				}
				break;
			}
		}
		return null;
	}
}
