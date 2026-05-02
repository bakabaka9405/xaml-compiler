using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

internal class XamlDomDFSValidator
{
	private XamlDomObject _owner;

	private DirectUISchemaContext _schema;

	internal XamlDomDFSValidator(XamlDomObject owner, DirectUISchemaContext schema)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		_owner = owner;
		_schema = schema;
	}

	internal virtual List<XamlCompileError> EnsureNoXBindIsUsedInsideAStyle()
	{
		Stack<XamlDomNode> stack = new Stack<XamlDomNode>();
		Stack<int> stack2 = new Stack<int>();
		stack.Push(_owner);
		List<XamlCompileError> list = new List<XamlCompileError>();
		while (stack.Count > 0)
		{
			XamlDomNode xamlDomNode = stack.Pop();
			XamlDomObject xamlDomObject = xamlDomNode as XamlDomObject;
			XamlDomMember xamlDomMember = xamlDomNode as XamlDomMember;
			if (xamlDomObject != null)
			{
				if (_schema.DirectUISystem.Style.IsAssignableFrom(xamlDomObject.Type.UnderlyingType))
				{
					stack2.Push(stack.Count);
				}
				else if (_schema.DirectUIXamlLanguage.BindExtension == xamlDomObject.Type && stack2.Count > 0)
				{
					list.Add(new XamlXBindUsedInStyleError(xamlDomObject));
				}
				foreach (XamlDomMember memberNode in xamlDomObject.MemberNodes)
				{
					stack.Push(memberNode);
				}
			}
			else if (xamlDomMember != null)
			{
				foreach (XamlDomItem item in xamlDomMember.Items)
				{
					stack.Push(item);
				}
			}
			if (stack2.Count > 0 && stack2.Peek() == stack.Count)
			{
				stack2.Pop();
			}
		}
		return list;
	}
}
