namespace System.Xaml;

internal class ReaderDelegate : ReaderBaseDelegate
{
	private XamlNodeNextDelegate _nextDelegate;

	public ReaderDelegate(XamlSchemaContext schemaContext, XamlNodeNextDelegate next, bool hasLineInfo)
		: base(schemaContext)
	{
		_nextDelegate = next;
		_currentNode = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
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
			_currentNode = _nextDelegate();
			if (_currentNode.NodeType != XamlNodeType.None)
			{
				return true;
			}
			if (_currentNode.IsLineInfo)
			{
				_currentLineInfo = _currentNode.LineInfo;
			}
			else if (_currentNode.IsEof)
			{
				break;
			}
		}
		while (_currentNode.NodeType == XamlNodeType.None);
		return !IsEof;
	}
}
