using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.Reflection.Adds;

internal static class NativeMethods
{
	public sealed class SafeWin32Handle : SafeHandleZeroOrMinusOneIsInvalid
	{
		private SafeWin32Handle()
			: base(ownsHandle: true)
		{
		}

		protected override bool ReleaseHandle()
		{
			return CloseHandle(handle);
		}
	}

	[Flags]
	public enum PageProtection : uint
	{
		NoAccess = 1u,
		Readonly = 2u,
		ReadWrite = 4u,
		WriteCopy = 8u,
		Execute = 0x10u,
		ExecuteRead = 0x20u,
		ExecuteReadWrite = 0x40u,
		ExecuteWriteCopy = 0x80u,
		Guard = 0x100u,
		NoCache = 0x200u,
		WriteCombine = 0x400u
	}

	public sealed class SafeMapViewHandle : SafeHandleZeroOrMinusOneIsInvalid
	{
		public IntPtr BaseAddress => handle;

		private SafeMapViewHandle()
			: base(ownsHandle: true)
		{
		}

		protected override bool ReleaseHandle()
		{
			return UnmapViewOfFile(handle);
		}
	}

	private const string Kernel32LibraryName = "kernel32.dll";

	private const int FILE_TYPE_DISK = 1;

	private const int GENERIC_READ = int.MinValue;

	[DllImport("kernel32.dll", BestFitMapping = false, CharSet = CharSet.Auto, SetLastError = true)]
	private static extern SafeFileHandle CreateFile(string lpFileName, int dwDesiredAccess, FileShare dwShareMode, IntPtr securityAttrs, FileMode dwCreationDisposition, int dwFlagsAndAttributes, IntPtr hTemplateFile);

	[DllImport("kernel32.dll")]
	private static extern int GetFileType(SafeFileHandle handle);

	internal static SafeFileHandle SafeOpenFile(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		if (fileName.Length == 0 || fileName.StartsWith("\\\\.\\", StringComparison.Ordinal))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidFileName, fileName));
		}
		SafeFileHandle safeFileHandle = CreateFile(fileName, int.MinValue, FileShare.Read, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
		if (safeFileHandle.IsInvalid)
		{
			int hRForLastWin32Error = Marshal.GetHRForLastWin32Error();
			Marshal.ThrowExceptionForHR(hRForLastWin32Error, new IntPtr(-1));
		}
		else
		{
			int fileType = GetFileType(safeFileHandle);
			if (fileType != 1)
			{
				safeFileHandle.Dispose();
				throw new ArgumentException(Resources.UnsupportedImageType);
			}
		}
		return safeFileHandle;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern int GetFileSize(SafeFileHandle hFile, out int highSize);

	internal static long FileSize(SafeFileHandle handle)
	{
		int highSize = 0;
		int num = 0;
		num = GetFileSize(handle, out highSize);
		if (num == -1)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			Marshal.ThrowExceptionForHR(lastWin32Error);
		}
		return ((long)highSize << 32) | (uint)num;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool CloseHandle(IntPtr handle);

	[DllImport("kernel32.dll", BestFitMapping = false, SetLastError = true)]
	public static extern SafeWin32Handle CreateFileMapping(SafeFileHandle hFile, IntPtr lpFileMappingAttributes, PageProtection flProtect, uint dwMaximumSizeHigh, uint dwMaximumSizeLow, string lpName);

	[DllImport("kernel32.dll", SetLastError = true)]
	public static extern SafeMapViewHandle MapViewOfFile(SafeWin32Handle hFileMappingObject, uint dwDesiredAccess, uint dwFileOffsetHigh, uint dwFileOffsetLow, IntPtr dwNumberOfBytesToMap);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool UnmapViewOfFile(IntPtr baseAddress);
}
