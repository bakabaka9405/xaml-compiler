using System;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public interface IXamlTypeResolver
{
	XamlType ResolveType(Type type);

	XamlType ResolveXmlName(string name);

	bool CanAssignDirectlyTo(XamlType source, XamlType destination);

	bool CanInlineConvert(XamlType source, XamlType destination);
}
