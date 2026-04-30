using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class ArrayIndexStep : BindPathStep
{
	public int Index { get; }

	public override string UniqueName => $"I{Index}";

	public ArrayIndexStep(int index, XamlType valueType, BindPathStep parent, ApiInformation apiInformation)
		: base(valueType, parent, apiInformation)
	{
		Index = index;
	}
}
