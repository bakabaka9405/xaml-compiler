using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[TypeForwardedFrom("WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
public abstract class MarkupExtension
{
	public abstract object ProvideValue(IServiceProvider serviceProvider);
}
