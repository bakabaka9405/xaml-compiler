namespace System.Xaml;

public class AmbientPropertyValue
{
	private XamlMember _property;

	private object _value;

	public object Value => _value;

	public XamlMember RetrievedProperty => _property;

	public AmbientPropertyValue(XamlMember property, object value)
	{
		_property = property;
		_value = value;
	}
}
