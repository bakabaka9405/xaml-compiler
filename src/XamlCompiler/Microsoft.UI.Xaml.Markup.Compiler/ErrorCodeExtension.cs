namespace Microsoft.UI.Xaml.Markup.Compiler;

public static class ErrorCodeExtension
{
	public static string AsErrorCode(this ErrorCode code)
	{
		return ((int)code).AsErrorCode();
	}

	public static string AsErrorCode(this int code)
	{
		return "WMC" + code.ToString("D4");
	}
}
