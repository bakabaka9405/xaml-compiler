using System.Collections.Generic;

namespace System.Xaml;

public class XamlNodeList
{
	private List<XamlNode> _nodeList;

	private bool _readMode;

	private XamlWriter _writer;

	private bool _hasLineInfo;

	public XamlWriter Writer => _writer;

	public int Count => _nodeList.Count;

	public XamlNodeList(XamlSchemaContext schemaContext)
	{
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		Initialize(schemaContext, 0);
	}

	public XamlNodeList(XamlSchemaContext schemaContext, int size)
	{
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		Initialize(schemaContext, size);
	}

	private void Initialize(XamlSchemaContext schemaContext, int size)
	{
		if (size == 0)
		{
			_nodeList = new List<XamlNode>();
		}
		else
		{
			_nodeList = new List<XamlNode>(size);
		}
		_writer = new WriterDelegate(Add, AddLineInfo, schemaContext);
	}

	public XamlReader GetReader()
	{
		if (!_readMode)
		{
			throw new XamlException(SR.Get("CloseXamlWriterBeforeReading"));
		}
		if (_writer.SchemaContext == null)
		{
			throw new XamlException(SR.Get("SchemaContextNotInitialized"));
		}
		return new ReaderMultiIndexDelegate(_writer.SchemaContext, Index, _nodeList.Count, _hasLineInfo);
	}

	private void Add(XamlNodeType nodeType, object data)
	{
		if (!_readMode)
		{
			if (nodeType != XamlNodeType.None)
			{
				XamlNode item = new XamlNode(nodeType, data);
				_nodeList.Add(item);
			}
			else
			{
				_readMode = true;
			}
			return;
		}
		throw new XamlException(SR.Get("CannotWriteClosedWriter"));
	}

	private void AddLineInfo(int lineNumber, int linePosition)
	{
		if (_readMode)
		{
			throw new XamlException(SR.Get("CannotWriteClosedWriter"));
		}
		XamlNode item = new XamlNode(new LineInfo(lineNumber, linePosition));
		_nodeList.Add(item);
		if (!_hasLineInfo)
		{
			_hasLineInfo = true;
		}
	}

	private XamlNode Index(int idx)
	{
		if (!_readMode)
		{
			throw new XamlException(SR.Get("CloseXamlWriterBeforeReading"));
		}
		return _nodeList[idx];
	}

	public void Clear()
	{
		_nodeList.Clear();
		_readMode = false;
	}
}
