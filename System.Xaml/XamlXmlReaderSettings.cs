using System.Collections.Generic;

namespace System.Xaml;

public class XamlXmlReaderSettings : XamlReaderSettings
{
	internal Dictionary<string, string> _xmlnsDictionary;

	public string XmlLang { get; set; }

	public bool XmlSpacePreserve { get; set; }

	public bool SkipXmlCompatibilityProcessing { get; set; }

	public bool CloseInput { get; set; }

	public XamlXmlReaderSettings()
	{
	}

	public XamlXmlReaderSettings(XamlXmlReaderSettings settings)
		: base(settings)
	{
		if (settings != null)
		{
			if (settings._xmlnsDictionary != null)
			{
				_xmlnsDictionary = new Dictionary<string, string>(settings._xmlnsDictionary);
			}
			XmlLang = settings.XmlLang;
			XmlSpacePreserve = settings.XmlSpacePreserve;
			SkipXmlCompatibilityProcessing = settings.SkipXmlCompatibilityProcessing;
			CloseInput = settings.CloseInput;
		}
	}
}
