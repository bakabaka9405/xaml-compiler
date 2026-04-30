using System.Collections.Generic;
using System.Xaml;
using System.Xaml.MS.Impl;
using System.Xaml.Schema;
using System.Xml;
using MS.Internal.Xaml.Context;

namespace MS.Internal.Xaml.Parser;

internal class XamlScanner
{
	private XmlReader _xmlReader;

	private IXmlLineInfo _xmlLineInfo;

	private XamlScannerStack _scannerStack;

	private XamlParserContext _parserContext;

	private XamlText _accumulatedText;

	private List<XamlAttribute> _attributes;

	private int _nextAttribute;

	private XamlScannerNode _currentNode;

	private Queue<XamlScannerNode> _readNodesQueue;

	private XamlXmlReaderSettings _settings;

	private XamlAttribute _typeArgumentAttribute;

	private bool _hasKeyAttribute;

	public ScannerNodeType PeekNodeType
	{
		get
		{
			LoadQueue();
			return _readNodesQueue.Peek().NodeType;
		}
	}

	public XamlType PeekType
	{
		get
		{
			LoadQueue();
			return _readNodesQueue.Peek().Type;
		}
	}

	public ScannerNodeType NodeType => _currentNode.NodeType;

	public XamlType Type => _currentNode.Type;

	public XamlMember PropertyAttribute => _currentNode.PropertyAttribute;

	public XamlText PropertyAttributeText => _currentNode.PropertyAttributeText;

	public bool IsCtorForcingMember => _currentNode.IsCtorForcingMember;

	public XamlMember PropertyElement => _currentNode.PropertyElement;

	public XamlText TextContent => _currentNode.TextContent;

	public bool IsXDataText => _currentNode.IsXDataText;

	public bool HasKeyAttribute => _hasKeyAttribute;

	public string Prefix => _currentNode.Prefix;

	public string Namespace => _currentNode.TypeNamespace;

	public int LineNumber => _currentNode.LineNumber;

	public int LinePosition => _currentNode.LinePosition;

	private XamlText AccumulatedText
	{
		get
		{
			if (_accumulatedText == null)
			{
				_accumulatedText = new XamlText(_scannerStack.CurrentXmlSpacePreserve);
			}
			return _accumulatedText;
		}
	}

	private bool HaveAccumulatedText
	{
		get
		{
			if (_accumulatedText != null)
			{
				return !_accumulatedText.IsEmpty;
			}
			return false;
		}
	}

	private bool HaveUnprocessedAttributes => _attributes != null;

	internal XamlScanner(XamlParserContext context, XmlReader xmlReader, XamlXmlReaderSettings settings)
	{
		_xmlReader = xmlReader;
		_xmlLineInfo = (settings.ProvideLineInfo ? (xmlReader as IXmlLineInfo) : null);
		_parserContext = context;
		_scannerStack = new XamlScannerStack();
		_readNodesQueue = new Queue<XamlScannerNode>();
		_settings = settings;
		if (settings.XmlSpacePreserve)
		{
			_scannerStack.CurrentXmlSpacePreserve = true;
		}
	}

	public void Read()
	{
		LoadQueue();
		_currentNode = _readNodesQueue.Dequeue();
	}

	private void LoadQueue()
	{
		if (_readNodesQueue.Count == 0)
		{
			DoXmlRead();
		}
	}

	private void DoXmlRead()
	{
		while (_readNodesQueue.Count == 0)
		{
			if (_xmlReader.Read())
			{
				ProcessCurrentXmlNode();
			}
			else
			{
				ReadNone();
			}
		}
	}

	private void ProcessCurrentXmlNode()
	{
		switch (_xmlReader.NodeType)
		{
		case XmlNodeType.Element:
			ReadElement();
			break;
		case XmlNodeType.EndElement:
			ReadEndElement();
			break;
		case XmlNodeType.Text:
		case XmlNodeType.CDATA:
			ReadText();
			break;
		case XmlNodeType.Whitespace:
		case XmlNodeType.SignificantWhitespace:
			ReadWhitespace();
			break;
		case XmlNodeType.None:
			ReadNone();
			break;
		}
	}

	private void ClearAccumulatedText()
	{
		_accumulatedText = null;
	}

