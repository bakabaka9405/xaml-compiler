using System.Windows.Markup;
using System.Xaml.Permissions;

namespace System.Xaml;

public class XamlObjectWriterSettings : XamlWriterSettings
{
	public EventHandler<XamlObjectEventArgs> AfterBeginInitHandler { get; set; }

	public EventHandler<XamlObjectEventArgs> BeforePropertiesHandler { get; set; }

	public EventHandler<XamlObjectEventArgs> AfterPropertiesHandler { get; set; }

	public EventHandler<XamlObjectEventArgs> AfterEndInitHandler { get; set; }

	public EventHandler<XamlSetValueEventArgs> XamlSetValueHandler { get; set; }

	public object RootObjectInstance { get; set; }

	public bool IgnoreCanConvert { get; set; }

	public INameScope ExternalNameScope { get; set; }

	public bool SkipDuplicatePropertyCheck { get; set; }

	public bool RegisterNamesOnExternalNamescope { get; set; }

	public bool SkipProvideValueOnRoot { get; set; }

	public bool PreferUnconvertedDictionaryKeys { get; set; }

	public Uri SourceBamlUri { get; set; }

	public XamlAccessLevel AccessLevel { get; set; }

	public XamlObjectWriterSettings()
	{
	}

	public XamlObjectWriterSettings(XamlObjectWriterSettings settings)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		AfterBeginInitHandler = settings.AfterBeginInitHandler;
		BeforePropertiesHandler = settings.BeforePropertiesHandler;
		AfterPropertiesHandler = settings.AfterPropertiesHandler;
		AfterEndInitHandler = settings.AfterEndInitHandler;
		XamlSetValueHandler = settings.XamlSetValueHandler;
		RootObjectInstance = settings.RootObjectInstance;
		IgnoreCanConvert = settings.IgnoreCanConvert;
		ExternalNameScope = settings.ExternalNameScope;
		SkipDuplicatePropertyCheck = settings.SkipDuplicatePropertyCheck;
		RegisterNamesOnExternalNamescope = settings.RegisterNamesOnExternalNamescope;
		AccessLevel = settings.AccessLevel;
		SkipProvideValueOnRoot = settings.SkipProvideValueOnRoot;
		PreferUnconvertedDictionaryKeys = settings.PreferUnconvertedDictionaryKeys;
		SourceBamlUri = settings.SourceBamlUri;
	}

	internal XamlObjectWriterSettings StripDelegates()
	{
		XamlObjectWriterSettings xamlObjectWriterSettings = new XamlObjectWriterSettings(this);
		xamlObjectWriterSettings.AfterBeginInitHandler = null;
		xamlObjectWriterSettings.AfterEndInitHandler = null;
		xamlObjectWriterSettings.AfterPropertiesHandler = null;
		xamlObjectWriterSettings.BeforePropertiesHandler = null;
		xamlObjectWriterSettings.XamlSetValueHandler = null;
		return xamlObjectWriterSettings;
	}
}
