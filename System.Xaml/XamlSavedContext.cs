using MS.Internal.Xaml.Context;

namespace System.Xaml;

internal class XamlSavedContext
{
	private XamlSchemaContext _context;

	private XamlContextStack<ObjectWriterFrame> _stack;

	private SavedContextType _savedContextType;

	public SavedContextType SaveContextType => _savedContextType;

	public XamlContextStack<ObjectWriterFrame> Stack => _stack;

	public XamlSchemaContext SchemaContext => _context;

	public Uri BaseUri { get; private set; }

	public XamlSavedContext(SavedContextType savedContextType, ObjectWriterContext owContext, XamlContextStack<ObjectWriterFrame> stack)
	{
		_savedContextType = savedContextType;
		_context = owContext.SchemaContext;
		_stack = stack;
		if (savedContextType == SavedContextType.Template)
		{
			stack.CurrentFrame.Instance = null;
		}
		BaseUri = owContext.BaseUri;
	}
}
