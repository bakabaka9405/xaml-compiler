using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public class xProperty
{
	public string Name;

	public bool IsReadOnly;

	public XamlType PropertyType;

	public XamlDomObject OriginalXProperty;

	private string _changedHandler;

	public string FullTypeName;

	public string DefaultValueMarkup;

	public string DefaultValueString;

	public string CodegenComment;

	public string ChangedHandler
	{
		get
		{
			if (_changedHandler == null)
			{
				return Name + "Changed";
			}
			return _changedHandler;
		}
		set
		{
			_changedHandler = value;
		}
	}
}
