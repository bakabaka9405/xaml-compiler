namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class XamlSetTypeConverterAttribute : Attribute
{
	public string XamlSetTypeConverterHandler { get; private set; }

	public XamlSetTypeConverterAttribute(string xamlSetTypeConverterHandler)
	{
		XamlSetTypeConverterHandler = xamlSetTypeConverterHandler;
	}
}
