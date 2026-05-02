namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

public interface IXbfFileNameInfo
{
	string GivenXamlName { get; set; }

	string InputXamlName { get; set; }

	string OutputXbfName { get; set; }

	string XamlFileChecksum { get; set; }
}
