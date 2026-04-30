using System.Reflection;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class AttachedPropertyStep : DependencyPropertyStep
{
	public override string UniqueName => $"A_{base.OwnerType.UnderlyingType.FullName.GetMemberFriendlyName()}_{base.PropertyName}";

	public override bool IsValueRequired
	{
		get
		{
			if (base.Parent.ImplementsINDEI)
			{
				MemberInfo[] member = base.OwnerType.UnderlyingType.GetMember(base.PropertyName + "Property");
				if (member != null && member.Length != 0)
				{
					return Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ReflectionHelper.FindAttributeByShortTypeName(member[0], "DataAnnotations.RequiredAttribute") != null;
				}
			}
			return false;
		}
	}

	public AttachedPropertyStep(string propertyName, XamlType valueType, XamlType ownerType, BindPathStep parent, ApiInformation apiInformation)
		: base(propertyName, valueType, ownerType, parent, apiInformation)
	{
	}
}
