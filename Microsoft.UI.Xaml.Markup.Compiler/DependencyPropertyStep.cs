using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class DependencyPropertyStep : PropertyStep
{
	public XamlType OwnerType { get; }

	public override string UniqueName => base.PropertyName;

	public DependencyPropertyStep(string name, XamlType valueType, BindPathStep parent, ApiInformation apiInformation)
		: base(name, valueType, parent, apiInformation)
	{
		XamlMember member = parent.ValueType.GetMember(name);
		if (member?.DeclaringType != null)
		{
			OwnerType = member.DeclaringType;
		}
		else
		{
			OwnerType = parent.ValueType;
		}
	}

	public DependencyPropertyStep(string name, XamlType valueType, XamlType ownerType, BindPathStep parent, ApiInformation apiInformation)
		: base(name, valueType, parent, apiInformation)
	{
		OwnerType = ownerType;
	}
}
