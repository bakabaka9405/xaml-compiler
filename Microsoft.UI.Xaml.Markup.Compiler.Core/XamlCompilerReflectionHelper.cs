using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xaml;
using System.Xml;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;
using Microsoft.UI.Xaml.Markup.Compiler.XBF;

namespace Microsoft.UI.Xaml.Markup.Compiler.Core;

internal class XamlCompilerReflectionHelper
{
	private static string testSourceFile = string.Empty;

	public XamlDomObject CreateCompilerDomRoot(XamlReader xamlReader)
	{
		XamlDomWriter xamlDomWriter = new XamlDomWriter(xamlReader.SchemaContext, testSourceFile);
		XamlServices.Transform(xamlReader, xamlDomWriter, closeWriter: true);
		return xamlDomWriter.RootNode as XamlDomObject;
	}

	public XamlDomObject CreateDomRoot(string xamlString, XamlSchemaContext schema, Assembly localAssembly)
	{
		TextReader input = new StringReader(xamlString);
		XmlReader xmlReader = XmlReader.Create(input);
		XamlXmlReaderSettings xamlXmlReaderSettings = new XamlXmlReaderSettings();
		xamlXmlReaderSettings.LocalAssembly = localAssembly;
		xamlXmlReaderSettings.AllowProtectedMembersOnRoot = true;
		xamlXmlReaderSettings.ProvideLineInfo = true;
		XamlXmlReader xamlReader = new XamlXmlReader(xmlReader, schema, xamlXmlReaderSettings);
		XamlDomWriter xamlDomWriter = new XamlDomWriter(schema, testSourceFile);
		XamlServices.Transform(xamlReader, xamlDomWriter, closeWriter: true);
		return xamlDomWriter.RootNode as XamlDomObject;
	}

	public static IEnumerable<IXbfFileNameInfo> CreateXbfFilenameInfoArray(string[] filenames)
	{
		if (filenames.Length % 3 != 0)
		{
			throw new ArgumentException("Array of filenames must be a multiple of 3 in length");
		}
		int num = filenames.Length / 3;
		List<IXbfFileNameInfo> list = new List<IXbfFileNameInfo>();
		for (int i = 0; i < num; i++)
		{
			int num2 = i * 3;
			list.Add(new XbfFileNameInfo(filenames[num2], filenames[num2], filenames[num2 + 1], filenames[num2 + 2]));
		}
		return list;
	}
}
