using System.Collections.Generic;
using System.Xaml.Schema;

namespace MS.Internal.Xaml.Parser;

internal class TypeNameFrame
{
	private List<XamlTypeName> _typeArgs;

	public string Namespace { get; set; }

	public string Name { get; set; }

	public List<XamlTypeName> TypeArgs => _typeArgs;

	public void AllocateTypeArgs()
	{
		_typeArgs = new List<XamlTypeName>();
	}
}
