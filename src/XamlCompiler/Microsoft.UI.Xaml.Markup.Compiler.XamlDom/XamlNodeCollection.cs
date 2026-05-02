using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

public class XamlNodeCollection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable where T : XamlDomNode
{
	private XamlDomNode _parentNode;

	private List<T> _nodes;

	private bool _isSealed;

	public int Count => Nodes.Count;

	public bool IsSealed => _isSealed;

	public T this[int index]
	{
		get
		{
			return Nodes[index];
		}
		set
		{
			CheckSealed();
			Nodes[index] = value;
			SetParent(value);
		}
	}

	public bool IsReadOnly => _isSealed;

	private List<T> Nodes
	{
		get
		{
			if (_nodes == null)
			{
				_nodes = new List<T>();
			}
			return _nodes;
		}
	}

	public XamlNodeCollection(XamlDomNode parent)
	{
		_parentNode = parent;
	}

	public void Seal()
	{
		_isSealed = true;
		foreach (T node in _nodes)
		{
			node.Seal();
		}
	}

	public void Add(T node)
	{
		CheckSealed();
		Nodes.Add(node);
		SetParent(node);
	}

	public int IndexOf(T item)
	{
		return Nodes.IndexOf(item);
	}

	public void Insert(int index, T item)
	{
		CheckSealed();
		Nodes.Insert(index, item);
		SetParent(item);
	}

	public void RemoveAt(int index)
	{
		CheckSealed();
		SetParentToNull(Nodes[index]);
		Nodes.RemoveAt(index);
	}

	public void Clear()
	{
		CheckSealed();
		foreach (T node in Nodes)
		{
			SetParentToNull(node);
		}
		Nodes.Clear();
	}

	public bool Contains(T item)
	{
		return Nodes.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		CheckSealed();
		Nodes.CopyTo(array, arrayIndex);
	}

	public bool Remove(T item)
	{
		CheckSealed();
		SetParentToNull(item);
		return Nodes.Remove(item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return Nodes.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Nodes.GetEnumerator();
	}

	private void SetParent(T node)
	{
		XamlDomItem xamlDomItem = node as XamlDomItem;
		XamlDomMember xamlDomMember = node as XamlDomMember;
		if (xamlDomItem != null)
		{
			xamlDomItem.Parent = (XamlDomMember)_parentNode;
		}
		if (xamlDomMember != null)
		{
			xamlDomMember.Parent = (XamlDomObject)_parentNode;
		}
	}

	private static void SetParentToNull(T node)
	{
		XamlDomObject xamlDomObject = node as XamlDomObject;
		XamlDomMember xamlDomMember = node as XamlDomMember;
		if (xamlDomObject != null)
		{
			xamlDomObject.Parent = null;
		}
		if (xamlDomMember != null)
		{
			xamlDomMember.Parent = null;
		}
	}

	private void CheckSealed()
	{
		if (IsSealed)
		{
			throw new NotSupportedException("The MemberNode is read-only");
		}
	}
}
