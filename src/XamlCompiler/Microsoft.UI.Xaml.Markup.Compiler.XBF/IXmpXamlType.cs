using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler.XBF;

internal interface IXmpXamlType
{
	IXbfType GetXmpXamlType(DirectUIXamlType xamlType);
}
