using System.Xaml;

namespace MS.Internal.Xaml.Runtime;

internal interface IAddLineInfo
{
	XamlException WithLineInfo(XamlException ex);
}
