using System;
using System.Collections.Generic;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

public class XamlDomReader : XamlReader, IXamlLineInfo
{
	private class XamlNode
	{
		private static XamlNode _xamlNode = new XamlNode();

		public XamlType Type;

		public XamlMember Member;

		public NamespaceDeclaration Namespace;

		public XamlNodeType NodeType;

		public object Value;

		public int LineNumber;

		public int LinePosition;

		public void Clear()
		{
			Type = null;
			Member = null;
			Namespace = null;
			NodeType = XamlNodeType.None;
			Value = null;
			LineNumber = 0;
			LinePosition = 0;
		}

		public static XamlNode GetNamespaceDeclaration(XamlDomNamespace nsNode)
		{
			_xamlNode.Clear();
			_xamlNode.Namespace = nsNode.NamespaceDeclaration;
			_xamlNode.NodeType = XamlNodeType.NamespaceDeclaration;
			_xamlNode.LineNumber = nsNode.StartLineNumber;
			_xamlNode.LinePosition = nsNode.StartLinePosition;
			return _xamlNode;
		}

		public static XamlNode GetStartObject(XamlDomObject objectNode)
		{
			_xamlNode.Clear();
			if (objectNode.IsGetObject)
			{
				_xamlNode.NodeType = XamlNodeType.GetObject;
			}
			else
			{
				_xamlNode.NodeType = XamlNodeType.StartObject;
				_xamlNode.Type = objectNode.Type;
			}
			_xamlNode.LineNumber = objectNode.StartLineNumber;
			_xamlNode.LinePosition = objectNode.StartLinePosition;
			return _xamlNode;
		}

		internal static XamlNode GetEndObject(XamlDomObject objectNode)
		{
			_xamlNode.Clear();
			_xamlNode.NodeType = XamlNodeType.EndObject;
			_xamlNode.LineNumber = objectNode.EndLineNumber;
			_xamlNode.LinePosition = objectNode.EndLinePosition;
			return _xamlNode;
		}

		internal static XamlNode GetStartMember(XamlDomMember memberNode)
		{
			_xamlNode.Clear();
			_xamlNode.NodeType = XamlNodeType.StartMember;
			_xamlNode.Member = memberNode.Member;
			_xamlNode.LineNumber = memberNode.StartLineNumber;
			_xamlNode.LinePosition = memberNode.StartLinePosition;
			return _xamlNode;
		}

		internal static XamlNode GetEndMember(XamlDomMember memberNode)
		{
			_xamlNode.Clear();
			_xamlNode.NodeType = XamlNodeType.EndMember;
			_xamlNode.LineNumber = memberNode.EndLineNumber;
			_xamlNode.LinePosition = memberNode.EndLinePosition;
			return _xamlNode;
		}

		internal static XamlNode GetValue(XamlDomValue XamlDomValue)
		{
			_xamlNode.Clear();
			_xamlNode.NodeType = XamlNodeType.Value;
			_xamlNode.Value = XamlDomValue.Value;
			_xamlNode.LineNumber = XamlDomValue.StartLineNumber;
			_xamlNode.LinePosition = XamlDomValue.StartLinePosition;
			return _xamlNode;
		}
	}

	private IEnumerator<XamlNode> nodes;

	private XamlSchemaContext schemaContext;

	private bool doNotReorder;

	public override bool IsEof => NodeType != XamlNodeType.None;

	public override XamlMember Member
	{
		get
		{
			if (NodeType != XamlNodeType.StartMember)
			{
				return null;
			}
			return nodes.Current.Member;
		}
	}

	public override NamespaceDeclaration Namespace => nodes.Current.Namespace;

	public override XamlNodeType NodeType => nodes.Current.NodeType;

	public override XamlSchemaContext SchemaContext => schemaContext;

	public override XamlType Type
	{
		get
		{
			if (NodeType != XamlNodeType.StartObject)
			{
				return null;
			}
			return nodes.Current.Type;
		}
	}

	public override object Value
	{
		get
		{
			if (NodeType != XamlNodeType.Value)
			{
				return null;
			}
			return nodes.Current.Value;
		}
	}

	bool IXamlLineInfo.HasLineInfo => true;

