using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

internal class XbfFileNameInfo : IXbfFileNameInfo
{
	public string GivenXamlName { get; set; }

	public string InputXamlName { get; set; }

	public string OutputXbfName { get; set; }

	public string XamlFileChecksum { get; set; }

	public XbfFileNameInfo(string sourceXamlFullName, string givenXaml, string inputXaml, string outputXbf, string checksum = null)
	{
		GivenXamlName = givenXaml;
		InputXamlName = inputXaml;
		OutputXbfName = outputXbf;
		XamlFileChecksum = checksum ?? ChecksumHelper.Instance.ComputeCheckSumForXamlFile(sourceXamlFullName);
	}
}
