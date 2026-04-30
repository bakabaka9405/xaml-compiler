using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class RootStep : BindPathStep
{
	public bool IsElementRoot;

	public override bool IsIncludedInUpdate => true;

	public override string UniqueName => "";

	public override bool NeedsCheckForNull
	{
		get
		{
			if (!IsElementRoot)
			{
				return base.NeedsCheckForNull;
			}
			return false;
		}
	}

	public RootStep(XamlType valueType, bool isElementRoot = false)
		: base(valueType, null, null)
	{
		IsElementRoot = isElementRoot;
	}
}
