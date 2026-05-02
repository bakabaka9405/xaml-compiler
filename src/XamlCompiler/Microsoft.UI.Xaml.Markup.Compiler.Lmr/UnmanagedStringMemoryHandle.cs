using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
internal sealed class UnmanagedStringMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
{
	internal UnmanagedStringMemoryHandle()
		: base(ownsHandle: true)
	{
	}

	internal UnmanagedStringMemoryHandle(int countBytes)
		: base(ownsHandle: true)
	{
		if (countBytes != 0)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(countBytes);
			SetHandle(intPtr);
		}
	}

	protected override bool ReleaseHandle()
	{
		if (handle != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(handle);
			handle = IntPtr.Zero;
			return true;
		}
		return false;
	}

	public string GetAsString(int countCharsNoNull)
	{
		return Marshal.PtrToStringUni(handle, countCharsNoNull);
	}
}
