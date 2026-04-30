using System.ComponentModel;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

internal abstract class XamlDomItem : XamlDomNode
{
	private XamlDomMember _parent;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DefaultValue(null)]
	public XamlDomMember Parent
	{
		get
		{
			return _parent;
		}
		set
		{
			CheckSealed();
			_parent = value;
		}
	}

	public XamlDomItem(string sourceFilePath)
		: base(sourceFilePath)
	{
	}
}
