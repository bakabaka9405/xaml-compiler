namespace System.Xaml;

public class XamlXmlWriterSettings : XamlWriterSettings
{
	public bool AssumeValidInput { get; set; }

	public bool CloseOutput { get; set; }

	public XamlXmlWriterSettings Copy()
	{
		return new XamlXmlWriterSettings
		{
			AssumeValidInput = AssumeValidInput,
			CloseOutput = CloseOutput
		};
	}
}
