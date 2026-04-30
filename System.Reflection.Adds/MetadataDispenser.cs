using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Reflection.Adds;

[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
internal class MetadataDispenser
{
	private class MetadataFileOnByteArray : MetadataFile
	{
		private GCHandle m_handle;

		public MetadataFileOnByteArray(ref GCHandle h, IntPtr pUnk)
			: base(pUnk)
		{
			m_handle = h;
			h = default(GCHandle);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			m_handle.Free();
		}
	}

	internal enum CorFileMapping : uint
	{
		Flat,
		ExecutableImage
	}

	[Guid("7DAC8207-D3AE-4c75-9B67-92801A497D44")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IMetadataImportDummy
	{
	}

	[ComImport]
	[Guid("31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IMetaDataDispenserEx
	{
		int DefineScope(ref Guid rclsid, uint dwCreateFlags, ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppIUnk);

		[PreserveSig]
		int OpenScope([MarshalAs(UnmanagedType.LPWStr)] string szScope, CorOpenFlags dwOpenFlags, ref Guid riid, out IntPtr ppIUnk);

		[PreserveSig]
		int OpenScopeOnMemory(IntPtr pData, uint cbData, CorOpenFlags dwOpenFlags, ref Guid riid, out IntPtr ppIUnk);

		int SetOption(ref Guid optionid, [MarshalAs(UnmanagedType.Struct)] object value);

		[PreserveSig]
		int GetOption(ref Guid optionid, [MarshalAs(UnmanagedType.Struct)] out object pvalue);

		int _OpenScopeOnITypeInfo();

		int GetCORSystemDirectory([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] szBuffer, uint cchBuffer, out uint pchBuffer);

		int FindAssembly([MarshalAs(UnmanagedType.LPWStr)] string szAppBase, [MarshalAs(UnmanagedType.LPWStr)] string szPrivateBin, [MarshalAs(UnmanagedType.LPWStr)] string szGlobalBin, [MarshalAs(UnmanagedType.LPWStr)] string szAssemblyName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)] char[] szName, uint cchName, out uint pcName);

		int FindAssemblyModule([MarshalAs(UnmanagedType.LPWStr)] string szAppBase, [MarshalAs(UnmanagedType.LPWStr)] string szPrivateBin, [MarshalAs(UnmanagedType.LPWStr)] string szGlobalBin, [MarshalAs(UnmanagedType.LPWStr)] string szAssemblyName, [MarshalAs(UnmanagedType.LPWStr)] string szModuleName, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 5)] char[] szName, uint cchName, out uint pcName);
	}

	[ComImport]
	[Guid("E5CB7A31-7512-11D2-89CE-0080C792E5D8")]
	private class CorMetaDataDispenserExClass
	{
	}

	[ComImport]
	[Guid("31BCFCE2-DAFB-11D2-9F81-00C04F79A0A3")]
	[CoClass(typeof(CorMetaDataDispenserExClass))]
	private interface MetaDataDispenserEx : IMetaDataDispenserEx
	{
	}

	[ComImport]
	[Guid("7998EA64-7F95-48B8-86FC-17CAF48BF5CB")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IMetaDataInfo
	{
		[PreserveSig]
		int GetFileMapping(out IntPtr ppvData, out long pcbData, out CorFileMapping pdwMappingType);
	}

	private CorOpenFlags m_openFlags = CorOpenFlags.ReadOnly | CorOpenFlags.NoTransform;

	public CorOpenFlags OpenFlags
	{
		get
		{
			return m_openFlags;
		}
		set
		{
			m_openFlags = value;
		}
	}

	private static IMetaDataDispenserEx GetDispenserShim()
	{
		return (IMetaDataDispenserEx)RuntimeEnvironment.GetRuntimeInterfaceAsObject(typeof(CorMetaDataDispenserExClass).GUID, typeof(IMetaDataDispenserEx).GUID);
	}

	public MetadataFile OpenFromByteArray(byte[] data)
	{
		data = (byte[])data.Clone();
		IMetaDataDispenserEx dispenserShim = GetDispenserShim();
		Guid riid = typeof(IMetadataImportDummy).GUID;
		IntPtr ppIUnk = IntPtr.Zero;
		GCHandle h = default(GCHandle);
		try
		{
			h = GCHandle.Alloc(data, GCHandleType.Pinned);
			IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(data, 0);
			uint cbData = (uint)data.Length;
			int errorCode = dispenserShim.OpenScopeOnMemory(pData, cbData, m_openFlags, ref riid, out ppIUnk);
			Marshal.ThrowExceptionForHR(errorCode);
			GC.KeepAlive(dispenserShim);
			Marshal.FinalReleaseComObject(dispenserShim);
			dispenserShim = null;
			return new MetadataFileOnByteArray(ref h, ppIUnk);
		}
		finally
		{
			if (h.IsAllocated)
			{
				h.Free();
			}
			if (ppIUnk != IntPtr.Zero)
			{
				Marshal.Release(ppIUnk);
			}
		}
	}

	public MetadataFile OpenFileAsFileMapping(string fileName)
	{
		FileMapping fileMapping = new FileMapping(fileName);
		IMetaDataDispenserEx dispenserShim = GetDispenserShim();
		Guid riid = typeof(IMetadataImportDummy).GUID;
		IntPtr ppIUnk = IntPtr.Zero;
		try
		{
			IntPtr baseAddress = fileMapping.BaseAddress;
			uint cbData = (uint)fileMapping.Length;
			int errorCode = dispenserShim.OpenScopeOnMemory(baseAddress, cbData, m_openFlags, ref riid, out ppIUnk);
			Marshal.ThrowExceptionForHR(errorCode);
			GC.KeepAlive(dispenserShim);
			Marshal.FinalReleaseComObject(dispenserShim);
			dispenserShim = null;
			return new MetadataFileAndRvaResolver(ppIUnk, fileMapping, (m_openFlags & CorOpenFlags.NoTransform) == 0);
		}
		finally
		{
			if (ppIUnk != IntPtr.Zero)
			{
				Marshal.Release(ppIUnk);
			}
		}
	}

	public MetadataFile OpenFileAsFileMapping(object importer, string fileName)
	{
		if (importer == null)
		{
			throw new ArgumentNullException("importer");
		}
		FileMapping fileMapping = null;
		if (importer is IMetaDataInfo metaDataInfo && metaDataInfo.GetFileMapping(out var ppvData, out var pcbData, out var pdwMappingType) == 0 && pdwMappingType == CorFileMapping.Flat)
		{
			fileMapping = new FileMapping(ppvData, pcbData, fileName);
			return new MetadataFileAndRvaResolver(importer, fileMapping);
		}
		return OpenFileAsFileMapping(fileName);
	}

	public MetadataFile OpenFile(string fileName)
	{
		IMetaDataDispenserEx dispenserShim = GetDispenserShim();
		Guid riid = typeof(IMetadataImportDummy).GUID;
		IntPtr ppIUnk = IntPtr.Zero;
		try
		{
			int errorCode = dispenserShim.OpenScope(fileName, m_openFlags, ref riid, out ppIUnk);
			Marshal.ThrowExceptionForHR(errorCode);
			GC.KeepAlive(dispenserShim);
			Marshal.FinalReleaseComObject(dispenserShim);
			dispenserShim = null;
			return new MetadataFile(ppIUnk);
		}
		finally
		{
			if (ppIUnk != IntPtr.Zero)
			{
				Marshal.Release(ppIUnk);
			}
		}
	}
}
