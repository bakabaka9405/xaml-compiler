using System.Collections.Generic;
using System.Xaml;
using System.Xaml.Schema;
using MS.Internal.Xaml.Context;

namespace MS.Internal.Xaml.Parser;

internal class XamlPullParser
{
	private XamlParserContext _context;

	private XamlScanner _xamlScanner;

	private XamlXmlReaderSettings _settings;

	private readonly XamlTypeName arrayType = new XamlTypeName("http://schemas.microsoft.com/winfx/2006/xaml", "Array");

	private XamlType _arrayExtensionType;

	private XamlMember _arrayTypeMember;

	private XamlMember _itemsTypeMember;

	private int LineNumber => _xamlScanner.LineNumber;

	private int LinePosition => _xamlScanner.LinePosition;

	private bool ProvideLineInfo => _settings.ProvideLineInfo;

	private XamlType ArrayExtensionType
	{
		get
		{
			if (_arrayExtensionType == null)
			{
				_arrayExtensionType = _context.GetXamlType(arrayType);
			}
			return _arrayExtensionType;
		}
	}

	private XamlMember ArrayTypeMember
	{
		get
		{
			if (_arrayTypeMember == null)
			{
				_arrayTypeMember = _context.GetXamlProperty(ArrayExtensionType, "Type", null);
			}
			return _arrayTypeMember;
		}
	}

	private XamlMember ItemsTypeMember
	{
		get
		{
			if (_itemsTypeMember == null)
			{
				_itemsTypeMember = _context.GetXamlProperty(ArrayExtensionType, "Items", null);
			}
			return _itemsTypeMember;
		}
	}

	public XamlPullParser(XamlParserContext context, XamlScanner scanner, XamlXmlReaderSettings settings)
	{
		_context = context;
		_xamlScanner = scanner;
		_settings = settings;
	}

	public IEnumerable<XamlNode> Parse()
	{
		_xamlScanner.Read();
		if (ProvideLineInfo)
		{
			yield return Logic_LineInfo();
		}
		for (ScannerNodeType nodeType = _xamlScanner.NodeType; nodeType == ScannerNodeType.PREFIXDEFINITION; nodeType = _xamlScanner.NodeType)
		{
			yield return Logic_PrefixDefinition();
			_xamlScanner.Read();
			if (ProvideLineInfo)
			{
				yield return Logic_LineInfo();
			}
		}
		foreach (XamlNode item in P_Element())
		{
			yield return item;
		}
	}

	public IEnumerable<XamlNode> P_Element()
	{
		ScannerNodeType nodeType = _xamlScanner.NodeType;
		switch (nodeType)
		{
		case ScannerNodeType.EMPTYELEMENT:
			foreach (XamlNode item in P_EmptyElement())
			{
				yield return item;
			}
			break;
		case ScannerNodeType.ELEMENT:
			foreach (XamlNode item2 in P_StartElement())
			{
				yield return item2;
			}
			foreach (XamlNode item3 in P_ElementBody())
			{
				yield return item3;
			}
			break;
		default:
			throw new XamlUnexpectedParseException(_xamlScanner, nodeType, SR.Get("ElementRuleException"));
		}
	}

