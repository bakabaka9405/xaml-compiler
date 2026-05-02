using System;

namespace Microsoft.UI.Xaml.Markup.Compiler.Tracing;

internal static class PerformanceUtility
{
	private static ILog logger;

	internal static void Initialize(ILog logger)
	{
		PerformanceUtility.logger = logger;
	}

	internal static void Shutdown()
	{
		logger = null;
	}

	internal static void FireCodeMarker(CodeMarkerEvent marker, string additionalInformation = null)
	{
		DateTime now = DateTime.Now;
		string text = string.Format("Xaml Compiler Marker: {0}:{1,4} {2}", now.ToString("T"), now.Millisecond, marker.ToString());
		if (!string.IsNullOrEmpty(additionalInformation))
		{
			text = text + ", " + additionalInformation;
		}
		logger.LogDiagnosticMessage(text);
	}
}
