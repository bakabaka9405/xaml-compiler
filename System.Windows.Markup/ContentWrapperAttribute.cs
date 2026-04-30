using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public sealed class ContentWrapperAttribute : Attribute
{
	private Type _contentWrapper;

	public Type ContentWrapper => _contentWrapper;

	public override object TypeId => this;

	public ContentWrapperAttribute(Type contentWrapper)
	{
		_contentWrapper = contentWrapper;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is ContentWrapperAttribute contentWrapperAttribute))
		{
			return false;
		}
		return _contentWrapper == contentWrapperAttribute._contentWrapper;
	}

	public override int GetHashCode()
	{
		return _contentWrapper.GetHashCode();
	}
}
