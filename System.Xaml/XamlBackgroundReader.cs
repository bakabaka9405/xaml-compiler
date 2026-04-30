using System.Threading;

namespace System.Xaml;

public class XamlBackgroundReader : XamlReader, IXamlLineInfo
{
	private EventWaitHandle _providerFullEvent;

	private EventWaitHandle _dataReceivedEvent;

	private XamlNode[] _incoming;

	private int _inIdx;

	private XamlNode[] _outgoing;

	private int _outIdx;

	private int _outValid;

	private XamlNode _currentNode;

	private XamlReader _wrappedReader;

	private XamlReader _internalReader;

	private XamlWriter _writer;

	private bool _wrappedReaderHasLineInfo;

	private int _lineNumber;

	private int _linePosition;

	private Thread _thread;

	private Exception _caughtException;

	internal bool IncomingFull => _inIdx >= _incoming.Length;

	internal bool OutgoingEmpty => _outIdx >= _outValid;

	public override XamlNodeType NodeType => _internalReader.NodeType;

	public override bool IsEof => _internalReader.IsEof;

	public override NamespaceDeclaration Namespace => _internalReader.Namespace;

	public override XamlType Type => _internalReader.Type;

	public override object Value => _internalReader.Value;

	public override XamlMember Member => _internalReader.Member;

	public override XamlSchemaContext SchemaContext => _internalReader.SchemaContext;

	public bool HasLineInfo => _wrappedReaderHasLineInfo;

	public int LineNumber => _lineNumber;

	public int LinePosition => _linePosition;

	public XamlBackgroundReader(XamlReader wrappedReader)
	{
		if (wrappedReader == null)
		{
			throw new ArgumentNullException("wrappedReader");
		}
		Initialize(wrappedReader, 64);
	}

	private void Initialize(XamlReader wrappedReader, int bufferSize)
	{
		_providerFullEvent = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);
		_dataReceivedEvent = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);
		_incoming = new XamlNode[bufferSize];
		_outgoing = new XamlNode[bufferSize];
		_wrappedReader = wrappedReader;
		_wrappedReaderHasLineInfo = ((IXamlLineInfo)_wrappedReader).HasLineInfo;
		XamlNodeAddDelegate add = Add;
		XamlLineInfoAddDelegate addlineInfoDelegate = null;
		if (_wrappedReaderHasLineInfo)
		{
			addlineInfoDelegate = AddLineInfo;
		}
		_writer = new WriterDelegate(add, addlineInfoDelegate, _wrappedReader.SchemaContext);
		_internalReader = new ReaderDelegate(next: (!_wrappedReaderHasLineInfo) ? new XamlNodeNextDelegate(Next) : new XamlNodeNextDelegate(Next_ProcessLineInfo), schemaContext: _wrappedReader.SchemaContext, hasLineInfo: _wrappedReaderHasLineInfo);
		_currentNode = new XamlNode(XamlNode.InternalNodeType.StartOfStream);
	}

	public void StartThread()
	{
		StartThread("XAML reader thread");
	}

	public void StartThread(string threadName)
	{
		if (_thread != null)
		{
			throw new InvalidOperationException(SR.Get("ThreadAlreadyStarted"));
		}
		ParameterizedThreadStart start = XamlReaderThreadStart;
		_thread = new Thread(start);
		_thread.Name = threadName;
		_thread.Start();
	}

	private void XamlReaderThreadStart(object none)
	{
		try
		{
			InterruptableTransform(_wrappedReader, _writer, closeWriter: true);
		}
		catch (Exception caughtException)
		{
			_writer.Close();
			_caughtException = caughtException;
		}
	}

	private void SwapBuffers()
	{
		XamlNode[] incoming = _incoming;
		_incoming = _outgoing;
		_outgoing = incoming;
		_outIdx = 0;
		_outValid = _inIdx;
		_inIdx = 0;
	}

	private void AddToBuffer(XamlNode node)
	{
		_incoming[_inIdx] = node;
		_inIdx++;
		if (IncomingFull)
		{
			_providerFullEvent.Set();
			_dataReceivedEvent.WaitOne();
		}
	}

	private void Add(XamlNodeType nodeType, object data)
	{
		if (!base.IsDisposed)
		{
			if (nodeType != XamlNodeType.None)
			{
				AddToBuffer(new XamlNode(nodeType, data));
				return;
			}
			AddToBuffer(new XamlNode(XamlNode.InternalNodeType.EndOfStream));
			_providerFullEvent.Set();
		}
	}

	private void AddLineInfo(int lineNumber, int linePosition)
	{
		if (!base.IsDisposed)
		{
			LineInfo lineInfo = new LineInfo(lineNumber, linePosition);
			XamlNode node = new XamlNode(lineInfo);
			AddToBuffer(node);
		}
	}

	private XamlNode Next()
	{
		if (base.IsDisposed)
		{
			throw new ObjectDisposedException("XamlBackgroundReader");
		}
		if (OutgoingEmpty)
		{
			if (_currentNode.IsEof)
			{
				return _currentNode;
			}
			_providerFullEvent.WaitOne();
			SwapBuffers();
			_dataReceivedEvent.Set();
		}
		_currentNode = _outgoing[_outIdx++];
		if (_currentNode.IsEof && _thread != null)
		{
			_thread.Join();
			if (_caughtException != null)
			{
				Exception caughtException = _caughtException;
				_caughtException = null;
				throw caughtException;
			}
		}
		return _currentNode;
	}

	private XamlNode Next_ProcessLineInfo()
	{
		bool flag = false;
		while (!flag)
		{
			Next();
			if (_currentNode.IsLineInfo)
			{
				_lineNumber = _currentNode.LineInfo.LineNumber;
				_linePosition = _currentNode.LineInfo.LinePosition;
			}
			else
			{
				flag = true;
			}
		}
		return _currentNode;
	}

	private void InterruptableTransform(XamlReader reader, XamlWriter writer, bool closeWriter)
	{
		IXamlLineInfo xamlLineInfo = reader as IXamlLineInfo;
		IXamlLineInfoConsumer xamlLineInfoConsumer = writer as IXamlLineInfoConsumer;
		bool flag = false;
		if (xamlLineInfo != null && xamlLineInfo.HasLineInfo && xamlLineInfoConsumer != null && xamlLineInfoConsumer.ShouldProvideLineInfo)
		{
			flag = true;
		}
		while (reader.Read() && !base.IsDisposed)
		{
			if (flag && xamlLineInfo.LineNumber != 0)
			{
				xamlLineInfoConsumer.SetLineInfo(xamlLineInfo.LineNumber, xamlLineInfo.LinePosition);
			}
			writer.WriteNode(reader);
		}
		if (closeWriter)
		{
			writer.Close();
		}
	}

	public override bool Read()
	{
		return _internalReader.Read();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		_dataReceivedEvent.Set();
		((IDisposable)_dataReceivedEvent).Dispose();
		((IDisposable)_internalReader).Dispose();
		((IDisposable)_providerFullEvent).Dispose();
		((IDisposable)_writer).Dispose();
	}
}
