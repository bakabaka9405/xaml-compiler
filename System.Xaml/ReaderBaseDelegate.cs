namespace System.Xaml;

internal abstract class ReaderBaseDelegate : XamlReader, IXamlLineInfo
{
	protected XamlSchemaContext _schemaContext;

	protected XamlNode _currentNode;

	protected LineInfo _currentLineInfo;

	protected bool _hasLineInfo;

	public override XamlNodeType NodeType => _currentNode.NodeType;

	public override bool IsEof => _currentNode.IsEof;

	public override NamespaceDeclaration Namespace => _currentNode.NamespaceDeclaration;

	public override XamlType Type => _currentNode.XamlType;

	public override object Value => _currentNode.Value;

	public override XamlMember Member => _currentNode.Member;

	public override XamlSchemaContext SchemaContext => _schemaContext;

	public bool HasLineInfo
	{
		get
		{
			return _hasLineInfo;
		}
		set
		{
			_hasLineInfo = value;
		}
	}

	public int LineNumber
	{
		get
		{
			if (_currentLineInfo != null)
			{
				return _currentLineInfo.LineNumber;
			}
			return 0;
		}
	}

	public int LinePosition
	{
		get
		{
			if (_currentLineInfo != null)
			{
				return _currentLineInfo.LinePosition;
			}
			return 0;
		}
	}

	protected ReaderBaseDelegate(XamlSchemaContext schemaContext)
	{
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		_schemaContext = schemaContext;
	}
}
