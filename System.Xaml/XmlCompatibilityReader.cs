using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace System.Xaml;

internal sealed class XmlCompatibilityReader : XmlWrappingReader
{
	private struct NamespaceElementPair(string namespaceName, string itemName)
	{
		public string namespaceName = namespaceName;

		public string itemName = itemName;
	}

	private class CompatibilityScope
	{
		private CompatibilityScope _previous;

		private int _depth;

		private bool _fallbackSeen;

		private bool _inAlternateContent;

		private bool _inProcessContent;

		private bool _choiceTaken;

		private bool _choiceSeen;

		private XmlCompatibilityReader _reader;

		private Dictionary<string, object> _ignorables;

		private Dictionary<string, ProcessContentSet> _processContents;

		private Dictionary<string, PreserveItemSet> _preserveElements;

		private Dictionary<string, PreserveItemSet> _preserveAttributes;

		public CompatibilityScope Previous => _previous;

		public int Depth => _depth;

		public bool FallbackSeen
		{
			get
			{
				if (_inProcessContent && _previous != null)
				{
					return _previous.FallbackSeen;
				}
				return _fallbackSeen;
			}
			set
			{
				if (_inProcessContent && _previous != null)
				{
					_previous.FallbackSeen = value;
				}
				else
				{
					_fallbackSeen = value;
				}
			}
		}

		public bool InAlternateContent
		{
			get
			{
				if (_inProcessContent && _previous != null)
				{
					return _previous.InAlternateContent;
				}
				return _inAlternateContent;
			}
			set
			{
				_inAlternateContent = value;
			}
		}

		public bool InProcessContent
		{
			set
			{
				_inProcessContent = value;
			}
		}

		public bool ChoiceTaken
		{
			get
			{
				if (_inProcessContent && _previous != null)
				{
					return _previous.ChoiceTaken;
				}
				return _choiceTaken;
			}
			set
			{
				if (_inProcessContent && _previous != null)
				{
					_previous.ChoiceTaken = value;
				}
				else
				{
					_choiceTaken = value;
				}
			}
		}

		public bool ChoiceSeen
		{
			get
			{
				if (_inProcessContent && _previous != null)
				{
					return _previous.ChoiceSeen;
				}
				return _choiceSeen;
			}
			set
			{
				if (_inProcessContent && _previous != null)
				{
					_previous.ChoiceSeen = value;
				}
				else
				{
					_choiceSeen = value;
				}
			}
		}

		public CompatibilityScope(CompatibilityScope previous, int depth, XmlCompatibilityReader reader)
		{
			_previous = previous;
			_depth = depth;
			_reader = reader;
		}

		public bool CanIgnore(string namespaceName)
		{
			bool flag = IsIgnorableAtCurrentScope(namespaceName);
			if (!flag && _previous != null)
			{
				flag = _previous.CanIgnore(namespaceName);
			}
			return flag;
		}

		public bool IsIgnorableAtCurrentScope(string namespaceName)
		{
			if (_ignorables != null)
			{
				return _ignorables.ContainsKey(namespaceName);
			}
			return false;
		}

		public bool ShouldProcessContent(string namespaceName, string elementName)
		{
			bool result = false;
			if (_processContents != null && _processContents.TryGetValue(namespaceName, out var value))
			{
				result = value.ShouldProcessContent(elementName);
			}
			else if (_previous != null)
			{
				result = _previous.ShouldProcessContent(namespaceName, elementName);
			}
			return result;
		}

		public void Ignorable(string namespaceName)
		{
			if (_ignorables == null)
			{
				_ignorables = new Dictionary<string, object>();
			}
			_ignorables[namespaceName] = null;
		}

		public void ProcessContent(string namespaceName, string elementName)
		{
			if (_processContents == null)
			{
				_processContents = new Dictionary<string, ProcessContentSet>();
			}
			if (!_processContents.TryGetValue(namespaceName, out var value))
			{
				value = new ProcessContentSet(namespaceName, _reader);
				_processContents.Add(namespaceName, value);
			}
			value.Add(elementName);
		}

