using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.UI.Xaml.Markup.Compiler.XBF;

namespace Microsoft.UI.Xaml.Markup.Compiler.Utilities;

internal static class NativeMethods
{
	public delegate int Write(IStream[] xamlStreams, int numFiles, string[] pbChecksum, int checksumSize, IXbfMetadataProvider provider, TargetOSVersion targetVersion, uint xbfGenerationFlags, IStream[] xbfStreams, out int errorCode, out int errorFileIndex, out int errorLine, out int errorColumn);

	public static int LOAD_WITH_ALTERED_SEARCH_PATH = 8;

	[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern IntPtr LoadLibraryEx(string libFilename, IntPtr reserved, int flags);

	[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
	public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

	[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
	public static extern int FreeLibrary(IntPtr hModule);

	[DllImport("vcmeta.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
	public static extern int HashForWinMD(string lpFileName, out Guid hash);
}