	public IEnumerable<XamlNode> P_EmptyElement()
	{
		if (_xamlScanner.NodeType != ScannerNodeType.EMPTYELEMENT)
		{
			throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType, SR.Get("EmptyElementRuleException"));
		}
		yield return Logic_StartObject(_xamlScanner.Type, _xamlScanner.Namespace);
		_xamlScanner.Read();
		if (ProvideLineInfo)
		{
			yield return Logic_LineInfo();
		}
		while (_xamlScanner.NodeType == ScannerNodeType.DIRECTIVE)
		{
			foreach (XamlNode item in LogicStream_Attribute())
			{
				yield return item;
			}
			_xamlScanner.Read();
			if (ProvideLineInfo)
			{
				yield return Logic_LineInfo();
			}
		}
		while (_xamlScanner.NodeType == ScannerNodeType.ATTRIBUTE)
		{
			foreach (XamlNode item2 in LogicStream_Attribute())
			{
				yield return item2;
			}
			_xamlScanner.Read();
			if (ProvideLineInfo)
			{
				yield return Logic_LineInfo();
			}
		}
		yield return Logic_EndOfAttributes();
		yield return Logic_EndObject();
	}

	public IEnumerable<XamlNode> P_StartElement()
	{
		if (_xamlScanner.NodeType != ScannerNodeType.ELEMENT)
		{
			throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType, SR.Get("StartElementRuleException"));
		}
		yield return Logic_StartObject(_xamlScanner.Type, _xamlScanner.Namespace);
		_xamlScanner.Read();
		if (ProvideLineInfo)
		{
			yield return Logic_LineInfo();
		}
		while (_xamlScanner.NodeType == ScannerNodeType.DIRECTIVE)
		{
			foreach (XamlNode item in LogicStream_Attribute())
			{
				yield return item;
			}
			_xamlScanner.Read();
			if (ProvideLineInfo)
			{
				yield return Logic_LineInfo();
			}
		}
	}

	public IEnumerable<XamlNode> P_ElementBody()
	{
		while (_xamlScanner.NodeType == ScannerNodeType.ATTRIBUTE)
		{
			foreach (XamlNode item in LogicStream_Attribute())
			{
				yield return item;
			}
			_xamlScanner.Read();
			if (ProvideLineInfo)
			{
				yield return Logic_LineInfo();
			}
		}
		yield return Logic_EndOfAttributes();
		bool doneWithElementContent = false;
		bool hasContent = false;
		do
		{
			switch (_xamlScanner.NodeType)
			{
			case ScannerNodeType.PROPERTYELEMENT:
			case ScannerNodeType.EMPTYPROPERTYELEMENT:
				hasContent = true;
				foreach (XamlNode item2 in P_PropertyElement())
				{
					yield return item2;
				}
				break;
			case ScannerNodeType.ELEMENT:
			case ScannerNodeType.EMPTYELEMENT:
			case ScannerNodeType.PREFIXDEFINITION:
			case ScannerNodeType.TEXT:
			{
				hasContent = true;
				ScannerNodeType nodeType;
				do
				{
					foreach (XamlNode item3 in P_ElementContent())
					{
						yield return item3;
					}
					nodeType = _xamlScanner.NodeType;
				}
				while (nodeType == ScannerNodeType.PREFIXDEFINITION || nodeType == ScannerNodeType.ELEMENT || nodeType == ScannerNodeType.EMPTYELEMENT || nodeType == ScannerNodeType.TEXT);
				if (!_context.CurrentInItemsProperty && !_context.CurrentInInitProperty && !_context.CurrentInUnknownContent)
				{
					break;
				}
				yield return Logic_EndMember();
				if (_context.CurrentInCollectionFromMember)
				{
					yield return Logic_EndObject();
					yield return Logic_EndMember();
					_context.CurrentInCollectionFromMember = false;
					if (_context.CurrentInImplicitArray)
					{
						_context.CurrentInImplicitArray = false;
						yield return Logic_EndObject();
						yield return Logic_EndMember();
					}
				}
				break;
			}
			case ScannerNodeType.ENDTAG:
			{
				XamlType currentType = _context.CurrentType;
				bool flag = currentType.TypeConverter != null;
				bool flag2 = currentType.IsConstructible && !currentType.ConstructionRequiresArguments;
				if (!hasContent && flag && !flag2)
				{
					yield return Logic_StartInitProperty(currentType);
					yield return new XamlNode(XamlNodeType.Value, string.Empty);
					yield return Logic_EndMember();
				}
				doneWithElementContent = true;
				break;
			}
			default:
				doneWithElementContent = true;
				break;
			}
		}
		while (!doneWithElementContent);
		if (_xamlScanner.NodeType != ScannerNodeType.ENDTAG)
		{
			throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType, SR.Get("ElementBodyRuleException"));
		}
		yield return Logic_EndObject();
		_xamlScanner.Read();
		if (ProvideLineInfo)
		{
			yield return Logic_LineInfo();
		}
	}

	public IEnumerable<XamlNode> P_PropertyElement()
	{
		ScannerNodeType nodeType = _xamlScanner.NodeType;
		switch (nodeType)
		{
		case ScannerNodeType.EMPTYPROPERTYELEMENT:
			foreach (XamlNode item in P_EmptyPropertyElement())
			{
				yield return item;
			}
			break;
		case ScannerNodeType.PROPERTYELEMENT:
			foreach (XamlNode item2 in P_NonemptyPropertyElement())
			{
				yield return item2;
			}
			break;
		default:
			throw new XamlUnexpectedParseException(_xamlScanner, nodeType, SR.Get("PropertyElementRuleException"));
		}
	}

	public IEnumerable<XamlNode> P_EmptyPropertyElement()
	{
		if (_xamlScanner.NodeType != ScannerNodeType.EMPTYPROPERTYELEMENT)
		{
			throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType, SR.Get("EmptyPropertyElementRuleException"));
		}
		yield return Logic_StartMember(_xamlScanner.PropertyElement);
		yield return Logic_EndMember();
		_xamlScanner.Read();
		if (ProvideLineInfo)
		{
			yield return Logic_LineInfo();
		}
	}

	public IEnumerable<XamlNode> P_NonemptyPropertyElement()
	{
		if (_xamlScanner.NodeType != ScannerNodeType.PROPERTYELEMENT)
		{
			throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType, SR.Get("NonemptyPropertyElementRuleException"));
		}
		yield return Logic_StartMember(_xamlScanner.PropertyElement);
		_xamlScanner.Read();
		if (ProvideLineInfo)
		{
			yield return Logic_LineInfo();
		}
		bool doingPropertyContent = true;
		do
		{
			ScannerNodeType nodeType = _xamlScanner.NodeType;
			if ((uint)(nodeType - 1) <= 1u || nodeType == ScannerNodeType.PREFIXDEFINITION || nodeType == ScannerNodeType.TEXT)
			{
				do
				{
					foreach (XamlNode item in P_PropertyContent())
					{
						yield return item;
					}
					nodeType = _xamlScanner.NodeType;
				}
				while (nodeType == ScannerNodeType.PREFIXDEFINITION || nodeType == ScannerNodeType.ELEMENT || nodeType == ScannerNodeType.EMPTYELEMENT || nodeType == ScannerNodeType.TEXT);
				if (!_context.CurrentInItemsProperty && !_context.CurrentInInitProperty)
				{
					continue;
				}
				yield return Logic_EndMember();
				if (_context.CurrentInCollectionFromMember)
				{
					yield return Logic_EndObject();
					_context.CurrentInCollectionFromMember = false;
					if (_context.CurrentInImplicitArray)
					{
						_context.CurrentInImplicitArray = false;
						yield return Logic_EndMember();
						yield return Logic_EndObject();
					}
				}
			}
			else
			{
				doingPropertyContent = false;
			}
		}
		while (doingPropertyContent);
		if (_xamlScanner.NodeType != ScannerNodeType.ENDTAG)
		{
			throw new XamlUnexpectedParseException(_xamlScanner, _xamlScanner.NodeType, SR.Get("NonemptyPropertyElementRuleException"));
		}
		yield return Logic_EndMember();
		_xamlScanner.Read();
		if (ProvideLineInfo)
		{
			yield return Logic_LineInfo();
		}
	}

	public IEnumerable<XamlNode> P_ElementContent()
	{
		XamlType currentType = _context.CurrentType;
		List<XamlNode> savedPrefixDefinitions = null;
		ScannerNodeType nodeType = _xamlScanner.NodeType;
		ScannerNodeType scannerNodeType = nodeType;
		if ((uint)(scannerNodeType - 1) > 1u && scannerNodeType != ScannerNodeType.PREFIXDEFINITION && scannerNodeType != ScannerNodeType.TEXT)
		{
			yield break;
		}
		if (nodeType == ScannerNodeType.TEXT)
		{
			XamlText textContent = _xamlScanner.TextContent;
			if (Logic_IsDiscardableWhitespace(textContent))
			{
				_xamlScanner.Read();
				if (ProvideLineInfo)
				{
					yield return Logic_LineInfo();
				}
				yield break;
			}
		}
		while (nodeType == ScannerNodeType.PREFIXDEFINITION)
		{
			if (savedPrefixDefinitions == null)
			{
				savedPrefixDefinitions = new List<XamlNode>();
			}
			if (ProvideLineInfo)
			{
				savedPrefixDefinitions.Add(Logic_LineInfo());
			}
			savedPrefixDefinitions.Add(Logic_PrefixDefinition());
			_xamlScanner.Read();
			if (ProvideLineInfo)
			{
				yield return Logic_LineInfo();
			}
			nodeType = _xamlScanner.NodeType;
		}
		bool isTextInitialization = false;
		if (!_context.CurrentInItemsProperty && !_context.CurrentInUnknownContent)
		{
			bool isContentProperty = false;
			if (nodeType == ScannerNodeType.TEXT)
			{
				if (currentType.ContentProperty != null && CanAcceptString(currentType.ContentProperty))
				{
					isContentProperty = true;
				}
				else if (!_context.CurrentForcedToUseConstructor && !_xamlScanner.TextContent.IsEmpty && currentType.TypeConverter != null)
				{
					isTextInitialization = true;
				}
			}
			if (!isTextInitialization && !isContentProperty)
			{
				if (currentType.IsCollection || currentType.IsDictionary)
				{
					yield return Logic_StartItemsProperty(currentType);
				}
				else
				{
					isContentProperty = true;
				}
			}
			if (isContentProperty && !_context.CurrentInUnknownContent)
			{
				XamlMember xamlMember = currentType.ContentProperty;
				if (xamlMember != null && !_context.IsVisible(xamlMember, _context.CurrentTypeIsRoot ? _context.CurrentType : null))
				{
					xamlMember = new XamlMember(xamlMember.Name, currentType, isAttachable: false);
				}
				yield return Logic_StartContentProperty(xamlMember);
				foreach (XamlNode item in LogicStream_CheckForStartGetCollectionFromMember())
				{
					yield return item;
				}
			}
		}
		if (savedPrefixDefinitions != null)
		{
			for (int i = 0; i < savedPrefixDefinitions.Count; i++)
			{
				yield return savedPrefixDefinitions[i];
			}
			if (ProvideLineInfo)
			{
				yield return Logic_LineInfo();
			}
		}
		if (nodeType == ScannerNodeType.TEXT)
		{
			XamlText textContent2 = _xamlScanner.TextContent;
			string trimmed = Logic_ApplyFinalTextTrimming(textContent2);
			bool isContentProperty = _xamlScanner.IsXDataText;
			_xamlScanner.Read();
			if (ProvideLineInfo)
			{
				yield return Logic_LineInfo();
			}
			if (trimmed == string.Empty)
			{
				yield break;
			}
			if (isTextInitialization)
			{
				yield return Logic_StartInitProperty(currentType);
			}
			if (isContentProperty)
			{
				yield return Logic_StartObject(XamlLanguage.XData, null);
				XamlMember xDataTextProperty = XamlLanguage.XData.GetMember("Text");
				yield return Logic_EndOfAttributes();
				yield return Logic_StartMember(xDataTextProperty);
			}
			yield return new XamlNode(XamlNodeType.Value, trimmed);
			if (isContentProperty)
			{
				yield return Logic_EndMember();
				yield return Logic_EndObject();
			}
		}
		else
		{
			foreach (XamlNode item2 in P_Element())
			{
				yield return item2;
			}
		}
		if (!_context.CurrentInItemsProperty && !_context.CurrentInUnknownContent)
		{
			yield return Logic_EndMember();
		}
	}

	public IEnumerable<XamlNode> P_PropertyContent()
	{
		ScannerNodeType nodeType = _xamlScanner.NodeType;
		List<XamlNode> _savedPrefixDefinitions = null;
		string trimmed = string.Empty;
		bool isTextXML = false;
		ScannerNodeType scannerNodeType = nodeType;
		if ((uint)(scannerNodeType - 1) > 1u && scannerNodeType != ScannerNodeType.PREFIXDEFINITION && scannerNodeType != ScannerNodeType.TEXT)
		{
			yield break;
		}
		if (nodeType == ScannerNodeType.TEXT)
		{
			XamlText textContent = _xamlScanner.TextContent;
			trimmed = ((!Logic_IsDiscardableWhitespace(textContent)) ? Logic_ApplyFinalTextTrimming(textContent) : string.Empty);
			isTextXML = _xamlScanner.IsXDataText;
			_xamlScanner.Read();
			if (ProvideLineInfo)
			{
				yield return Logic_LineInfo();
			}
			if (trimmed == string.Empty)
			{
				yield break;
			}
		}
		while (true)
		{
			switch (nodeType)
			{
			case ScannerNodeType.PREFIXDEFINITION:
				if (_savedPrefixDefinitions == null)
				{
					_savedPrefixDefinitions = new List<XamlNode>();
				}
				_savedPrefixDefinitions.Add(Logic_PrefixDefinition());
				if (ProvideLineInfo)
				{
					_savedPrefixDefinitions.Add(Logic_LineInfo());
				}
				_xamlScanner.Read();
				if (ProvideLineInfo)
				{
					yield return Logic_LineInfo();
				}
				goto IL_01a6;
			case ScannerNodeType.TEXT:
				if (_context.CurrentMember.TypeConverter != null)
				{
					yield return new XamlNode(XamlNodeType.Value, trimmed);
					yield break;
				}
				break;
			}
			break;
			IL_01a6:
			nodeType = _xamlScanner.NodeType;
		}
		if (!_context.CurrentInCollectionFromMember)
		{
			foreach (XamlNode item in LogicStream_CheckForStartGetCollectionFromMember())
			{
				yield return item;
			}
		}
		if (nodeType == ScannerNodeType.TEXT)
		{
			if (isTextXML)
			{
				yield return Logic_StartObject(XamlLanguage.XData, null);
				XamlMember xDataTextProperty = XamlLanguage.XData.GetMember("Text");
				yield return Logic_EndOfAttributes();
				yield return Logic_StartMember(xDataTextProperty);
			}
			yield return new XamlNode(XamlNodeType.Value, trimmed);
			if (isTextXML)
			{
				yield return Logic_EndMember();
				yield return Logic_EndObject();
			}
			yield break;
		}
		if (_savedPrefixDefinitions != null)
		{
			for (int i = 0; i < _savedPrefixDefinitions.Count; i++)
			{
				yield return _savedPrefixDefinitions[i];
			}
		}
		foreach (XamlNode item2 in P_Element())
		{
			yield return item2;
		}
	}

	private XamlNode Logic_LineInfo()
	{
		LineInfo lineInfo = new LineInfo(LineNumber, LinePosition);
		return new XamlNode(lineInfo);
	}

	private XamlNode Logic_PrefixDefinition()
	{
		string prefix = _xamlScanner.Prefix;
		string ns = _xamlScanner.Namespace;
		return new XamlNode(XamlNodeType.NamespaceDeclaration, new NamespaceDeclaration(ns, prefix));
	}

	private XamlNode Logic_StartObject(XamlType xamlType, string xamlNamespace)
	{
		_context.PushScope();
		_context.CurrentType = xamlType;
		_context.CurrentTypeNamespace = xamlNamespace;
		return new XamlNode(XamlNodeType.StartObject, xamlType);
	}

	private XamlNode Logic_EndObject()
	{
		XamlType currentType = _context.CurrentType;
		_context.PopScope();
		_context.CurrentPreviousChildType = currentType;
		return new XamlNode(XamlNodeType.EndObject);
	}

	private IEnumerable<XamlNode> LogicStream_Attribute()
	{
		XamlMember propertyAttribute = _xamlScanner.PropertyAttribute;
		XamlText text = _xamlScanner.PropertyAttributeText;
		if (_xamlScanner.IsCtorForcingMember)
		{
			_context.CurrentForcedToUseConstructor = true;
		}
		yield return new XamlNode(XamlNodeType.StartMember, propertyAttribute);
		if (text.LooksLikeAMarkupExtension)
		{
			MePullParser mePullParser = new MePullParser(_context);
			foreach (XamlNode item in mePullParser.Parse(text.Text, LineNumber, LinePosition))
			{
				yield return item;
			}
		}
		else
		{
			yield return new XamlNode(XamlNodeType.Value, text.AttributeText);
		}
		yield return new XamlNode(XamlNodeType.EndMember);
	}

	private XamlNode Logic_EndOfAttributes()
	{
		return new XamlNode(XamlNode.InternalNodeType.EndOfAttributes);
	}

	private XamlNode Logic_StartMember(XamlMember member)
	{
		_context.CurrentMember = member;
		if (_xamlScanner.IsCtorForcingMember)
		{
			_context.CurrentForcedToUseConstructor = true;
		}
		XamlType type = member.Type;
		_context.CurrentInContainerDirective = member.IsDirective && type != null && (type.IsCollection || type.IsDictionary);
		return new XamlNode(XamlNodeType.StartMember, member);
	}

	private XamlNode Logic_EndMember()
	{
		_context.CurrentMember = null;
		_context.CurrentPreviousChildType = null;
		_context.CurrentInContainerDirective = false;
		return new XamlNode(XamlNodeType.EndMember);
	}

	private XamlNode Logic_StartContentProperty(XamlMember property)
	{
		if (property == null)
		{
			property = XamlLanguage.UnknownContent;
		}
		_context.CurrentMember = property;
		return new XamlNode(XamlNodeType.StartMember, property);
	}

	private XamlNode Logic_StartInitProperty(XamlType ownerType)
	{
		XamlDirective initialization = XamlLanguage.Initialization;
		_context.CurrentMember = initialization;
		return new XamlNode(XamlNodeType.StartMember, initialization);
	}

	private string Logic_ApplyFinalTextTrimming(XamlText text)
	{
		ScannerNodeType peekNodeType = _xamlScanner.PeekNodeType;
		string text2 = text.Text;
		if (!text.IsSpacePreserved)
		{
			if (peekNodeType == ScannerNodeType.ENDTAG || peekNodeType == ScannerNodeType.PROPERTYELEMENT || peekNodeType == ScannerNodeType.EMPTYPROPERTYELEMENT)
			{
				text2 = XamlText.TrimTrailingWhitespace(text2);
			}
			XamlType currentPreviousChildType = _context.CurrentPreviousChildType;
			if (currentPreviousChildType == null || currentPreviousChildType.TrimSurroundingWhitespace)
			{
				text2 = XamlText.TrimLeadingWhitespace(text2);
			}
			if (peekNodeType == ScannerNodeType.ELEMENT || peekNodeType == ScannerNodeType.EMPTYELEMENT)
			{
				XamlType peekType = _xamlScanner.PeekType;
				if (peekType.TrimSurroundingWhitespace)
				{
					text2 = XamlText.TrimTrailingWhitespace(text2);
				}
			}
		}
		return text2;
	}

	private XamlNode Logic_StartGetObjectFromMember(XamlType realType)
	{
		_context.PushScope();
		_context.CurrentType = realType;
		_context.CurrentInCollectionFromMember = true;
		return new XamlNode(XamlNodeType.GetObject);
	}

	private XamlNode Logic_StartItemsProperty(XamlType collectionType)
	{
		_context.CurrentMember = XamlLanguage.Items;
		_context.CurrentInContainerDirective = true;
		return new XamlNode(XamlNodeType.StartMember, XamlLanguage.Items);
	}

	private IEnumerable<XamlNode> LogicStream_CheckForStartGetCollectionFromMember()
	{
		_ = _context.CurrentType;
		XamlMember currentMember = _context.CurrentMember;
		XamlType propertyType = currentMember.Type;
		XamlType valueElementType = ((_xamlScanner.NodeType == ScannerNodeType.TEXT) ? XamlLanguage.String : _xamlScanner.Type);
		if (propertyType.IsArray && _xamlScanner.Type != ArrayExtensionType)
		{
			IEnumerable<NamespaceDeclaration> newNamespaces = null;
			XamlTypeName xamlTypeName = new XamlTypeName(propertyType.ItemType);
			INamespacePrefixLookup namespacePrefixLookup = new NamespacePrefixLookup(out newNamespaces, _context.FindNamespaceByPrefix);
			string typeNameString = xamlTypeName.ToString(namespacePrefixLookup);
			foreach (NamespaceDeclaration item in newNamespaces)
			{
				yield return new XamlNode(XamlNodeType.NamespaceDeclaration, item);
			}
			yield return Logic_StartObject(ArrayExtensionType, null);
			_context.CurrentInImplicitArray = true;
			yield return Logic_StartMember(ArrayTypeMember);
			yield return new XamlNode(XamlNodeType.Value, typeNameString);
			yield return Logic_EndMember();
			yield return Logic_EndOfAttributes();
			yield return Logic_StartMember(ItemsTypeMember);
			_ = _context.CurrentType;
			currentMember = _context.CurrentMember;
			propertyType = currentMember.Type;
		}
		if (currentMember.IsDirective || (!propertyType.IsCollection && !propertyType.IsDictionary))
		{
			yield break;
		}
		bool flag = false;
		if (currentMember.IsReadOnly || !_context.CurrentMemberIsWriteVisible())
		{
			flag = true;
		}
		else if (propertyType.TypeConverter != null && !currentMember.IsReadOnly && _xamlScanner.NodeType == ScannerNodeType.TEXT)
		{
			flag = false;
		}
		else if ((valueElementType == null || !valueElementType.CanAssignTo(propertyType)) && valueElementType != null)
		{
			if (!valueElementType.IsMarkupExtension || _xamlScanner.HasKeyAttribute)
			{
				flag = true;
			}
			else if (valueElementType == XamlLanguage.Array)
			{
				flag = true;
			}
		}
		if (flag)
		{
			yield return Logic_StartGetObjectFromMember(propertyType);
			yield return Logic_StartItemsProperty(propertyType);
		}
	}

	private bool Logic_IsDiscardableWhitespace(XamlText text)
	{
		if (!text.IsWhiteSpaceOnly)
		{
			return false;
		}
		if (_context.CurrentMember != null && _context.CurrentMember.IsUnknown)
		{
			return false;
		}
		if (_context.CurrentInContainerDirective)
		{
			XamlType xamlType = ((_context.CurrentMember == XamlLanguage.Items) ? _context.CurrentType : _context.CurrentMember.Type);
			if (xamlType.IsWhitespaceSignificantCollection)
			{
				return false;
			}
		}
		else
		{
			XamlMember xamlMember = _context.CurrentMember;
			if (_xamlScanner.PeekNodeType == ScannerNodeType.ELEMENT)
			{
				if (xamlMember == null)
				{
					xamlMember = _context.CurrentType.ContentProperty;
				}
				if (xamlMember != null && xamlMember.Type != null && xamlMember.Type.IsWhitespaceSignificantCollection)
				{
					return false;
				}
				if (xamlMember == null && _context.CurrentType.IsWhitespaceSignificantCollection)
				{
					return false;
				}
			}
			else if (text.IsSpacePreserved && _xamlScanner.PeekNodeType == ScannerNodeType.ENDTAG)
			{
				if (xamlMember != null)
				{
					if (_context.CurrentPreviousChildType == null)
					{
						return false;
					}
				}
				else if (_context.CurrentType.ContentProperty != null)
				{
					xamlMember = _context.CurrentType.ContentProperty;
					if (xamlMember.Type == XamlLanguage.String)
					{
						return false;
					}
					if (xamlMember.Type.IsWhitespaceSignificantCollection)
					{
						return false;
					}
				}
				else if (_context.CurrentType.TypeConverter != null && !_context.CurrentForcedToUseConstructor)
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool CanAcceptString(XamlMember property)
	{
		if (property == null)
		{
			return false;
		}
		if (property.TypeConverter == BuiltInValueConverter.String)
		{
			return true;
		}
		if (property.TypeConverter == BuiltInValueConverter.Object)
		{
			return true;
		}
		XamlType type = property.Type;
		if (type.IsCollection)
		{
			foreach (XamlType allowedContentType in type.AllowedContentTypes)
			{
				if (allowedContentType == XamlLanguage.String || allowedContentType == XamlLanguage.Object)
				{
					return true;
				}
			}
		}
		return false;
	}
}