	private void ReadElement()
	{
		EnqueueAnyText();
		_hasKeyAttribute = false;
		bool isEmptyElement = _xmlReader.IsEmptyElement;
		string prefix = _xmlReader.Prefix;
		string localName = _xmlReader.LocalName;
		if (XamlName.ContainsDot(localName))
		{
			XamlPropertyName name = XamlPropertyName.Parse(_xmlReader.Name, _xmlReader.NamespaceURI);
			if (_scannerStack.CurrentType == null)
			{
				throw LineInfo(new XamlParseException(SR.Get("ParentlessPropertyElement", _xmlReader.Name)));
			}
			ReadPropertyElement(name, _scannerStack.CurrentType, _scannerStack.CurrentTypeNamespace, isEmptyElement);
		}
		else
		{
			XamlName name2 = new XamlQualifiedName(prefix, localName);
			ReadObjectElement(name2, isEmptyElement);
		}
	}

	private void ReadObjectElement(XamlName name, bool isEmptyTag)
	{
		_typeArgumentAttribute = null;
		XamlScannerNode xamlScannerNode = new XamlScannerNode(_xmlLineInfo);
		PreprocessAttributes();
		xamlScannerNode.Prefix = name.Prefix;
		xamlScannerNode.IsEmptyTag = isEmptyTag;
		string namespaceURI = _xmlReader.NamespaceURI;
		if (namespaceURI == null)
		{
			ReadObjectElement_NoNamespace(name, xamlScannerNode);
		}
		else
		{
			xamlScannerNode.TypeNamespace = namespaceURI;
			XamlSchemaContext schemaContext = _parserContext.SchemaContext;
			XamlMember xamlDirective = schemaContext.GetXamlDirective(namespaceURI, name.Name);
			if (xamlDirective != null)
			{
				ReadObjectElement_DirectiveProperty(xamlDirective, xamlScannerNode);
			}
			else if (ReadObjectElement_Object(namespaceURI, name.Name, xamlScannerNode))
			{
				return;
			}
		}
		_readNodesQueue.Enqueue(xamlScannerNode);
		while (HaveUnprocessedAttributes)
		{
			EnqueueAnotherAttribute(isEmptyTag);
		}
	}

	private void ReadObjectElement_NoNamespace(XamlName name, XamlScannerNode node)
	{
		XamlType type = CreateErrorXamlType(name, string.Empty);
		node.Type = type;
		PostprocessAttributes(node);
		if (!node.IsEmptyTag)
		{
			node.NodeType = ScannerNodeType.ELEMENT;
			_scannerStack.Push(node.Type, node.TypeNamespace);
		}
		else
		{
			node.NodeType = ScannerNodeType.EMPTYELEMENT;
		}
	}

	private void ReadObjectElement_DirectiveProperty(XamlMember dirProperty, XamlScannerNode node)
	{
		node.PropertyElement = dirProperty;
		PostprocessAttributes(node);
		if (_scannerStack.Depth > 0)
		{
			_scannerStack.CurrentlyInContent = false;
		}
		if (!node.IsEmptyTag)
		{
			_scannerStack.CurrentProperty = node.PropertyElement;
		}
		node.NodeType = ScannerNodeType.PROPERTYELEMENT;
		node.IsCtorForcingMember = false;
	}

	private bool ReadObjectElement_Object(string xmlns, string name, XamlScannerNode node)
	{
		if (IsXDataElement(xmlns, name))
		{
			ReadInnerXDataSection();
			return true;
		}
		IList<XamlTypeName> list = null;
		if (_typeArgumentAttribute != null)
		{
			list = XamlTypeName.ParseListInternal(_typeArgumentAttribute.Value, _parserContext.FindNamespaceByPrefix, out var error);
			if (list == null)
			{
				throw new XamlParseException(_typeArgumentAttribute.LineNumber, _typeArgumentAttribute.LinePosition, error);
			}
		}
		XamlTypeName typeName = new XamlTypeName(xmlns, name, list);
		node.Type = _parserContext.GetXamlType(typeName, returnUnknownTypesOnFailure: true);
		PostprocessAttributes(node);
		if (_scannerStack.Depth > 0)
		{
			_scannerStack.CurrentlyInContent = true;
		}
		if (!node.IsEmptyTag)
		{
			node.NodeType = ScannerNodeType.ELEMENT;
			_scannerStack.Push(node.Type, node.TypeNamespace);
		}
		else
		{
			node.NodeType = ScannerNodeType.EMPTYELEMENT;
		}
		return false;
	}

