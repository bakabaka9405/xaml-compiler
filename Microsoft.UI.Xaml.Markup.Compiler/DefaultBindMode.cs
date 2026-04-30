using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

[Flags]
internal enum DefaultBindMode
{
	OneTime = 0,
	OneWay = 1,
	TwoWay = 2
}
