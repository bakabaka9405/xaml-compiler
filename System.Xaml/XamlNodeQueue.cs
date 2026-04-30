using System.Collections.Generic;

namespace System.Xaml;

public class XamlNodeQueue
{
	private Queue<XamlNode> _nodeQueue;

	private XamlNode _endOfStreamNode;

	private ReaderDelegate _reader;

	private XamlWriter _writer;

	private bool _hasLineInfo;

	public XamlReader Reader
	{
		get
		{
			if (_reader == null)
			{
				_reader = new ReaderDelegate(_writer.SchemaContext, Next, _hasLineInfo);
			}
			return _reader;
		}
	}

	public XamlWriter Writer => _writer;

	public bool IsEmpty => _nodeQueue.Count == 0;

	public int Count => _nodeQueue.Count;

	public XamlNodeQueue(XamlSchemaContext schemaContext)
	{
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		_nodeQueue = new Queue<XamlNode>();
		_endOfStreamNode = new XamlNode(XamlNode.InternalNodeType.EndOfStream);
		_writer = new WriterDelegate(Add, AddLineInfo, schemaContext);
	}

	private void Add(XamlNodeType nodeType, object data)
	{
		if (nodeType != XamlNodeType.None)
		{
			XamlNode item = new XamlNode(nodeType, data);
			_nodeQueue.Enqueue(item);
		}
		else
		{
			_nodeQueue.Enqueue(_endOfStreamNode);
		}
	}

	private void AddLineInfo(int lineNumber, int linePosition)
	{
		LineInfo lineInfo = new LineInfo(lineNumber, linePosition);
		XamlNode item = new XamlNode(lineInfo);
		_nodeQueue.Enqueue(item);
		if (!_hasLineInfo)
		{
			_hasLineInfo = true;
		}
		if (_reader != null && !_reader.HasLineInfo)
		{
			_reader.HasLineInfo = true;
		}
	}

	private XamlNode Next()
	{
		if (_nodeQueue.Count > 0)
		{
			return _nodeQueue.Dequeue();
		}
		return _endOfStreamNode;
	}
}