		public void PreserveElement(string namespaceName, string elementName)
		{
			if (_preserveElements == null)
			{
				_preserveElements = new Dictionary<string, PreserveItemSet>();
			}
			if (!_preserveElements.TryGetValue(namespaceName, out var value))
			{
				value = new PreserveItemSet(namespaceName, _reader);
				_preserveElements.Add(namespaceName, value);
			}
			value.Add(elementName);
		}

		public void PreserveAttribute(string namespaceName, string attributeName)
		{
			if (_preserveAttributes == null)
			{
				_preserveAttributes = new Dictionary<string, PreserveItemSet>();
			}
			if (!_preserveAttributes.TryGetValue(namespaceName, out var value))
			{
				value = new PreserveItemSet(namespaceName, _reader);
				_preserveAttributes.Add(namespaceName, value);
			}
			value.Add(attributeName);
		}

		public void Verify()
		{
			if (_processContents != null)
			{
				foreach (string key in _processContents.Keys)
				{
					if (!IsIgnorableAtCurrentScope(key))
					{
						_reader.Error(SR.Get("XCRNSProcessContentNotIgnorable"), key);
					}
				}
			}
			if (_preserveElements != null)
			{
				foreach (string key2 in _preserveElements.Keys)
				{
					if (!IsIgnorableAtCurrentScope(key2))
					{
						_reader.Error(SR.Get("XCRNSPreserveNotIgnorable"), key2);
					}
				}
			}
			if (_preserveAttributes == null)
			{
				return;
			}
			foreach (string key3 in _preserveAttributes.Keys)
			{
				if (!IsIgnorableAtCurrentScope(key3))
				{
					_reader.Error(SR.Get("XCRNSPreserveNotIgnorable"), key3);
				}
			}
		}
	}

	private class ProcessContentSet
	{
		private bool _all;

		private string _namespaceName;

		private XmlCompatibilityReader _reader;

		private Dictionary<string, object> _names;

		public ProcessContentSet(string namespaceName, XmlCompatibilityReader reader)
		{
			_namespaceName = namespaceName;
			_reader = reader;
		}

		public bool ShouldProcessContent(string elementName)
		{
			if (!_all)
			{
				if (_names != null)
				{
					return _names.ContainsKey(elementName);
				}
				return false;
			}
			return true;
		}

		public void Add(string elementName)
		{
			if (ShouldProcessContent(elementName))
			{
				if (elementName == "*")
				{
					_reader.Error(SR.Get("XCRDuplicateWildcardProcessContent"), _namespaceName);
				}
				else
				{
					_reader.Error(SR.Get("XCRDuplicateProcessContent"), _namespaceName, elementName);
				}
			}
			if (elementName == "*")
			{
				if (_names != null)
				{
					_reader.Error(SR.Get("XCRInvalidProcessContent"), _namespaceName);
				}
				else
				{
					_all = true;
				}
			}
			else
			{
				if (_names == null)
				{
					_names = new Dictionary<string, object>();
				}
				_names[elementName] = null;
			}
		}
	}

	private class PreserveItemSet
	{
		private bool _all;

		private string _namespaceName;

		private XmlCompatibilityReader _reader;

		private Dictionary<string, string> _names;

		public PreserveItemSet(string namespaceName, XmlCompatibilityReader reader)
		{
			_namespaceName = namespaceName;
			_reader = reader;
		}

		public bool ShouldPreserveItem(string itemName)
		{
			if (!_all)
			{
				if (_names != null)
				{
					return _names.ContainsKey(itemName);
				}
				return false;
			}
			return true;
		}

		public void Add(string itemName)
		{
			if (ShouldPreserveItem(itemName))
			{
				if (itemName == "*")
				{
					_reader.Error(SR.Get("XCRDuplicateWildcardPreserve"), _namespaceName);
				}
				else
				{
					_reader.Error(SR.Get("XCRDuplicatePreserve"), itemName, _namespaceName);
				}
			}
			if (itemName == "*")
			{
				if (_names != null)
				{
					_reader.Error(SR.Get("XCRInvalidPreserve"), _namespaceName);
				}
				else
				{
					_all = true;
				}
			}
			else
			{
				if (_names == null)
				{
					_names = new Dictionary<string, string>();
				}
				_names.Add(itemName, itemName);
			}
		}
	}

