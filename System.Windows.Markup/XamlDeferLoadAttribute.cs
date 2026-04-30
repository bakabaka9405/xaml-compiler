namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class XamlDeferLoadAttribute : Attribute
{
	private string _contentTypeName;

	private string _loaderTypeName;

	public string LoaderTypeName => _loaderTypeName;

	public string ContentTypeName => _contentTypeName;

	public Type LoaderType { get; private set; }

	public Type ContentType { get; private set; }

	public XamlDeferLoadAttribute(Type loaderType, Type contentType)
	{
		if (loaderType == null)
		{
			throw new ArgumentNullException("loaderType");
		}
		if (contentType == null)
		{
			throw new ArgumentNullException("contentType");
		}
		_loaderTypeName = loaderType.AssemblyQualifiedName;
		_contentTypeName = contentType.AssemblyQualifiedName;
		LoaderType = loaderType;
		ContentType = contentType;
	}

	public XamlDeferLoadAttribute(string loaderType, string contentType)
	{
		if (loaderType == null)
		{
			throw new ArgumentNullException("loaderType");
		}
		if (contentType == null)
		{
			throw new ArgumentNullException("contentType");
		}
		_loaderTypeName = loaderType;
		_contentTypeName = contentType;
	}
}
