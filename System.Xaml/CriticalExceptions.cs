using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;

namespace System.Xaml;

internal static class CriticalExceptions
{
	internal static bool IsCriticalException(Exception ex)
	{
		ex = Unwrap(ex);
		if (!(ex is NullReferenceException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException) && !(ex is SEHException))
		{
			return ex is SecurityException;
		}
		return true;
	}

	internal static Exception Unwrap(Exception ex)
	{
		while (ex.InnerException != null && ex is TargetInvocationException)
		{
			ex = ex.InnerException;
		}
		return ex;
	}
}
