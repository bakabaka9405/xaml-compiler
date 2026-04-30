using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[Flags]
internal enum CorManifestResourceFlags
{
	mrVisibilityMask = 7,
	mrPublic = 1,
	mrPrivate = 2
}