	private bool _inAttribute;

	private string _currentName;

	private IsXmlNamespaceSupportedCallback _namespaceCallback;

	private Dictionary<string, object> _knownNamespaces;

	private Dictionary<string, string> _namespaceMap = new Dictionary<string, string>();

	private Dictionary<string, object> _subsumingNamespaces;

	private Dictionary<string, HandleElementCallback> _elementHandler = new Dictionary<string, HandleElementCallback>();

	private Dictionary<string, HandleAttributeCallback> _attributeHandler = new Dictionary<string, HandleAttributeCallback>();

	private int _depthOffset;

	private int _ignoredAttributeCount;

	private int _attributePosition;

	private string _compatibilityUri;

	private string _alternateContent;

	private string _choice;

	private string _fallback;

	private string _requires;

	private string _ignorable;

	private string _mustUnderstand;

	private string _processContent;

	private string _preserveElements;

	private string _preserveAttributes;

	private CompatibilityScope _compatibilityScope;

	private bool isPreviousElementEmpty;

	private int previousElementDepth;

	private const string XmlnsDeclaration = "xmlns";

	private const string MarkupCompatibilityURI = "http://schemas.openxmlformats.org/markup-compatibility/2006";

	private static string[] _predefinedNamespaces = new string[4] { "http://www.w3.org/2000/xmlns/", "http://www.w3.org/XML/1998/namespace", "http://www.w3.org/2001/XMLSchema-instance", "http://schemas.openxmlformats.org/markup-compatibility/2006" };

	public override string Value
	{
		get
		{
			if (string.Equals("xmlns", base.Reader.LocalName, StringComparison.Ordinal))
			{
				return LookupNamespace(string.Empty);
			}
			if (string.Equals("xmlns", base.Reader.Prefix, StringComparison.Ordinal))
			{
				return LookupNamespace(base.Reader.LocalName);
			}
			return base.Reader.Value;
		}
	}

	public override string NamespaceURI => GetMappedNamespace(base.Reader.NamespaceURI);

	public override int Depth => base.Reader.Depth - _depthOffset;

	public override bool HasAttributes => AttributeCount != 0;

	public override int AttributeCount => base.Reader.AttributeCount - _ignoredAttributeCount;

	public bool Normalization
	{
		set
		{
			if (base.Reader is XmlTextReader xmlTextReader)
			{
				xmlTextReader.Normalization = value;
			}
		}
	}

