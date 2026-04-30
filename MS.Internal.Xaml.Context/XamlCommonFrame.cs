using System.Collections.Generic;
using System.Xaml;

namespace MS.Internal.Xaml.Context;

internal abstract class XamlCommonFrame : XamlFrame
{
	internal Dictionary<string, string> _namespaces;

	public Dictionary<string, string> Namespaces
	{
		get
		{
			if (_namespaces == null)
			{
				_namespaces = new Dictionary<string, string>();
			}
			return _namespaces;
		}
	}

	public XamlType XamlType { get; set; }

	public XamlMember Member { get; set; }

	public XamlCommonFrame()
	{
	}

	public XamlCommonFrame(XamlCommonFrame source)
		: base(source)
	{
		XamlType = source.XamlType;
		Member = source.Member;
		if (source._namespaces != null)
		{
			SetNamespaces(source._namespaces);
		}
	}

	public override void Reset()
	{
		XamlType = null;
		Member = null;
		if (_namespaces != null)
		{
			_namespaces.Clear();
		}
	}

	public void AddNamespace(string prefix, string xamlNs)
	{
		Namespaces.Add(prefix, xamlNs);
	}

	public void SetNamespaces(Dictionary<string, string> namespaces)
	{
		if (_namespaces != null)
		{
			_namespaces.Clear();
		}
		if (namespaces == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> @namespace in namespaces)
		{
			Namespaces.Add(@namespace.Key, @namespace.Value);
		}
	}

	public bool TryGetNamespaceByPrefix(string prefix, out string xamlNs)
	{
		if (_namespaces != null && _namespaces.TryGetValue(prefix, out xamlNs))
		{
			return true;
		}
		xamlNs = null;
		return false;
	}

	public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
	{
		List<NamespaceDeclaration> list = new List<NamespaceDeclaration>();
		foreach (KeyValuePair<string, string> @namespace in _namespaces)
		{
			list.Add(new NamespaceDeclaration(@namespace.Value, @namespace.Key));
		}
		return list;
	}
}
