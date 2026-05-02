using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ProxyTypes;

public class NullExtension : MarkupExtension
{
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return null;
	}
}
