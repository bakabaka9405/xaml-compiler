using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.UI.Xaml.Markup.Compiler.XBF;

namespace Microsoft.UI.Xaml.Markup.Compiler.Utilities;

internal static class NativeMethodsHelper
{
	private static bool s_VcMetaIsLoaded;

	public static bool EnsureVcMetaIsLoaded(string vsInstallDir, string vcInstallPath32, string vcInstallPath64)
	{
		if (!s_VcMetaIsLoaded)
		{
			string path32bit = vcInstallPath32;
			string path64bit = vcInstallPath64;
			if (LoadLibrary32_64(path32bit, path64bit) != IntPtr.Zero)
			{
				s_VcMetaIsLoaded = true;
			}
			else if (!string.IsNullOrEmpty(vsInstallDir))
			{
				path32bit = Path.Combine(vsInstallDir, "bin\\VcMeta.dll");
				path64bit = Path.Combine(vsInstallDir, "bin\\amd64\\VcMeta.dll");
				if (LoadLibrary32_64(path32bit, path64bit) != IntPtr.Zero)
				{
					s_VcMetaIsLoaded = true;
				}
				else
				{
					path32bit = Path.Combine(vsInstallDir, "vcpackages\\vcmeta.dll");
					if (LoadLibrary32_64(path32bit, path64bit) != IntPtr.Zero)
					{
						s_VcMetaIsLoaded = true;
					}
				}
			}
		}
		return s_VcMetaIsLoaded;
	}

	private static IntPtr LoadLibrary32_64(string path32bit, string path64bit)
	{
		string libFilename = (Environment.Is64BitProcess ? path64bit : path32bit);
		return NativeMethods.LoadLibraryEx(libFilename, IntPtr.Zero, NativeMethods.LOAD_WITH_ALTERED_SEARCH_PATH);
	}

	public static int Write(IntPtr dllHandle, IStream[] xamlStreams, int numFiles, string[] pbChecksum, int checksumSize, IXbfMetadataProvider provider, TargetOSVersion targetVersion, uint xbfGenerationFlags, IStream[] xbfStreams, out int errorCode, out int errorFileIndex, out int errorLine, out int errorColumn)
	{
		if (dllHandle == IntPtr.Zero)
		{
			throw new ArgumentException("dllHandle");
		}
		IntPtr procAddress = NativeMethods.GetProcAddress(dllHandle, "Write");
		if (procAddress != IntPtr.Zero)
		{
			NativeMethods.Write write = (NativeMethods.Write)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.Write));
			return write(xamlStreams, numFiles, pbChecksum, checksumSize, provider, targetVersion, xbfGenerationFlags, xbfStreams, out errorCode, out errorFileIndex, out errorLine, out errorColumn);
		}
		errorCode = -1;
		errorFileIndex = -1;
		errorLine = -1;
		errorColumn = -1;
		return -1;
	}
}
