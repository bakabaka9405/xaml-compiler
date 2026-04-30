using System.Xaml;

namespace MS.Internal.Xaml.Context;

internal class XamlObjectWriterFactory : IXamlObjectWriterFactory
{
	private XamlSavedContext _savedContext;

	private XamlObjectWriterSettings _parentSettings;

	public XamlObjectWriterFactory(ObjectWriterContext context)
	{
		_savedContext = context.GetSavedContext(SavedContextType.Template);
		_parentSettings = context.ServiceProvider_GetSettings();
	}

	public XamlObjectWriter GetXamlObjectWriter(XamlObjectWriterSettings settings)
	{
		return new XamlObjectWriter(_savedContext, settings);
	}

	public XamlObjectWriterSettings GetParentSettings()
	{
		return new XamlObjectWriterSettings(_parentSettings);
	}
}
