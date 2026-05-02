using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace System.Reflection.Adds;

internal class FileMapping : IDisposable
{
	private readonly string m_fileName;

	private readonly string m_filePath;

	private readonly long m_fileLength;

	private readonly IntPtr m_baseAddress;

	private readonly SafeFileHandle m_fileHandle;

	private readonly NativeMethods.SafeWin32Handle m_fileMapping;

	private readonly NativeMethods.SafeMapViewHandle m_View;

	public string Path => m_filePath;

	public long Length => m_fileLength;

	public IntPtr BaseAddress => m_baseAddress;

	public FileMapping(IntPtr baseAddress, long fileLength, string fileName)
	{
		m_fileName = fileName;
		m_filePath = System.IO.Path.GetFullPath(m_fileName);
		m_fileLength = fileLength;
		m_baseAddress = baseAddress;
	}

	public FileMapping(string fileName)
	{
		m_fileName = fileName;
		m_fileHandle = NativeMethods.SafeOpenFile(fileName);
		m_fileLength = NativeMethods.FileSize(m_fileHandle);
		m_filePath = System.IO.Path.GetFullPath(m_fileName);
		m_fileMapping = NativeMethods.CreateFileMapping(m_fileHandle, IntPtr.Zero, NativeMethods.PageProtection.Readonly, 0u, 0u, null);
		if (m_fileMapping.IsInvalid)
		{
			int hRForLastWin32Error = Marshal.GetHRForLastWin32Error();
			Marshal.ThrowExceptionForHR(hRForLastWin32Error);
		}
		m_View = NativeMethods.MapViewOfFile(m_fileMapping, 4u, 0u, 0u, IntPtr.Zero);
		if (m_View.IsInvalid)
		{
			int hRForLastWin32Error2 = Marshal.GetHRForLastWin32Error();
			Marshal.ThrowExceptionForHR(hRForLastWin32Error2);
		}
		m_baseAddress = m_View.BaseAddress;
	}

	public override string ToString()
	{
		if (m_baseAddress != IntPtr.Zero)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} Addr=0x{1}, Length=0x{2}", m_fileName, m_baseAddress.ToString("x"), m_fileLength.ToString("x", CultureInfo.InvariantCulture));
		}
		if (m_View != null && m_View.IsInvalid)
		{
			return m_fileName + " (closed)";
		}
		return m_fileName;
	}

	public void Dispose()
	{
		if (m_View != null)
		{
			m_View.Close();
		}
		if (m_fileMapping != null)
		{
			m_fileMapping.Close();
		}
		if (m_fileHandle != null)
		{
			m_fileHandle.Close();
		}
		GC.SuppressFinalize(this);
	}
}