	private void ReadPropertyElement(XamlPropertyName name, XamlType tagType, string tagNamespace, bool isEmptyTag)
	{
		XamlScannerNode xamlScannerNode = new XamlScannerNode(_xmlLineInfo);
		PreprocessAttributes();
		string namespaceURI = _xmlReader.NamespaceURI;
		XamlMember xamlMember = null;
		bool tagIsRoot = _scannerStack.Depth == 1;
		xamlMember = _parserContext.GetDottedProperty(tagType, tagNamespace, name, tagIsRoot);
		xamlScannerNode.Prefix = name.Prefix;
		xamlScannerNode.TypeNamespace = namespaceURI;
		xamlScannerNode.IsEmptyTag = isEmptyTag;
		PostprocessAttributes(xamlScannerNode);
		if (_scannerStack.Depth > 0)
		{
			_scannerStack.CurrentlyInContent = false;
		}
		xamlScannerNode.PropertyElement = xamlMember;
		xamlScannerNode.IsCtorForcingMember = !xamlMember.IsAttachable;
		if (!xamlScannerNode.IsEmptyTag)
		{
			_scannerStack.CurrentProperty = xamlScannerNode.PropertyElement;
			xamlScannerNode.NodeType = ScannerNodeType.PROPERTYELEMENT;
		}
		else
		{
			xamlScannerNode.NodeType = ScannerNodeType.EMPTYPROPERTYELEMENT;
		}
		_readNodesQueue.Enqueue(xamlScannerNode);
		while (HaveUnprocessedAttributes)
		{
			EnqueueAnotherAttribute(isEmptyTag);
		}
	}

	private void ReadEndElement()
	{
		EnqueueAnyText();
		if (_scannerStack.CurrentProperty != null)
		{
			_scannerStack.CurrentProperty = null;
			_scannerStack.CurrentlyInContent = false;
		}
		else
		{
			_scannerStack.Pop();
		}
		XamlScannerNode xamlScannerNode = new XamlScannerNode(_xmlLineInfo);
		xamlScannerNode.NodeType = ScannerNodeType.ENDTAG;
		_readNodesQueue.Enqueue(xamlScannerNode);
	}

	private void ReadText()
	{
		bool trimLeadingWhitespace = !_scannerStack.CurrentlyInContent;
		AccumulatedText.Paste(_xmlReader.Value, trimLeadingWhitespace);
		_scannerStack.CurrentlyInContent = true;
	}

	private void ReadWhitespace()
	{
		bool trimLeadingWhitespace = !_scannerStack.CurrentlyInContent;
		AccumulatedText.Paste(_xmlReader.Value, trimLeadingWhitespace);
	}

	private void ReadNone()
	{
		XamlScannerNode xamlScannerNode = new XamlScannerNode(_xmlLineInfo);
		xamlScannerNode.NodeType = ScannerNodeType.NONE;
		_readNodesQueue.Enqueue(xamlScannerNode);
	}

	private void ReadInnerXDataSection()
	{
		XamlScannerNode xamlScannerNode = new XamlScannerNode(_xmlLineInfo);
		_xmlReader.MoveToContent();
		string text = _xmlReader.ReadInnerXml();
		text = text.Trim();
		xamlScannerNode.NodeType = ScannerNodeType.TEXT;
		xamlScannerNode.IsXDataText = true;
		XamlText xamlText = new XamlText(spacePreserve: true);
		xamlText.Paste(text, trimLeadingWhitespace: false);
		xamlScannerNode.TextContent = xamlText;
		_readNodesQueue.Enqueue(xamlScannerNode);
		ProcessCurrentXmlNode();
	}

	private XamlType CreateErrorXamlType(XamlName name, string xmlns)
	{
		return new XamlType(xmlns, name.Name, null, _parserContext.SchemaContext);
	}

