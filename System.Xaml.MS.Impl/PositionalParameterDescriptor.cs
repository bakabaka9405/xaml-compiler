namespace System.Xaml.MS.Impl;

internal class PositionalParameterDescriptor
{
	public object Value { get; set; }

	public bool WasText { get; set; }

	public PositionalParameterDescriptor(object value, bool wasText)
	{
		Value = value;
		WasText = wasText;
	}
}
