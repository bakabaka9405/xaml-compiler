namespace Microsoft.UI.Xaml.Markup.Compiler;

internal static class ClrNamespaceParser
{
	internal static bool TryParseUri(string uriInput, out string clrNs, out string assemblyName, out string error, bool returnErrors)
	{
		clrNs = null;
		assemblyName = null;
		error = null;
		int num = KS.IndexOf(uriInput, ":");
		if (-1 == num)
		{
			return false;
		}
		string a = uriInput.Substring(0, num);
		if (!KS.Eq(a, "clr-namespace"))
		{
			return false;
		}
		int num2 = num + 1;
		int num3 = KS.IndexOf(uriInput, ";");
		if (-1 == num3)
		{
			clrNs = uriInput.Substring(num2);
			assemblyName = null;
			return true;
		}
		int length = num3 - num2;
		clrNs = uriInput.Substring(num2, length);
		int num4 = num3 + 1;
		int num5 = KS.IndexOf(uriInput, "=");
		if (-1 == num5)
		{
			return false;
		}
		a = uriInput.Substring(num4, num5 - num4);
		if (!KS.Eq(a, "assembly"))
		{
			return false;
		}
		assemblyName = uriInput.Substring(num5 + 1);
		return true;
	}
}
