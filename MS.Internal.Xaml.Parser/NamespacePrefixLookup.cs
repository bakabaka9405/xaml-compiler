using System;
using System.Collections.Generic;
using System.Xaml;

namespace MS.Internal.Xaml.Parser;

internal class NamespacePrefixLookup : INamespacePrefixLookup
{
	private readonly List<NamespaceDeclaration> _newNamespaces;

	private readonly Func<string, string> _nsResolver;

	private int n;

	public NamespacePrefixLookup(out IEnumerable<NamespaceDeclaration> newNamespaces, Func<string, string> nsResolver)
	{
		newNamespaces = (_newNamespaces = new List<NamespaceDeclaration>());
		_nsResolver = nsResolver;
	}

	public string LookupPrefix(string ns)
	{
		string text;
		do
		{
			text = "prefix" + n++;
		}
		while (_nsResolver(text) != null);
		_newNamespaces.Add(new NamespaceDeclaration(ns, text));
		return text;
	}
}
