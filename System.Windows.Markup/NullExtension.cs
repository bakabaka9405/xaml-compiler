using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[MarkupExtensionReturnType(typeof(object))]
public class NullExtension : MarkupExtension
{
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return null;
	}
}
