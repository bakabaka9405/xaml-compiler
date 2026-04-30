using System.Xaml;

namespace MS.Internal.Xaml.Parser;

internal class XamlScannerFrame
{
	public XamlType XamlType { get; set; }

	public XamlMember XamlProperty { get; set; }

	public bool XmlSpacePreserve { get; set; }

	public bool InContent { get; set; }

	public string TypeNamespace { get; set; }

	public XamlScannerFrame(XamlType xamlType, string ns)
	{
		XamlType = xamlType;
		TypeNamespace = ns;
	}
}
