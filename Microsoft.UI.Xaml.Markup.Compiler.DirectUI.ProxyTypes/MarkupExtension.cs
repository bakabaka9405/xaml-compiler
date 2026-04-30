using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ProxyTypes;

public abstract class MarkupExtension
{
	public abstract object ProvideValue(IServiceProvider serviceProvider);
}
