using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class TypeSignatureDescriptor
{
	public Type Type { get; set; }

	public CustomModifiers CustomModifiers { get; set; }

	public bool IsPinned { get; set; }
}
