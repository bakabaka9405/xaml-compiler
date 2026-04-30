using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler.RootLog;

internal class Roots
{
	public List<RootType> RootTypes { get; private set; }

	public List<RootPropertyPathName> PropertyPathNames { get; private set; }

	public List<RootInterface> Interfaces { get; private set; }

	public Roots()
	{
		RootTypes = new List<RootType>();
		PropertyPathNames = new List<RootPropertyPathName>();
		Interfaces = new List<RootInterface>();
	}
}
