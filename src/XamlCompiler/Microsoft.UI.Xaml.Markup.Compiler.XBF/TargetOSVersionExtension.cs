using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

internal static class TargetOSVersionExtension
{
	public static TargetOSVersion ToTargetOSVersion(this Version version)
	{
		return new TargetOSVersion
		{
			major = (ushort)version.Major,
			minor = (ushort)version.Minor,
			build = (ushort)version.Build,
			revision = (ushort)version.Revision
		};
	}
}
