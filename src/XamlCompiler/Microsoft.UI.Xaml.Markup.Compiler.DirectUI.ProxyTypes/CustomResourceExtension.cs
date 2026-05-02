using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ProxyTypes;

public class CustomResourceExtension : MarkupExtension
{
	public object ResourceKey { get; set; }

	public CustomResourceExtension()
	{
	}

	public CustomResourceExtension(object resourceKey)
	{
		ResourceKey = resourceKey;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return null;
	}
}
