using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class MapIndexStep : BindPathStep
{
	private readonly string keyHashCode;

	public string Key { get; }

	public override string UniqueName => $"K{keyHashCode}";

	public MapIndexStep(string key, XamlType valueType, BindPathStep parent, ApiInformation apiInformation)
		: base(valueType, parent, apiInformation)
	{
		Key = key;
		keyHashCode = ((uint)key.GetHashCode()).ToString();
	}
}
