using System.Collections.Generic;
using System.Windows.Markup;

namespace Microsoft.UI.Xaml.Markup.Compiler.RootLog;

[ContentProperty("Members")]
internal class RootType
{
	public string FullName { get; set; }

	public List<RootMember> Members { get; private set; }

	public RootType()
	{
		Members = new List<RootMember>();
	}
}