	internal Encoding Encoding
	{
		get
		{
			if (!(base.Reader is XmlTextReader xmlTextReader))
			{
				return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true, throwOnInvalidBytes: true);
			}
			return xmlTextReader.Encoding;
		}
	}

	private CompatibilityScope Scope => _compatibilityScope;

	private string AlternateContent
	{
		get
		{
			if (_alternateContent == null)
			{
				_alternateContent = base.Reader.NameTable.Add("AlternateContent");
			}
			return _alternateContent;
		}
	}

	private string Choice
	{
		get
		{
			if (_choice == null)
			{
				_choice = base.Reader.NameTable.Add("Choice");
			}
			return _choice;
		}
	}

	private string Fallback
	{
		get
		{
			if (_fallback == null)
			{
				_fallback = base.Reader.NameTable.Add("Fallback");
			}
			return _fallback;
		}
	}

	private string Requires
	{
		get
		{
			if (_requires == null)
			{
				_requires = base.Reader.NameTable.Add("Requires");
			}
			return _requires;
		}
	}

	private string Ignorable
	{
		get
		{
			if (_ignorable == null)
			{
				_ignorable = base.Reader.NameTable.Add("Ignorable");
			}
			return _ignorable;
		}
	}

	private string MustUnderstand
	{
		get
		{
			if (_mustUnderstand == null)
			{
				_mustUnderstand = base.Reader.NameTable.Add("MustUnderstand");
			}
			return _mustUnderstand;
		}
	}

	private string ProcessContent
	{
		get
		{
			if (_processContent == null)
			{
				_processContent = base.Reader.NameTable.Add("ProcessContent");
			}
			return _processContent;
		}
	}

	private string PreserveElements
	{
		get
		{
			if (_preserveElements == null)
			{
				_preserveElements = base.Reader.NameTable.Add("PreserveElements");
			}
			return _preserveElements;
		}
	}

	private string PreserveAttributes
	{
		get
		{
			if (_preserveAttributes == null)
			{
				_preserveAttributes = base.Reader.NameTable.Add("PreserveAttributes");
			}
			return _preserveAttributes;
		}
	}

	private string CompatibilityUri
	{
		get
		{
			if (_compatibilityUri == null)
			{
				_compatibilityUri = base.Reader.NameTable.Add("http://schemas.openxmlformats.org/markup-compatibility/2006");
			}
			return _compatibilityUri;
		}
	}

	public XmlCompatibilityReader(XmlReader baseReader)
		: base(baseReader)
	{
		_compatibilityScope = new CompatibilityScope(null, -1, this);
		string[] predefinedNamespaces = _predefinedNamespaces;
		foreach (string text in predefinedNamespaces)
		{
			AddKnownNamespace(text);
			_namespaceMap[text] = text;
			base.Reader.NameTable.Add(text);
		}
		_elementHandler.Add(AlternateContent, HandleAlternateContent);
		_elementHandler.Add(Choice, HandleChoice);
		_elementHandler.Add(Fallback, HandleFallback);
		_attributeHandler.Add(Ignorable, HandleIgnorable);
		_attributeHandler.Add(MustUnderstand, HandleMustUnderstand);
		_attributeHandler.Add(ProcessContent, HandleProcessContent);
		_attributeHandler.Add(PreserveElements, HandlePreserveElements);
		_attributeHandler.Add(PreserveAttributes, HandlePreserveAttributes);
	}

	public XmlCompatibilityReader(XmlReader baseReader, IsXmlNamespaceSupportedCallback isXmlNamespaceSupported)
		: this(baseReader)
	{
		_namespaceCallback = isXmlNamespaceSupported;
	}

	public XmlCompatibilityReader(XmlReader baseReader, IsXmlNamespaceSupportedCallback isXmlNamespaceSupported, IEnumerable<string> supportedNamespaces)
		: this(baseReader, isXmlNamespaceSupported)
	{
		foreach (string supportedNamespace in supportedNamespaces)
		{
			AddKnownNamespace(supportedNamespace);
			_namespaceMap[supportedNamespace] = supportedNamespace;
		}
	}

	public XmlCompatibilityReader(XmlReader baseReader, IEnumerable<string> supportedNamespaces)
		: this(baseReader, null, supportedNamespaces)
	{
	}

	public void DeclareNamespaceCompatibility(string newNamespace, string oldNamespace)
	{
		if (newNamespace != oldNamespace)
		{
			AddSubsumingNamespace(newNamespace);
			if (_namespaceMap.TryGetValue(newNamespace, out var value))
			{
				newNamespace = value;
			}
			if (IsSubsumingNamespace(oldNamespace))
			{
				List<string> list = new List<string>();
				foreach (KeyValuePair<string, string> item in _namespaceMap)
				{
					if (item.Value == oldNamespace)
					{
						list.Add(item.Key);
					}
				}
				foreach (string item2 in list)
				{
					_namespaceMap[item2] = newNamespace;
				}
			}
		}
		_namespaceMap[oldNamespace] = newNamespace;
	}

	public override bool Read()
	{
		if (isPreviousElementEmpty)
		{
			isPreviousElementEmpty = false;
			ScanForEndCompatibility(previousElementDepth);
		}
		bool more = base.Reader.Read();
		bool result = false;
		while (more)
		{
			switch (base.Reader.NodeType)
			{
			case XmlNodeType.Element:
				if (!ReadStartElement(ref more))
				{
					continue;
				}
				break;
			case XmlNodeType.EndElement:
				if (!ReadEndElement(ref more))
				{
					continue;
				}
				break;
			}
			result = true;
			break;
		}
		return result;
	}

	private bool ReadStartElement(ref bool more)
	{
		int depth = base.Reader.Depth;
		int depthOffset = _depthOffset;
		bool isEmptyElement = base.Reader.IsEmptyElement;
		string namespaceURI = NamespaceURI;
		bool result = false;
		if ((object)namespaceURI == CompatibilityUri)
		{
			string localName = base.Reader.LocalName;
			if (!_elementHandler.TryGetValue(localName, out var value))
			{
				Error(SR.Get("XCRUnknownCompatElement"), localName);
			}
			value(depth, ref more);
		}
		else
		{
			ScanForCompatibility(depth);
			if (ShouldIgnoreNamespace(namespaceURI))
			{
				if (Scope.ShouldProcessContent(namespaceURI, base.Reader.LocalName))
				{
					if (Scope.Depth == depth)
					{
						Scope.InProcessContent = true;
					}
					_depthOffset++;
					more = base.Reader.Read();
				}
				else
				{
					ScanForEndCompatibility(depth);
					base.Reader.Skip();
				}
			}
			else
			{
				if (Scope.InAlternateContent)
				{
					Error(SR.Get("XCRInvalidACChild"), base.Reader.Name);
				}
				result = true;
			}
		}
		if (isEmptyElement)
		{
			isPreviousElementEmpty = true;
			previousElementDepth = depth;
			_depthOffset = depthOffset;
		}
		return result;
	}

	private bool ReadEndElement(ref bool more)
	{
		int depth = base.Reader.Depth;
		string namespaceURI = NamespaceURI;
		bool result = false;
		if ((object)namespaceURI == CompatibilityUri)
		{
			string localName = base.Reader.LocalName;
			if ((object)localName == AlternateContent && !Scope.ChoiceSeen)
			{
				Error(SR.Get("XCRChoiceNotFound"));
			}
			_depthOffset--;
			PopScope();
			more = base.Reader.Read();
		}
		else if (ShouldIgnoreNamespace(namespaceURI))
		{
			ScanForEndCompatibility(depth);
			_depthOffset--;
			more = base.Reader.Read();
		}
		else
		{
			ScanForEndCompatibility(depth);
			result = true;
		}
		return result;
	}

	public override string GetAttribute(int i)
	{
		string text = null;
		if (_ignoredAttributeCount == 0)
		{
			text = base.Reader.GetAttribute(i);
		}
		else
		{
			SaveReaderPosition();
			MoveToAttribute(i);
			text = base.Reader.Value;
			RestoreReaderPosition();
		}
		return text;
	}

	public override string GetAttribute(string name)
	{
		string result = null;
		if (_ignoredAttributeCount == 0)
		{
			result = base.Reader.GetAttribute(name);
		}
		else
		{
			SaveReaderPosition();
			if (MoveToAttribute(name))
			{
				result = base.Reader.Value;
				RestoreReaderPosition();
			}
		}
		return result;
	}

	public override string GetAttribute(string localName, string namespaceURI)
	{
		string result = null;
		if (_ignoredAttributeCount == 0 || !ShouldIgnoreNamespace(namespaceURI))
		{
			result = base.Reader.GetAttribute(localName, namespaceURI);
		}
		return result;
	}

	public override void MoveToAttribute(int i)
	{
		if (_ignoredAttributeCount == 0)
		{
			base.Reader.MoveToAttribute(i);
			return;
		}
		if (i < 0 || i >= AttributeCount)
		{
			throw new ArgumentOutOfRangeException("i");
		}
		base.Reader.MoveToFirstAttribute();
		while (ShouldIgnoreNamespace(NamespaceURI) || i-- != 0)
		{
			base.Reader.MoveToNextAttribute();
		}
	}

	public override bool MoveToAttribute(string name)
	{
		bool flag;
		if (_ignoredAttributeCount == 0)
		{
			flag = base.Reader.MoveToAttribute(name);
		}
		else
		{
			SaveReaderPosition();
			flag = base.Reader.MoveToAttribute(name);
			if (flag && ShouldIgnoreNamespace(NamespaceURI))
			{
				flag = false;
				RestoreReaderPosition();
			}
		}
		return flag;
	}

	public override bool MoveToAttribute(string localName, string namespaceURI)
	{
		bool flag;
		if (_ignoredAttributeCount == 0)
		{
			flag = base.Reader.MoveToAttribute(localName, namespaceURI);
		}
		else
		{
			SaveReaderPosition();
			flag = base.Reader.MoveToAttribute(localName, namespaceURI);
			if (flag && ShouldIgnoreNamespace(namespaceURI))
			{
				flag = false;
				RestoreReaderPosition();
			}
		}
		return flag;
	}

	public override bool MoveToFirstAttribute()
	{
		bool hasAttributes = HasAttributes;
		if (hasAttributes)
		{
			MoveToAttribute(0);
		}
		return hasAttributes;
	}

	public override bool MoveToNextAttribute()
	{
		bool flag;
		if (_ignoredAttributeCount == 0)
		{
			flag = base.Reader.MoveToNextAttribute();
		}
		else
		{
			SaveReaderPosition();
			flag = base.Reader.MoveToNextAttribute();
			if (flag)
			{
				flag = SkipToKnownAttribute();
				if (!flag)
				{
					RestoreReaderPosition();
				}
			}
		}
		return flag;
	}

	public override string LookupNamespace(string prefix)
	{
		string text = base.Reader.LookupNamespace(prefix);
		if (text != null)
		{
			text = GetMappedNamespace(text);
		}
		return text;
	}

	private void SaveReaderPosition()
	{
		_inAttribute = base.Reader.NodeType == XmlNodeType.Attribute;
		_currentName = base.Reader.Name;
	}

	private void RestoreReaderPosition()
	{
		if (_inAttribute)
		{
			base.Reader.MoveToAttribute(_currentName);
		}
		else
		{
			base.Reader.MoveToElement();
		}
	}

	private string GetMappedNamespace(string namespaceName)
	{
		if (!_namespaceMap.TryGetValue(namespaceName, out var value))
		{
			return MapNewNamespace(namespaceName);
		}
		if (value == null)
		{
			return namespaceName;
		}
		return value;
	}

	private string MapNewNamespace(string namespaceName)
	{
		if (_namespaceCallback != null)
		{
			if (_namespaceCallback(namespaceName, out var newXmlNamespace))
			{
				AddKnownNamespace(namespaceName);
				if (string.IsNullOrEmpty(newXmlNamespace) || namespaceName == newXmlNamespace)
				{
					_namespaceMap[namespaceName] = namespaceName;
				}
				else
				{
					if (!_namespaceMap.TryGetValue(newXmlNamespace, out var value))
					{
						if (IsNamespaceKnown(newXmlNamespace))
						{
							Error(SR.Get("XCRCompatCycle"), newXmlNamespace);
						}
						value = MapNewNamespace(newXmlNamespace);
					}
					DeclareNamespaceCompatibility(value, namespaceName);
					namespaceName = value;
				}
			}
			else
			{
				_namespaceMap[namespaceName] = null;
			}
		}
		return namespaceName;
	}

	private bool IsSubsumingNamespace(string namespaceName)
	{
		if (_subsumingNamespaces != null)
		{
			return _subsumingNamespaces.ContainsKey(namespaceName);
		}
		return false;
	}

	private void AddSubsumingNamespace(string namespaceName)
	{
		if (_subsumingNamespaces == null)
		{
			_subsumingNamespaces = new Dictionary<string, object>();
		}
		_subsumingNamespaces[namespaceName] = null;
	}

	private bool IsNamespaceKnown(string namespaceName)
	{
		if (_knownNamespaces != null)
		{
			return _knownNamespaces.ContainsKey(namespaceName);
		}
		return false;
	}

	private void AddKnownNamespace(string namespaceName)
	{
		if (_knownNamespaces == null)
		{
			_knownNamespaces = new Dictionary<string, object>();
		}
		_knownNamespaces[namespaceName] = null;
	}

	private bool ShouldIgnoreNamespace(string namespaceName)
	{
		if (IsNamespaceKnown(namespaceName))
		{
			return (object)namespaceName == CompatibilityUri;
		}
		return Scope.CanIgnore(namespaceName);
	}

	private IEnumerable<NamespaceElementPair> ParseContentToNamespaceElementPair(string content, string callerContext)
	{
		string[] array = content.Trim().Split(' ');
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text))
			{
				int num = text.IndexOf(':');
				int length = text.Length;
				if (num <= 0 || num >= length - 1 || num != text.LastIndexOf(':'))
				{
					Error(SR.Get("XCRInvalidFormat"), callerContext);
				}
				string text2 = text.Substring(0, num);
				string text3 = text.Substring(num + 1, length - 1 - num);
				string text4 = LookupNamespace(text2);
				if (text4 == null)
				{
					Error(SR.Get("XCRUndefinedPrefix"), text2);
				}
				else if (text3 != "*" && !XmlReader.IsName(text3))
				{
					Error(SR.Get("XCRInvalidXMLName"), text);
				}
				else
				{
					yield return new NamespaceElementPair(text4, text3);
				}
			}
		}
	}

	private IEnumerable<string> PrefixesToNamespaces(string prefixes)
	{
		string[] array = prefixes.Trim().Split(' ');
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = LookupNamespace(text);
				if (text2 == null)
				{
					Error(SR.Get("XCRUndefinedPrefix"), text);
				}
				else
				{
					yield return text2;
				}
			}
		}
	}

	private bool SkipToKnownAttribute()
	{
		bool flag;
		for (flag = true; flag && ShouldIgnoreNamespace(NamespaceURI); flag = base.Reader.MoveToNextAttribute())
		{
		}
		return flag;
	}

	private void ScanForCompatibility(int elementDepth)
	{
		bool flag = base.Reader.MoveToFirstAttribute();
		_ignoredAttributeCount = 0;
		if (!flag)
		{
			return;
		}
		_attributePosition = 0;
		do
		{
			string namespaceURI = NamespaceURI;
			if (ShouldIgnoreNamespace(namespaceURI))
			{
				if ((object)namespaceURI == CompatibilityUri)
				{
					string localName = base.Reader.LocalName;
					if (!_attributeHandler.TryGetValue(localName, out var value))
					{
						Error(SR.Get("XCRUnknownCompatAttrib"), localName);
					}
					value(elementDepth);
				}
				_ignoredAttributeCount++;
			}
			flag = base.Reader.MoveToNextAttribute();
			_attributePosition++;
		}
		while (flag);
		if (Scope.Depth == elementDepth)
		{
			Scope.Verify();
		}
		base.Reader.MoveToElement();
	}

	private void ScanForEndCompatibility(int elementDepth)
	{
		if (elementDepth == Scope.Depth)
		{
			PopScope();
		}
	}

	private void PushScope(int elementDepth)
	{
		if (_compatibilityScope.Depth < elementDepth)
		{
			_compatibilityScope = new CompatibilityScope(_compatibilityScope, elementDepth, this);
		}
	}

	private void PopScope()
	{
		_compatibilityScope = _compatibilityScope.Previous;
	}

	private void HandleAlternateContent(int elementDepth, ref bool more)
	{
		if (Scope.InAlternateContent)
		{
			Error(SR.Get("XCRInvalidACChild", base.Reader.Name));
		}
		if (base.Reader.IsEmptyElement)
		{
			Error(SR.Get("XCRChoiceNotFound"));
		}
		ScanForCompatibility(elementDepth);
		PushScope(elementDepth);
		Scope.InAlternateContent = true;
		_depthOffset++;
		more = base.Reader.Read();
	}

	private void HandleChoice(int elementDepth, ref bool more)
	{
		if (!Scope.InAlternateContent)
		{
			Error(SR.Get("XCRChoiceOnlyInAC"));
		}
		if (Scope.FallbackSeen)
		{
			Error(SR.Get("XCRChoiceAfterFallback"));
		}
		string attribute = base.Reader.GetAttribute(Requires);
		if (attribute == null)
		{
			Error(SR.Get("XCRRequiresAttribNotFound"));
		}
		if (string.IsNullOrEmpty(attribute))
		{
			Error(SR.Get("XCRInvalidRequiresAttribute"));
		}
		CompatibilityScope scope = Scope;
		ScanForCompatibility(elementDepth);
		if (AttributeCount != 1)
		{
			MoveToFirstAttribute();
			if (base.Reader.LocalName == Requires)
			{
				MoveToNextAttribute();
			}
			string localName = base.Reader.LocalName;
			MoveToElement();
			Error(SR.Get("XCRInvalidAttribInElement"), localName, Choice);
		}
		if (scope.ChoiceTaken)
		{
			ScanForEndCompatibility(elementDepth);
			base.Reader.Skip();
			return;
		}
		scope.ChoiceSeen = true;
		bool flag = true;
		bool flag2 = false;
		foreach (string item in PrefixesToNamespaces(attribute))
		{
			flag2 = true;
			if (!IsNamespaceKnown(item))
			{
				flag = false;
				break;
			}
		}
		if (!flag2)
		{
			Error(SR.Get("XCRInvalidRequiresAttribute"));
		}
		if (flag)
		{
			scope.ChoiceTaken = true;
			PushScope(elementDepth);
			_depthOffset++;
			more = base.Reader.Read();
		}
		else
		{
			ScanForEndCompatibility(elementDepth);
			base.Reader.Skip();
		}
	}

	private void HandleFallback(int elementDepth, ref bool more)
	{
		if (!Scope.InAlternateContent)
		{
			Error(SR.Get("XCRFallbackOnlyInAC"));
		}
		if (!Scope.ChoiceSeen)
		{
			Error(SR.Get("XCRChoiceNotFound"));
		}
		if (Scope.FallbackSeen)
		{
			Error(SR.Get("XCRMultipleFallbackFound"));
		}
		Scope.FallbackSeen = true;
		bool choiceTaken = Scope.ChoiceTaken;
		ScanForCompatibility(elementDepth);
		if (AttributeCount != 0)
		{
			MoveToFirstAttribute();
			string localName = base.Reader.LocalName;
			MoveToElement();
			Error(SR.Get("XCRInvalidAttribInElement"), localName, Fallback);
		}
		if (choiceTaken)
		{
			ScanForEndCompatibility(elementDepth);
			base.Reader.Skip();
			return;
		}
		if (!base.Reader.IsEmptyElement)
		{
			PushScope(elementDepth);
			_depthOffset++;
		}
		more = base.Reader.Read();
	}

	private void HandleIgnorable(int elementDepth)
	{
		PushScope(elementDepth);
		foreach (string item in PrefixesToNamespaces(base.Reader.Value))
		{
			Scope.Ignorable(item);
		}
		if (_ignoredAttributeCount >= _attributePosition)
		{
			return;
		}
		_ignoredAttributeCount = 0;
		base.Reader.MoveToFirstAttribute();
		for (int i = 0; i < _attributePosition; i++)
		{
			if (ShouldIgnoreNamespace(base.Reader.NamespaceURI))
			{
				_ignoredAttributeCount++;
			}
			base.Reader.MoveToNextAttribute();
		}
	}

	private void HandleMustUnderstand(int elementDepth)
	{
		foreach (string item in PrefixesToNamespaces(base.Reader.Value))
		{
			if (!IsNamespaceKnown(item))
			{
				Error(SR.Get("XCRMustUnderstandFailed"), item);
			}
		}
	}

	private void HandleProcessContent(int elementDepth)
	{
		PushScope(elementDepth);
		foreach (NamespaceElementPair item in ParseContentToNamespaceElementPair(base.Reader.Value, _processContent))
		{
			Scope.ProcessContent(item.namespaceName, item.itemName);
		}
	}

	private void HandlePreserveElements(int elementDepth)
	{
		PushScope(elementDepth);
		foreach (NamespaceElementPair item in ParseContentToNamespaceElementPair(base.Reader.Value, _preserveElements))
		{
			Scope.PreserveElement(item.namespaceName, item.itemName);
		}
	}

	private void HandlePreserveAttributes(int elementDepth)
	{
		PushScope(elementDepth);
		foreach (NamespaceElementPair item in ParseContentToNamespaceElementPair(base.Reader.Value, _preserveAttributes))
		{
			Scope.PreserveAttribute(item.namespaceName, item.itemName);
		}
	}

	private void Error(string message, params object[] args)
	{
		IXmlLineInfo xmlLineInfo = base.Reader as IXmlLineInfo;
		throw new XmlException(string.Format(CultureInfo.InvariantCulture, message, args), null, xmlLineInfo?.LineNumber ?? 1, xmlLineInfo?.LinePosition ?? 1);
	}
}
