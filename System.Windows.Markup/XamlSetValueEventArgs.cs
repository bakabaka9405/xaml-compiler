using System.Xaml;

namespace System.Windows.Markup;

public class XamlSetValueEventArgs : EventArgs
{
	public XamlMember Member { get; private set; }

	public object Value { get; private set; }

	public bool Handled { get; set; }

	public XamlSetValueEventArgs(XamlMember member, object value)
	{
		Value = value;
		Member = member;
	}

	public virtual void CallBase()
	{
	}
}
