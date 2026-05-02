using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[Flags]
internal enum CorFileFlags
{
	ContainsMetaData = 0,
	ContainsNoMetaData = 1
}
