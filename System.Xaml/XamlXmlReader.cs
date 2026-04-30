using System.Collections.Generic;
using System.IO;
using System.Xaml.Schema;
using System.Xml;
using MS.Internal.Xaml;
using MS.Internal.Xaml.Context;
using MS.Internal.Xaml.Parser;

namespace System.Xaml;

public class XamlXmlReader : XamlReader, IXamlLineInfo
{
	private XamlParserContext _context;

	private IEnumerator<XamlNode> _nodeStream;

	private XamlNode _current;

	private LineInfo _currentLineInfo;

	private XamlNode _endOfStreamNode;

	private XamlXmlReaderSettings _mergedSettings;

	public override XamlNodeType NodeType => _current.NodeType;

	public override bool IsEof => _current.IsEof;

	public override NamespaceDeclaration Namespace => _current.NamespaceDeclaration;

	public override XamlType Type => _current.XamlType;

	public override object Value => _current.Value;

	public override XamlMember Member => _current.Member;

	public override XamlSchemaContext SchemaContext => _context.SchemaContext;

	public bool HasLineInfo => _mergedSettings.ProvideLineInfo;

	public int LineNumber => _currentLineInfo.LineNumber;

	public int LinePosition => _currentLineInfo.LinePosition;

	public XamlXmlReader(XmlReader xmlReader)
	{
		if (xmlReader == null)
		{
			throw new ArgumentNullException("xmlReader");
		}
		Initialize(xmlReader, null, null);
	}

	public XamlXmlReader(XmlReader xmlReader, XamlXmlReaderSettings settings)
	{
		if (xmlReader == null)
		{
			throw new ArgumentNullException("xmlReader");
		}
		Initialize(xmlReader, null, settings);
	}

	public XamlXmlReader(XmlReader xmlReader, XamlSchemaContext schemaContext)
	{
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		if (xmlReader == null)
		{
			throw new ArgumentNullException("xmlReader");
		}
		Initialize(xmlReader, schemaContext, null);
	}

	public XamlXmlReader(XmlReader xmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
	{
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		if (xmlReader == null)
		{
			throw new ArgumentNullException("xmlReader");
		}
		Initialize(xmlReader, schemaContext, settings);
	}

	public XamlXmlReader(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		Initialize(CreateXmlReader(fileName, null), null, null);
	}

	public XamlXmlReader(string fileName, XamlXmlReaderSettings settings)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		Initialize(CreateXmlReader(fileName, settings), null, settings);
	}

