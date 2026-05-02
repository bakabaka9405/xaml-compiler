using System.ComponentModel;
using System.Diagnostics;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

[DebuggerDisplay("{Value}")]
public class XamlDomValue : XamlDomItem
{
	private object _value;

	[DefaultValue(null)]
	public virtual object Value
	{
		get
		{
			return _value;
		}
		set
		{
			CheckSealed();
			_value = value;
		}
	}

	public XamlDomValue(string sourceFilePath)
		: base(sourceFilePath)
	{
	}

	public XamlDomValue(object value, string sourceFilePath)
		: base(sourceFilePath)
	{
		_value = value;
	}
}
