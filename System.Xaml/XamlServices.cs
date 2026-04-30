using System.Globalization;
using System.IO;
using System.Xml;

namespace System.Xaml;

public static class XamlServices
{
	public static object Parse(string xaml)
	{
		if (xaml == null)
		{
			throw new ArgumentNullException("xaml");
		}
		StringReader input = new StringReader(xaml);
		using XmlReader xmlReader = XmlReader.Create(input);
		XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
		return Load(xamlReader);
	}

	public static object Load(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		using XmlReader xmlReader = XmlReader.Create(fileName);
		XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
		return Load(xamlReader);
	}

	public static object Load(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using XmlReader xmlReader = XmlReader.Create(stream);
		XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
		return Load(xamlReader);
	}

	public static object Load(TextReader textReader)
	{
		if (textReader == null)
		{
			throw new ArgumentNullException("textReader");
		}
		using XmlReader xmlReader = XmlReader.Create(textReader);
		XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
		return Load(xamlReader);
	}

	public static object Load(XmlReader xmlReader)
	{
		if (xmlReader == null)
		{
			throw new ArgumentNullException("xmlReader");
		}
		using XamlXmlReader xamlReader = new XamlXmlReader(xmlReader);
		return Load(xamlReader);
	}

	public static object Load(XamlReader xamlReader)
	{
		if (xamlReader == null)
		{
			throw new ArgumentNullException("xamlReader");
		}
		XamlObjectWriter xamlObjectWriter = new XamlObjectWriter(xamlReader.SchemaContext);
		Transform(xamlReader, xamlObjectWriter);
		return xamlObjectWriter.Result;
	}

	public static void Transform(XamlReader xamlReader, XamlWriter xamlWriter)
	{
		Transform(xamlReader, xamlWriter, closeWriter: true);
	}

	public static void Transform(XamlReader xamlReader, XamlWriter xamlWriter, bool closeWriter)
	{
		if (xamlReader == null)
		{
			throw new ArgumentNullException("xamlReader");
		}
		if (xamlWriter == null)
		{
			throw new ArgumentNullException("xamlWriter");
		}
		IXamlLineInfo xamlLineInfo = xamlReader as IXamlLineInfo;
		IXamlLineInfoConsumer xamlLineInfoConsumer = xamlWriter as IXamlLineInfoConsumer;
		bool flag = false;
		if (xamlLineInfo != null && xamlLineInfo.HasLineInfo && xamlLineInfoConsumer != null && xamlLineInfoConsumer.ShouldProvideLineInfo)
		{
			flag = true;
		}
		while (xamlReader.Read())
		{
			if (flag && xamlLineInfo.LineNumber != 0)
			{
				xamlLineInfoConsumer.SetLineInfo(xamlLineInfo.LineNumber, xamlLineInfo.LinePosition);
			}
			xamlWriter.WriteNode(xamlReader);
		}
		if (closeWriter)
		{
			xamlWriter.Close();
		}
	}

	public static string Save(object instance)
	{
		StringWriter stringWriter = new StringWriter(CultureInfo.CurrentCulture);
		using (XmlWriter writer = XmlWriter.Create(stringWriter, new XmlWriterSettings
		{
			Indent = true,
			OmitXmlDeclaration = true
		}))
		{
			Save(writer, instance);
		}
		return stringWriter.ToString();
	}

	public static void Save(string fileName, object instance)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentException(SR.Get("StringIsNullOrEmpty"), "fileName");
		}
		using XmlWriter xmlWriter = XmlWriter.Create(fileName, new XmlWriterSettings
		{
			Indent = true,
			OmitXmlDeclaration = true
		});
		Save(xmlWriter, instance);
		xmlWriter.Flush();
	}

	public static void Save(Stream stream, object instance)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		using XmlWriter xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings
		{
			Indent = true,
			OmitXmlDeclaration = true
		});
		Save(xmlWriter, instance);
		xmlWriter.Flush();
	}

	public static void Save(TextWriter writer, object instance)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		using XmlWriter xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings
		{
			Indent = true,
			OmitXmlDeclaration = true
		});
		Save(xmlWriter, instance);
		xmlWriter.Flush();
	}

	public static void Save(XmlWriter writer, object instance)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		using XamlXmlWriter writer2 = new XamlXmlWriter(writer, new XamlSchemaContext());
		Save(writer2, instance);
	}

	public static void Save(XamlWriter writer, object instance)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		XamlObjectReader xamlReader = new XamlObjectReader(instance, writer.SchemaContext);
		Transform(xamlReader, writer);
	}
}
