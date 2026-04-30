using System.Xaml.MS.Impl;

namespace System.Xaml.Schema;

internal static class ClrNamespaceUriParser
{
	public static string GetUri(string clrNs, string assemblyName)
	{
		return string.Format(TypeConverterHelper.InvariantEnglishUS, "clr-namespace:{0};assembly={1}", clrNs, assemblyName);
	}

	public static bool TryParseUri(string uriInput, out string clrNs, out string assemblyName)
	{
		string error;
		return TryParseUri(uriInput, out clrNs, out assemblyName, out error, returnErrors: false);
	}

	private static bool TryParseUri(string uriInput, out string clrNs, out string assemblyName, out string error, bool returnErrors)
	{
		clrNs = null;
		assemblyName = null;
		error = null;
		int num = KS.IndexOf(uriInput, ":");
		if (-1 == num)
		{
			if (returnErrors)
			{
				error = SR.Get("MissingTagInNamespace", ":", uriInput);
			}
			return false;
		}
		string a = uriInput.Substring(0, num);
		if (!KS.Eq(a, "clr-namespace"))
		{
			if (returnErrors)
			{
				error = SR.Get("MissingTagInNamespace", "clr-namespace", uriInput);
			}
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
			if (returnErrors)
			{
				error = SR.Get("MissingTagInNamespace", "=", uriInput);
			}
			return false;
		}
		a = uriInput.Substring(num4, num5 - num4);
		if (!KS.Eq(a, "assembly"))
		{
			if (returnErrors)
			{
				error = SR.Get("AssemblyTagMissing", "assembly", uriInput);
			}
			return false;
		}
		assemblyName = uriInput.Substring(num5 + 1);
		return true;
	}
}
