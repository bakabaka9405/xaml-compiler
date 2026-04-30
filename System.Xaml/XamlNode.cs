using System.Diagnostics;

namespace System.Xaml;

[DebuggerDisplay("{ToString()}")]
internal struct XamlNode
{
	internal enum InternalNodeType : byte
	{
		None,
		StartOfStream,
		EndOfStream,
		EndOfAttributes,
		LineInfo
	}

	private XamlNodeType _nodeType;

	private InternalNodeType _internalNodeType;

	private object _data;

	public XamlNodeType NodeType => _nodeType;

	public NamespaceDeclaration NamespaceDeclaration
	{
		get
		{
			if (NodeType == XamlNodeType.NamespaceDeclaration)
			{
				return (NamespaceDeclaration)_data;
			}
			return null;
		}
	}

	public XamlType XamlType
	{
		get
		{
			if (NodeType == XamlNodeType.StartObject)
			{
				return (XamlType)_data;
			}
			return null;
		}
	}

	public object Value
	{
		get
		{
			if (NodeType == XamlNodeType.Value)
			{
				return _data;
			}
			return null;
		}
	}

	public XamlMember Member
	{
		get
		{
			if (NodeType == XamlNodeType.StartMember)
			{
				return (XamlMember)_data;
			}
			return null;
		}
	}

	public LineInfo LineInfo
	{
		get
		{
			if (NodeType == XamlNodeType.None)
			{
				return _data as LineInfo;
			}
			return null;
		}
	}

	internal bool IsEof
	{
		get
		{
			if (NodeType == XamlNodeType.None && _internalNodeType == InternalNodeType.EndOfStream)
			{
				return true;
			}
			return false;
		}
	}

	internal bool IsEndOfAttributes
	{
		get
		{
			if (NodeType == XamlNodeType.None && _internalNodeType == InternalNodeType.EndOfAttributes)
			{
				return true;
			}
			return false;
		}
	}

	internal bool IsLineInfo
	{
		get
		{
			if (NodeType == XamlNodeType.None && _internalNodeType == InternalNodeType.LineInfo)
			{
				return true;
			}
			return false;
		}
	}

	public XamlNode(XamlNodeType nodeType)
	{
		_nodeType = nodeType;
		_internalNodeType = InternalNodeType.None;
		_data = null;
	}

	public XamlNode(XamlNodeType nodeType, object data)
	{
		_nodeType = nodeType;
		_internalNodeType = InternalNodeType.None;
		_data = data;
	}

	public XamlNode(InternalNodeType internalNodeType)
	{
		_nodeType = XamlNodeType.None;
		_internalNodeType = internalNodeType;
		_data = null;
	}

	public XamlNode(LineInfo lineInfo)
	{
		_nodeType = XamlNodeType.None;
		_internalNodeType = InternalNodeType.LineInfo;
		_data = lineInfo;
	}

	public override string ToString()
	{
		string text = string.Format(TypeConverterHelper.InvariantEnglishUS, "{0}: ", NodeType);
		switch (NodeType)
		{
		case XamlNodeType.StartObject:
			text += XamlType.Name;
			break;
		case XamlNodeType.StartMember:
			text += Member.Name;
			break;
		case XamlNodeType.Value:
			text += Value.ToString();
			break;
		case XamlNodeType.NamespaceDeclaration:
			text += NamespaceDeclaration.ToString();
			break;
		case XamlNodeType.None:
			switch (_internalNodeType)
			{
			case InternalNodeType.EndOfAttributes:
				text += "End Of Attributes";
				break;
			case InternalNodeType.StartOfStream:
				text += "Start Of Stream";
				break;
			case InternalNodeType.EndOfStream:
				text += "End Of Stream";
				break;
			case InternalNodeType.LineInfo:
				text = text + "LineInfo: " + LineInfo.ToString();
				break;
			}
			break;
		}
		return text;
	}

	internal static bool IsEof_Helper(XamlNodeType nodeType, object data)
	{
		if (nodeType != XamlNodeType.None)
		{
			return false;
		}
		if (data is InternalNodeType internalNodeType && internalNodeType == InternalNodeType.EndOfStream)
		{
			return true;
		}
		return false;
	}
}