	private void PreprocessAttributes()
	{
		if (!_xmlReader.MoveToFirstAttribute())
		{
			return;
		}
		List<XamlAttribute> list = new List<XamlAttribute>();
		do
		{
			string name = _xmlReader.Name;
			string value = _xmlReader.Value;
			XamlPropertyName xamlPropertyName = XamlPropertyName.Parse(name);
			if (xamlPropertyName == null)
			{
				throw new XamlParseException(SR.Get("InvalidXamlMemberName", name));
			}
			XamlAttribute xamlAttribute = new XamlAttribute(xamlPropertyName, value, _xmlLineInfo);
			if (xamlAttribute.Kind == ScannerAttributeKind.Namespace)
			{
				EnqueuePrefixDefinition(xamlAttribute);
			}
			else
			{
				list.Add(xamlAttribute);
			}
		}
		while (_xmlReader.MoveToNextAttribute());
		PreprocessForTypeArguments(list);
		if (list.Count > 0)
		{
			_attributes = list;
		}
		_xmlReader.MoveToElement();
	}

	private void PreprocessForTypeArguments(List<XamlAttribute> attrList)
	{
		int num = -1;
		for (int i = 0; i < attrList.Count; i++)
		{
			XamlAttribute xamlAttribute = attrList[i];
			if (KS.Eq(xamlAttribute.Name.Name, XamlLanguage.TypeArguments.Name))
			{
				string xamlNS = _parserContext.FindNamespaceByPrefix(xamlAttribute.Name.Prefix);
				XamlMember xamlMember = _parserContext.ResolveDirectiveProperty(xamlNS, xamlAttribute.Name.Name);
				if (xamlMember != null)
				{
					num = i;
					_typeArgumentAttribute = xamlAttribute;
					break;
				}
			}
		}
		if (num >= 0)
		{
			attrList.RemoveAt(num);
		}
	}

	private void PostprocessAttributes(XamlScannerNode node)
	{
		if (_attributes == null)
		{
			return;
		}
		_nextAttribute = 0;
		if (node.Type == null)
		{
			if (_settings.IgnoreUidsOnPropertyElements)
			{
				StripUidProperty();
			}
			return;
		}
		bool tagIsRoot = _scannerStack.Depth == 0;
		foreach (XamlAttribute attribute in _attributes)
		{
			attribute.Initialize(_parserContext, node.Type, node.TypeNamespace, tagIsRoot);
		}
		List<XamlAttribute> list = null;
		List<XamlAttribute> list2 = null;
		List<XamlAttribute> list3 = null;
		XamlAttribute xamlAttribute = null;
		foreach (XamlAttribute attribute2 in _attributes)
		{
			switch (attribute2.Kind)
			{
			case ScannerAttributeKind.Name:
				xamlAttribute = attribute2;
				break;
			case ScannerAttributeKind.CtorDirective:
				if (list == null)
				{
					list = new List<XamlAttribute>();
				}
				list.Add(attribute2);
				break;
			case ScannerAttributeKind.Directive:
			case ScannerAttributeKind.XmlSpace:
				if (attribute2.Property == XamlLanguage.Key)
				{
					_hasKeyAttribute = true;
				}
				if (list2 == null)
				{
					list2 = new List<XamlAttribute>();
				}
				list2.Add(attribute2);
				break;
			default:
				if (list3 == null)
				{
					list3 = new List<XamlAttribute>();
				}
				list3.Add(attribute2);
				break;
			}
		}
		_attributes = new List<XamlAttribute>();
		if (list != null)
		{
			_attributes.AddRange(list);
		}
		if (list2 != null)
		{
			_attributes.AddRange(list2);
		}
		if (xamlAttribute != null)
		{
			_attributes.Add(xamlAttribute);
		}
		if (list3 != null)
		{
			_attributes.AddRange(list3);
		}
	}

	private void StripUidProperty()
	{
		for (int num = _attributes.Count - 1; num >= 0; num--)
		{
			if (KS.Eq(_attributes[num].Name.ScopedName, XamlLanguage.Uid.Name))
			{
				_attributes.RemoveAt(num);
			}
		}
		if (_attributes.Count == 0)
		{
			_attributes = null;
		}
	}

