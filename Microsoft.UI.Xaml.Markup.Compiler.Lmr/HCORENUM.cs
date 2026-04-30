using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal struct HCORENUM
{
	private IntPtr hEnum;

	public void Close(IMetadataImport import)
	{
		if (hEnum != IntPtr.Zero)
		{
			import.CloseEnum(hEnum);
			hEnum = IntPtr.Zero;
		}
	}

	public void Close(IMetadataImport2 import)
	{
		if (hEnum != IntPtr.Zero)
		{
			import.CloseEnum(hEnum);
			hEnum = IntPtr.Zero;
		}
	}

	public void Close(IMetadataAssemblyImport import)
	{
		if (hEnum != IntPtr.Zero)
		{
			import.CloseEnum(hEnum);
			hEnum = IntPtr.Zero;
		}
	}
}
