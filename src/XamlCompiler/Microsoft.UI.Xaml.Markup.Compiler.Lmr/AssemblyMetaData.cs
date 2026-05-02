using System;
using System.Globalization;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal struct AssemblyMetaData : IDisposable
{
	public ushort majorVersion;

	public ushort minorVersion;

	public ushort buildNumber;

	public ushort revisionNumber;

	public UnmanagedStringMemoryHandle szLocale;

	public uint cbLocale;

	public UnusedIntPtr rdwProcessor;

	public uint ulProcessor;

	public UnusedIntPtr rOS;

	public uint ulOS;

	public Version Version => new Version(majorVersion, minorVersion, buildNumber, revisionNumber);

	public string LocaleString
	{
		get
		{
			if (szLocale == null)
			{
				return null;
			}
			if (szLocale.IsInvalid)
			{
				return string.Empty;
			}
			if (cbLocale == 0)
			{
				return string.Empty;
			}
			int num = (int)cbLocale;
			int countCharsNoNull = num - 1;
			return szLocale.GetAsString(countCharsNoNull);
		}
	}

	public CultureInfo Locale
	{
		get
		{
			if (szLocale == null)
			{
				return CultureInfo.InvariantCulture;
			}
			return new CultureInfo(LocaleString);
		}
	}

	public void Init()
	{
		szLocale = new UnmanagedStringMemoryHandle();
		cbLocale = 0u;
		ulProcessor = 0u;
		ulOS = 0u;
	}

	public void Dispose()
	{
		if (szLocale != null)
		{
			szLocale.Dispose();
		}
	}
}
