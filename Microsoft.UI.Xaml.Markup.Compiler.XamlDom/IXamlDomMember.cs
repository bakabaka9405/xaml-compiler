using System.Collections.Generic;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

internal interface IXamlDomMember
{
	XamlDomItem Item { get; set; }

	IList<XamlDomItem> Items { get; }

	XamlMember Member { get; set; }

	XamlDomObject Parent { get; set; }

	XamlSchemaContext SchemaContext { get; set; }

	void Seal();
}
