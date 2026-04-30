using System.Collections.Generic;
using System.ComponentModel;
using System.Xaml;
using System.Xaml.Schema;

namespace System.Windows.Markup;

public class PropertyDefinition : MemberDefinition
{
	private IList<Attribute> attributes;

	public override string Name { get; set; }

	[TypeConverter(typeof(XamlTypeTypeConverter))]
	public XamlType Type { get; set; }

	[DefaultValue(null)]
	public string Modifier { get; set; }

	public IList<Attribute> Attributes
	{
		get
		{
			if (attributes == null)
			{
				attributes = new List<Attribute>();
			}
			return attributes;
		}
	}
}
