using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class FieldStep : PropertyStep
{
	public string FieldName { get; }

	public override string UniqueName => FieldName;

	public FieldStep(string name, XamlType valueType, BindPathStep parent, ApiInformation apiInformation)
		: base(name, valueType, parent, apiInformation)
	{
		FieldName = name;
	}
}
