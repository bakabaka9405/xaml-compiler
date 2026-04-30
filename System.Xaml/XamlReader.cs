namespace System.Xaml;

public abstract class XamlReader : IDisposable
{
	public abstract XamlNodeType NodeType { get; }

	public abstract bool IsEof { get; }

	public abstract NamespaceDeclaration Namespace { get; }

	public abstract XamlType Type { get; }

	public abstract object Value { get; }

	public abstract XamlMember Member { get; }

	public abstract XamlSchemaContext SchemaContext { get; }

	protected bool IsDisposed { get; private set; }

	public abstract bool Read();

	public virtual void Skip()
	{
		switch (NodeType)
		{
		case XamlNodeType.StartObject:
			SkipFromTo(XamlNodeType.StartObject, XamlNodeType.EndObject);
			break;
		case XamlNodeType.StartMember:
			SkipFromTo(XamlNodeType.StartMember, XamlNodeType.EndMember);
			break;
		}
		Read();
	}

	void IDisposable.Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		IsDisposed = true;
	}

	public void Close()
	{
		((IDisposable)this).Dispose();
	}

	public virtual XamlReader ReadSubtree()
	{
		return new XamlSubreader(this);
	}

	private void SkipFromTo(XamlNodeType startNodeType, XamlNodeType endNodeType)
	{
		int num = 1;
		while (num > 0)
		{
			Read();
			XamlNodeType nodeType = NodeType;
			if (nodeType == startNodeType)
			{
				num++;
			}
			else if (nodeType == endNodeType)
			{
				num--;
			}
		}
	}
}
