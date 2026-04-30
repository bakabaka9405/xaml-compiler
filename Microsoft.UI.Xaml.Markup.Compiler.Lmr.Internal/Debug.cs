using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr.Internal;

internal class Debug
{
	private enum MessageBoxResult
	{
		IDABORT = 3,
		IDRETRY,
		IDIGNORE
	}

	[DllImport("user32.dll", BestFitMapping = false)]
	private static extern int MessageBoxA(int h, string m, string c, int type);

	private static MessageBoxResult MessageBox(string message)
	{
		return (MessageBoxResult)MessageBoxA(0, message, "LMR Assert failed", 50);
	}

	[Conditional("DEBUG")]
	public static void Assert(bool f)
	{
	}

	[Conditional("DEBUG")]
	public static void Assert(bool f, string message)
	{
		if (!f)
		{
			Debugger.Log(0, "assert", message);
			string stackTrace = Environment.StackTrace;
			MessageBoxResult messageBoxResult = MessageBox(message + "\r\n" + stackTrace + "\r\nAbort - terminate the process\r\nRetry - break into the debugger\r\nIgnore - ignore the assert and continue running");
			if (messageBoxResult == MessageBoxResult.IDABORT)
			{
				Environment.Exit(1);
			}
			if (messageBoxResult == MessageBoxResult.IDRETRY)
			{
				Debugger.Break();
			}
		}
	}

	[Conditional("DEBUG")]
	public static void Fail(string message)
	{
	}
}