	private void EnqueueAnotherAttribute(bool isEmptyTag)
	{
		XamlAttribute xamlAttribute = _attributes[_nextAttribute++];
		XamlScannerNode xamlScannerNode = new XamlScannerNode(xamlAttribute);
		switch (xamlAttribute.Kind)
		{
		case ScannerAttributeKind.CtorDirective:
		case ScannerAttributeKind.Name:
		case ScannerAttributeKind.Directive:
			xamlScannerNode.NodeType = ScannerNodeType.DIRECTIVE;
			break;
		case ScannerAttributeKind.XmlSpace:
			if (!isEmptyTag)
			{
				if (KS.Eq(xamlAttribute.Value, "preserve"))
				{
					_scannerStack.CurrentXmlSpacePreserve = true;
				}
				else
				{
					_scannerStack.CurrentXmlSpacePreserve = false;
				}
			}
			xamlScannerNode.NodeType = ScannerNodeType.DIRECTIVE;
			break;
		case ScannerAttributeKind.Event:
		case ScannerAttributeKind.Property:
			xamlScannerNode.IsCtorForcingMember = true;
			xamlScannerNode.NodeType = ScannerNodeType.ATTRIBUTE;
			break;
		case ScannerAttributeKind.Unknown:
		{
			XamlMember property = xamlAttribute.Property;
			xamlScannerNode.IsCtorForcingMember = !property.IsAttachable && !property.IsDirective;
			xamlScannerNode.NodeType = ScannerNodeType.ATTRIBUTE;
			break;
		}
		case ScannerAttributeKind.AttachableProperty:
			xamlScannerNode.NodeType = ScannerNodeType.ATTRIBUTE;
			break;
		default:
			throw new XamlInternalException(SR.Get("AttributeUnhandledKind"));
		}
		XamlMember property2 = xamlAttribute.Property;
		bool convertCRLFtoLF = !(property2 != null) || !(property2.Name == "UnicodeString") || !(property2.DeclaringType.Name == "Glyphs");
		xamlScannerNode.PropertyAttribute = property2;
		XamlText xamlText = new XamlText(spacePreserve: true);
		xamlText.Paste(xamlAttribute.Value, trimLeadingWhitespace: false, convertCRLFtoLF);
		xamlScannerNode.PropertyAttributeText = xamlText;
		xamlScannerNode.Prefix = xamlAttribute.Name.Prefix;
		_readNodesQueue.Enqueue(xamlScannerNode);
		if (_nextAttribute >= _attributes.Count)
		{
			_attributes = null;
			_nextAttribute = -1;
		}
	}

	private void EnqueueAnyText()
	{
		if (HaveAccumulatedText)
		{
			EnqueueTextNode();
		}
		ClearAccumulatedText();
	}

	private void EnqueueTextNode()
	{
		if (_scannerStack.Depth != 0 || !AccumulatedText.IsWhiteSpaceOnly)
		{
			XamlScannerNode xamlScannerNode = new XamlScannerNode(_xmlLineInfo);
			xamlScannerNode.NodeType = ScannerNodeType.TEXT;
			xamlScannerNode.TextContent = AccumulatedText;
			_readNodesQueue.Enqueue(xamlScannerNode);
		}
	}

	private void EnqueuePrefixDefinition(XamlAttribute attr)
	{
		string xmlNsPrefixDefined = attr.XmlNsPrefixDefined;
		string xmlNsUriDefined = attr.XmlNsUriDefined;
		_parserContext.AddNamespacePrefix(xmlNsPrefixDefined, xmlNsUriDefined);
		XamlScannerNode xamlScannerNode = new XamlScannerNode(attr);
		xamlScannerNode.NodeType = ScannerNodeType.PREFIXDEFINITION;
		xamlScannerNode.Prefix = xmlNsPrefixDefined;
		xamlScannerNode.TypeNamespace = xmlNsUriDefined;
		_readNodesQueue.Enqueue(xamlScannerNode);
	}

	private bool IsXDataElement(string xmlns, string name)
	{
		if (XamlLanguage.XamlNamespaces.Contains(xmlns))
		{
			return KS.Eq(XamlLanguage.XData.Name, name);
		}
		return false;
	}

	private XamlException LineInfo(XamlException e)
	{
		if (_xmlLineInfo != null)
		{
			e.SetLineInfo(_xmlLineInfo.LineNumber, _xmlLineInfo.LinePosition);
		}
		return e;
	}
}
