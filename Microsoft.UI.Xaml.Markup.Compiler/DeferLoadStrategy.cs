using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

[Flags]
public enum DeferLoadStrategy
{
	None = 0,
	Lazy = 1
}
