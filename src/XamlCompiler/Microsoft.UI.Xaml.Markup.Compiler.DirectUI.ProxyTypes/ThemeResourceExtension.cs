using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ProxyTypes;

public class ThemeResourceExtension : MarkupExtension
{
	public object ResourceKey { get; set; }

	public ThemeResourceExtension()
	{
	}

	public ThemeResourceExtension(object resourceKey)
	{
		ResourceKey = resourceKey;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return null;
	}
}
