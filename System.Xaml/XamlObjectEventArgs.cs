namespace System.Xaml;

public class XamlObjectEventArgs : EventArgs
{
	public object Instance { get; private set; }

	public Uri SourceBamlUri { get; private set; }

	public int ElementLineNumber { get; private set; }

	public int ElementLinePosition { get; private set; }

	public XamlObjectEventArgs(object instance)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		Instance = instance;
	}

	internal XamlObjectEventArgs(object instance, Uri sourceBamlUri, int elementLineNumber, int elementLinePosition)
		: this(instance)
	{
		SourceBamlUri = sourceBamlUri;
		ElementLineNumber = elementLineNumber;
		ElementLinePosition = elementLinePosition;
	}
}
