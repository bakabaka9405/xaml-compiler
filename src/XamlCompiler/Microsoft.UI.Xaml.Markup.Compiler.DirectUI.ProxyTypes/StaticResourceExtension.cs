using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ProxyTypes;

public class StaticResourceExtension : MarkupExtension
{
	public object ResourceKey { get; set; }

	public StaticResourceExtension()
	{
	}

	public StaticResourceExtension(object resourceKey)
	{
		ResourceKey = resourceKey;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return null;
	}
}
