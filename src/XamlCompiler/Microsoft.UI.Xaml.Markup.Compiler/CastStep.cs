using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class CastStep : BindPathStep
{
	public override string UniqueName
	{
		get
		{
			string arg = ((base.Parent is RootStep) ? "Root" : base.Parent.UniqueName);
			return $"Cast_{arg}_To_{ValueType.Name}";
		}
	}

	public CastStep(XamlType valueType, BindPathStep parent, ApiInformation apiInformation)
		: base(valueType, parent, apiInformation)
	{
	}
}
