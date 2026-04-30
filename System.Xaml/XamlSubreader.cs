namespace System.Xaml;

internal class XamlSubreader : XamlReader, IXamlLineInfo
{
	private XamlReader _reader;

	private IXamlLineInfo _lineInfoReader;

	private bool _done;

	private bool _firstRead;

	private bool _rootIsStartMember;

	private int _depth;

	private bool IsEmpty
	{
		get
		{
			if (!_done)
			{
				return _firstRead;
			}
			return true;
		}
	}

	public override XamlNodeType NodeType
	{
		get
		{
			if (!IsEmpty)
			{
				return _reader.NodeType;
			}
			return XamlNodeType.None;
		}
	}

	public override bool IsEof
	{
		get
		{
			if (!IsEmpty)
			{
				return _reader.IsEof;
			}
			return true;
		}
	}

	public override NamespaceDeclaration Namespace
	{
		get
		{
			if (!IsEmpty)
			{
				return _reader.Namespace;
			}
			return null;
		}
	}

	public override XamlType Type
	{
		get
		{
			if (!IsEmpty)
			{
				return _reader.Type;
			}
			return null;
		}
	}

	public override object Value
	{
		get
		{
			if (!IsEmpty)
			{
				return _reader.Value;
			}
			return null;
		}
	}

	public override XamlMember Member
	{
		get
		{
			if (!IsEmpty)
			{
				return _reader.Member;
			}
			return null;
		}
	}

	public override XamlSchemaContext SchemaContext => _reader.SchemaContext;

	public bool HasLineInfo
	{
		get
		{
			if (_lineInfoReader == null)
			{
				return false;
			}
			return _lineInfoReader.HasLineInfo;
		}
	}

	public int LineNumber
	{
		get
		{
			if (_lineInfoReader == null)
			{
				return 0;
			}
			return _lineInfoReader.LineNumber;
		}
	}

	public int LinePosition
	{
		get
		{
			if (_lineInfoReader == null)
			{
				return 0;
			}
			return _lineInfoReader.LinePosition;
		}
	}

	public XamlSubreader(XamlReader reader)
	{
		_reader = reader;
		_lineInfoReader = reader as IXamlLineInfo;
		_done = false;
		_depth = 0;
		_firstRead = true;
		_rootIsStartMember = reader.NodeType == XamlNodeType.StartMember;
	}

	public override bool Read()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException("XamlReader");
		}
		if (!_firstRead)
		{
			return LimitedRead();
		}
		_firstRead = false;
		return true;
	}

	private bool LimitedRead()
	{
		if (IsEof)
		{
			return false;
		}
		XamlNodeType nodeType = _reader.NodeType;
		if (_rootIsStartMember)
		{
			switch (nodeType)
			{
			case XamlNodeType.StartMember:
				_depth++;
				break;
			case XamlNodeType.EndMember:
				_depth--;
				break;
			}
		}
		else
		{
			switch (nodeType)
			{
			case XamlNodeType.StartObject:
			case XamlNodeType.GetObject:
				_depth++;
				break;
			case XamlNodeType.EndObject:
				_depth--;
				break;
			}
		}
		if (_depth == 0)
		{
			_done = true;
		}
		_reader.Read();
		return !IsEof;
	}
}
