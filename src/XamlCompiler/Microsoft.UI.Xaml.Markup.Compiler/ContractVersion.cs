using System;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class ContractVersion
{
	public static Version ToVersion(uint contractVersion)
	{
		uint major = (contractVersion >> 16) & 0xFFFF;
		uint minor = contractVersion & 0xFFFF;
		return new Version((int)major, (int)minor, 0, 0);
	}
}
