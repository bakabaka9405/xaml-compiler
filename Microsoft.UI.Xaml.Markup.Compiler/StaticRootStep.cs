using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class StaticRootStep : BindPathStep
{
	public override bool IsIncludedInUpdate => false;

	public override string UniqueName => ValueType.UnderlyingType.FullName.GetMemberFriendlyName();

	public override bool NeedsCheckForNull => false;

	public StaticRootStep(XamlType staticType, ApiInformation apiInformation)
		: base(staticType, null, apiInformation)
	{
	}
}