	int IXamlLineInfo.LineNumber => nodes.Current.LineNumber;

	int IXamlLineInfo.LinePosition => nodes.Current.LinePosition;

	public XamlDomReader(IXamlDomNode domNode, XamlSchemaContext schemaContext)
		: this(domNode, schemaContext, null)
	{
	}

	public XamlDomReader(IXamlDomNode domNode, XamlSchemaContext schemaContext, XamlDomReaderSettings settings)
	{
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		if (domNode == null)
		{
			throw new ArgumentNullException("domNode");
		}
		this.schemaContext = schemaContext;
		if (settings != null)
		{
			doNotReorder = settings.DoNotReorderMembers;
		}
		nodes = WalkDom(domNode).GetEnumerator();
	}

	public override bool Read()
	{
		return nodes.MoveNext();
	}

	private IEnumerable<XamlNode> WalkDom(IXamlDomNode domNode)
	{
		if (domNode is XamlDomObject objectNode)
		{
			foreach (XamlNode item in ReadObjectNode(objectNode))
			{
				yield return item;
			}
			yield break;
		}
		if (domNode is XamlDomMember memberNode)
		{
			foreach (XamlNode item2 in ReadMemberNode(memberNode))
			{
				yield return item2;
			}
			yield break;
		}
		foreach (XamlNode item3 in ReadValueNode(domNode as XamlDomValue))
		{
			yield return item3;
		}
	}

	private IEnumerable<XamlNode> ReadValueNode(XamlDomValue xamlDomValue)
	{
		yield return XamlNode.GetValue(xamlDomValue);
	}

	private IEnumerable<XamlNode> ReadMemberNode(XamlDomMember memberNode)
	{
		if (memberNode.Items == null || memberNode.Items.Count == 0)
		{
			yield break;
		}
		yield return XamlNode.GetStartMember(memberNode);
		foreach (XamlDomItem item in memberNode.Items)
		{
			IEnumerable<XamlNode> enumerable = ((item is XamlDomObject objectNode) ? ReadObjectNode(objectNode) : ReadValueNode(item as XamlDomValue));
			foreach (XamlNode item2 in enumerable)
			{
				yield return item2;
			}
		}
		yield return XamlNode.GetEndMember(memberNode);
	}

	private IEnumerable<XamlNode> ReadObjectNode(XamlDomObject objectNode)
	{
		foreach (XamlDomNamespace @namespace in objectNode.Namespaces)
		{
			yield return XamlNode.GetNamespaceDeclaration(@namespace);
		}
		yield return XamlNode.GetStartObject(objectNode);
		if (!doNotReorder)
		{
			foreach (XamlNode item in WritePossibleAttributes(objectNode))
			{
				yield return item;
			}
			foreach (XamlNode item2 in WriteElementMembers(objectNode))
			{
				yield return item2;
			}
		}
		else
		{
			foreach (XamlDomMember memberNode in objectNode.MemberNodes)
			{
				foreach (XamlNode item3 in ReadMemberNode(memberNode))
				{
					yield return item3;
				}
			}
		}
		yield return XamlNode.GetEndObject(objectNode);
	}

	private IEnumerable<XamlNode> WriteElementMembers(XamlDomObject objectNode)
	{
		foreach (XamlDomMember memberNode in objectNode.MemberNodes)
		{
			if (IsAttribute(memberNode))
			{
				continue;
			}
			foreach (XamlNode item in ReadMemberNode(memberNode))
			{
				yield return item;
			}
		}
	}

	private IEnumerable<XamlNode> WritePossibleAttributes(XamlDomObject objectNode)
	{
		foreach (XamlDomMember memberNode in objectNode.MemberNodes)
		{
			if (!IsAttribute(memberNode))
			{
				continue;
			}
			foreach (XamlNode item in ReadMemberNode(memberNode))
			{
				yield return item;
			}
		}
	}

	private static bool IsAttribute(XamlDomMember memberNode)
	{
		if (memberNode.Items.Count == 1)
		{
			if (memberNode.Item is XamlDomValue)
			{
				return true;
			}
			XamlType type = ((XamlDomObject)memberNode.Item).Type;
			if (type != null && type.IsMarkupExtension)
			{
				return true;
			}
		}
		return false;
	}
}
