namespace System.Xaml;

internal class ReaderMultiIndexDelegate : ReaderBaseDelegate, IXamlIndexingReader
{
	private static XamlNode s_StartOfStream = new XamlNode(XamlNode.InternalNodeType.StartOfStream);

	private static XamlNode s_EndOfStream = new XamlNode(XamlNode.InternalNodeType.EndOfStream);

	private XamlNodeIndexDelegate _indexDelegate;

	private int _count;

	private int _idx;

	public int Count => _count;

	public int CurrentIndex
	{
		get
		{
			return _idx;
		}
		set
		{
			if (value < -1 || value > _count)
			{
				throw new IndexOutOfRangeException();
			}
			if (value == -1)
			{
				_idx = -1;
				_currentNode = s_StartOfStream;
				_currentLineInfo = null;
			}
			else
			{
				_idx = value - 1;
				Read();
			}
		}
	}

	public ReaderMultiIndexDelegate(XamlSchemaContext schemaContext, XamlNodeIndexDelegate indexDelegate, int count, bool hasLineInfo)
		: base(schemaContext)
	{
		_indexDelegate = indexDelegate;
		_count = count;
		_idx = -1;
		_currentNode = s_StartOfStream;
		_currentLineInfo = null;
		_hasLineInfo = hasLineInfo;
	}

	public override bool Read()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException("XamlReader");
		}
		do
		{
			if (_idx < _count - 1)
			{
				_currentNode = _indexDelegate(++_idx);
				if (_currentNode.NodeType != XamlNodeType.None)
				{
					return true;
				}
				if (_currentNode.LineInfo != null)
				{
					_currentLineInfo = _currentNode.LineInfo;
				}
				else if (_currentNode.IsEof)
				{
					break;
				}
				continue;
			}
			_idx = _count;
			_currentNode = s_EndOfStream;
			_currentLineInfo = null;
			break;
		}
		while (_currentNode.NodeType == XamlNodeType.None);
		return !IsEof;
	}
}
