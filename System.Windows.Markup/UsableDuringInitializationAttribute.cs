namespace System.Windows.Markup;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class UsableDuringInitializationAttribute : Attribute
{
	private bool _usable;

	public bool Usable => _usable;

	public UsableDuringInitializationAttribute(bool usable)
	{
		_usable = usable;
	}
}
