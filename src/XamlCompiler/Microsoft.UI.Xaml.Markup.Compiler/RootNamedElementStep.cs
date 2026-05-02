using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class RootNamedElementStep : BindPathStep
{
	public string FieldName { get; private set; }

	public string UpdateCallParamOverride { get; private set; }

	public override string UniqueName => FieldName;

	public RootNamedElementStep(string fieldName, XamlType fieldType, BindPathStep parent, ApiInformation apiInformation, string updateParamOverride)
		: this(fieldName, fieldType, parent, apiInformation)
	{
		UpdateCallParamOverride = updateParamOverride;
	}

	public RootNamedElementStep(string fieldName, XamlType fieldType, BindPathStep parent, ApiInformation apiInformation)
		: base(fieldType, parent, apiInformation)
	{
		FieldName = fieldName;
	}
}
