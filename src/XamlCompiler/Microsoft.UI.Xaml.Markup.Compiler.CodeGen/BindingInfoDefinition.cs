using System.Collections.Generic;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class BindingInfoDefinition : Definition
{
	internal Dictionary<string, XamlType> ObservableVectorTypes { get; set; }

	internal Dictionary<string, XamlType> ObservableMapTypes { get; set; }

	internal Dictionary<string, XamlMember> BindingSetters { get; set; }

	internal bool EventBindingUsed { get; set; }

	public BindingInfoDefinition(XamlProjectInfo projectInfo, XamlSchemaCodeInfo schemaInfo)
		: base(projectInfo, schemaInfo)
	{
	}
}
