using System.Collections.Generic;

namespace System.Xaml;

internal class XamlPropertySet
{
	private Dictionary<XamlMember, bool> dictionary = new Dictionary<XamlMember, bool>();

	public bool Contains(XamlMember member)
	{
		return dictionary.ContainsKey(member);
	}

	public void Add(XamlMember member)
	{
		dictionary.Add(member, value: true);
	}
}
