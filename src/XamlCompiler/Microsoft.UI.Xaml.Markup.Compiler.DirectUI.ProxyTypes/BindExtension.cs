using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ProxyTypes;

public class BindExtension : MarkupExtension
{
	public BindExtension()
	{
	}

	public BindExtension(string path)
	{
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		return null;
	}
}
