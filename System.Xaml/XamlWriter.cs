namespace System.Xaml;

public abstract class XamlWriter : IDisposable
{
	public abstract XamlSchemaContext SchemaContext { get; }

	protected bool IsDisposed { get; private set; }

	public abstract void WriteGetObject();

	public abstract void WriteStartObject(XamlType type);

	public abstract void WriteEndObject();

	public abstract void WriteStartMember(XamlMember xamlMember);

	public abstract void WriteEndMember();

	public abstract void WriteValue(object value);

	public abstract void WriteNamespace(NamespaceDeclaration namespaceDeclaration);

	public void WriteNode(XamlReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		switch (reader.NodeType)
		{
		case XamlNodeType.NamespaceDeclaration:
			WriteNamespace(reader.Namespace);
			break;
		case XamlNodeType.StartObject:
			WriteStartObject(reader.Type);
			break;
		case XamlNodeType.GetObject:
			WriteGetObject();
			break;
		case XamlNodeType.EndObject:
			WriteEndObject();
			break;
		case XamlNodeType.StartMember:
			WriteStartMember(reader.Member);
			break;
		case XamlNodeType.EndMember:
			WriteEndMember();
			break;
		case XamlNodeType.Value:
			WriteValue(reader.Value);
			break;
		default:
			throw new NotImplementedException(SR.Get("MissingCaseXamlNodes"));
		case XamlNodeType.None:
			break;
		}
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
}
