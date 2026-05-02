using System;
using System.Runtime.InteropServices;

namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

[StructLayout(LayoutKind.Explicit, Size = 8)]
public class TargetOSVersion
{
	[FieldOffset(0)]
	public ushort major;

	[FieldOffset(2)]
	public ushort minor;

	[FieldOffset(4)]
	public ushort build;

	[FieldOffset(6)]
	public ushort revision;
}
