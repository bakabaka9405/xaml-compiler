namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class XamlSetMarkupExtensionAttribute : Attribute
{
	public string XamlSetMarkupExtensionHandler { get; private set; }

	public XamlSetMarkupExtensionAttribute(string xamlSetMarkupExtensionHandler)
	{
		XamlSetMarkupExtensionHandler = xamlSetMarkupExtensionHandler;
	}
}
