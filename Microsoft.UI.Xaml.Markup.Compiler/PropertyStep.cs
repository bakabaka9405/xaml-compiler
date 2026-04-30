using System.Reflection;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class PropertyStep : BindPathStep
{
	public string PropertyName { get; }

	public override string UniqueName => PropertyName;

	public override bool IsValueRequired
	{
		get
		{
			if (base.Parent.ImplementsINDEI)
			{
				PropertyInfo property = base.Parent.ValueType.UnderlyingType.GetProperty(PropertyName);
				if (property != null)
				{
					return Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ReflectionHelper.FindAttributeByShortTypeName(property, "DataAnnotations.RequiredAttribute") != null;
				}
			}
			return false;
		}
	}

	public PropertyStep(string name, XamlType valueType, BindPathStep parent, ApiInformation apiInformation)
		: base(valueType, parent, apiInformation)
	{
		PropertyName = name;
	}
}