	public XamlXmlReader(string fileName, XamlSchemaContext schemaContext)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		Initialize(CreateXmlReader(fileName, null), schemaContext, null);
	}

	public XamlXmlReader(string fileName, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		Initialize(CreateXmlReader(fileName, settings), schemaContext, settings);
	}

	private XmlReader CreateXmlReader(string fileName, XamlXmlReaderSettings settings)
	{
		bool closeInput = settings?.CloseInput ?? true;
		return XmlReader.Create(fileName, new XmlReaderSettings
		{
			CloseInput = closeInput,
			DtdProcessing = DtdProcessing.Prohibit
		});
	}

	public XamlXmlReader(Stream stream)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		Initialize(CreateXmlReader(stream, null), null, null);
	}

	public XamlXmlReader(Stream stream, XamlXmlReaderSettings settings)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		Initialize(CreateXmlReader(stream, settings), null, settings);
	}

	public XamlXmlReader(Stream stream, XamlSchemaContext schemaContext)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		Initialize(CreateXmlReader(stream, null), schemaContext, null);
	}

	public XamlXmlReader(Stream stream, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		Initialize(CreateXmlReader(stream, settings), schemaContext, settings);
	}

	private XmlReader CreateXmlReader(Stream stream, XamlXmlReaderSettings settings)
	{
		bool closeInput = settings?.CloseInput ?? false;
		return XmlReader.Create(stream, new XmlReaderSettings
		{
			CloseInput = closeInput,
			DtdProcessing = DtdProcessing.Prohibit
		});
	}

	public XamlXmlReader(TextReader textReader)
	{
		if (textReader == null)
		{
			throw new ArgumentNullException("textReader");
		}
		Initialize(CreateXmlReader(textReader, null), null, null);
	}

	public XamlXmlReader(TextReader textReader, XamlXmlReaderSettings settings)
	{
		if (textReader == null)
		{
			throw new ArgumentNullException("textReader");
		}
		Initialize(CreateXmlReader(textReader, settings), null, settings);
	}

	public XamlXmlReader(TextReader textReader, XamlSchemaContext schemaContext)
	{
		if (textReader == null)
		{
			throw new ArgumentNullException("textReader");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		Initialize(CreateXmlReader(textReader, null), schemaContext, null);
	}

	public XamlXmlReader(TextReader textReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
	{
		if (textReader == null)
		{
			throw new ArgumentNullException("textReader");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		Initialize(CreateXmlReader(textReader, settings), schemaContext, settings);
	}

	private XmlReader CreateXmlReader(TextReader textReader, XamlXmlReaderSettings settings)
	{
		bool closeInput = settings?.CloseInput ?? false;
		return XmlReader.Create(textReader, new XmlReaderSettings
		{
			CloseInput = closeInput,
			DtdProcessing = DtdProcessing.Prohibit
		});
	}

	private void Initialize(XmlReader givenXmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
	{
		_mergedSettings = ((settings == null) ? new XamlXmlReaderSettings() : new XamlXmlReaderSettings(settings));
		XmlReader xmlReader;
		if (!_mergedSettings.SkipXmlCompatibilityProcessing)
		{
			XmlCompatibilityReader xmlCompatibilityReader = new XmlCompatibilityReader(givenXmlReader, IsXmlNamespaceSupported);
			xmlCompatibilityReader.Normalization = true;
			xmlReader = xmlCompatibilityReader;
		}
		else
		{
			xmlReader = givenXmlReader;
		}
		if (!string.IsNullOrEmpty(xmlReader.BaseURI))
		{
			_mergedSettings.BaseUri = new Uri(xmlReader.BaseURI);
		}
		if (xmlReader.XmlSpace == XmlSpace.Preserve)
		{
			_mergedSettings.XmlSpacePreserve = true;
		}
		if (!string.IsNullOrEmpty(xmlReader.XmlLang))
		{
			_mergedSettings.XmlLang = xmlReader.XmlLang;
		}
		IXmlNamespaceResolver xmlNamespaceResolver = xmlReader as IXmlNamespaceResolver;
		Dictionary<string, string> dictionary = null;
		if (xmlNamespaceResolver != null)
		{
			IDictionary<string, string> namespacesInScope = xmlNamespaceResolver.GetNamespacesInScope(XmlNamespaceScope.Local);
			if (namespacesInScope != null)
			{
				foreach (KeyValuePair<string, string> item in namespacesInScope)
				{
					if (dictionary == null)
					{
						dictionary = new Dictionary<string, string>();
					}
					dictionary[item.Key] = item.Value;
				}
			}
		}
		if (schemaContext == null)
		{
			schemaContext = new XamlSchemaContext();
		}
		_endOfStreamNode = new XamlNode(XamlNode.InternalNodeType.EndOfStream);
		_context = new XamlParserContext(schemaContext, _mergedSettings.LocalAssembly);
		_context.AllowProtectedMembersOnRoot = _mergedSettings.AllowProtectedMembersOnRoot;
		_context.AddNamespacePrefix("xml", "http://www.w3.org/XML/1998/namespace");
		Func<string, string> xmlNamespaceResolver2 = xmlReader.LookupNamespace;
		_context.XmlNamespaceResolver = xmlNamespaceResolver2;
		XamlScanner scanner = new XamlScanner(_context, xmlReader, _mergedSettings);
		XamlPullParser parser = new XamlPullParser(_context, scanner, _mergedSettings);
		_nodeStream = new NodeStreamSorter(_context, parser, _mergedSettings, dictionary);
		_current = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
		_currentLineInfo = new LineInfo(0, 0);
	}

	public override bool Read()
	{
		ThrowIfDisposed();
		do
		{
			if (_nodeStream.MoveNext())
			{
				_current = _nodeStream.Current;
				if (_current.NodeType == XamlNodeType.None)
				{
					if (_current.LineInfo != null)
					{
						_currentLineInfo = _current.LineInfo;
					}
					else if (_current.IsEof)
					{
						break;
					}
				}
				continue;
			}
			_current = _endOfStreamNode;
			break;
		}
		while (_current.NodeType == XamlNodeType.None);
		return !IsEof;
	}

	private void ThrowIfDisposed()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException("XamlXmlReader");
		}
	}

	internal bool IsXmlNamespaceSupported(string xmlNamespace, out string newXmlNamespace)
	{
		if (_mergedSettings.LocalAssembly != null && ClrNamespaceUriParser.TryParseUri(xmlNamespace, out var clrNs, out var assemblyName) && string.IsNullOrEmpty(assemblyName))
		{
			assemblyName = _mergedSettings.LocalAssembly.FullName;
			newXmlNamespace = ClrNamespaceUriParser.GetUri(clrNs, assemblyName);
			return true;
		}
		bool result = _context.SchemaContext.TryGetCompatibleXamlNamespace(xmlNamespace, out newXmlNamespace);
		if (newXmlNamespace == null)
		{
			newXmlNamespace = string.Empty;
		}
		return result;
	}
}
