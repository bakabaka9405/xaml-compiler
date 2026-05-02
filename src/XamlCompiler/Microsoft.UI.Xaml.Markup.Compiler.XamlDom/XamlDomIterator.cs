using System.Collections.Generic;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

public class XamlDomIterator
{
	private Stack<XamlDomMember> _members = new Stack<XamlDomMember>();

	private Stack<int> _scopes = new Stack<int>();

	private XamlDomObject _owner;

	internal event XamlDomIteratorEnterNewScopeEvent EnterNewScopeCallback;

	internal event XamlDomIteratorExitNewScopeEvent ExitScopeCallback;

	internal XamlDomIterator(XamlDomObject owner)
	{
		_owner = owner;
	}

	internal virtual IEnumerable<XamlDomObject> DescendantsAndSelf()
	{
		return DescendantsAndSelf(null);
	}

	internal virtual IEnumerable<XamlDomObject> DescendantsAndSelf(XamlType type)
	{
		for (int num = _owner.MemberNodes.Count - 1; num >= 0; num--)
		{
			_members.Push(_owner.MemberNodes[num]);
		}
		if (IsObjectNodeAssignable(type, _owner))
		{
			yield return _owner;
		}
		while (_members.Count > 0)
		{
			ShouldNotifyNamingScopeExit();
			XamlDomMember xamlDomMember = _members.Pop();
			ShouldNotifyNamingScopeEnter(xamlDomMember);
			foreach (XamlDomItem item in xamlDomMember.Items)
			{
				if (item is XamlDomObject xamlDomObject)
				{
					for (int num2 = xamlDomObject.MemberNodes.Count - 1; num2 >= 0; num2--)
					{
						_members.Push(xamlDomObject.MemberNodes[num2]);
					}
					if (IsObjectNodeAssignable(type, xamlDomObject))
					{
						yield return xamlDomObject;
					}
				}
			}
		}
		ShouldNotifyNamingScopeExit();
	}

	private void ShouldNotifyNamingScopeEnter(XamlDomMember member)
	{
		if (this.EnterNewScopeCallback != null)
		{
			DirectUIXamlMember directUIXamlMember = member.Member as DirectUIXamlMember;
			if (directUIXamlMember != null && directUIXamlMember.IsTemplate)
			{
				_scopes.Push(_members.Count);
				this.EnterNewScopeCallback(member.Parent);
			}
		}
	}

	private static bool IsObjectNodeAssignable(XamlType type, XamlDomObject objectNode)
	{
		if (type == null)
		{
			return true;
		}
		if (!objectNode.IsGetObject)
		{
			if (objectNode.Type.CanAssignTo(type))
			{
				return true;
			}
		}
		else if (objectNode.Parent != null && objectNode.Parent.Member.Type.CanAssignTo(type))
		{
			return true;
		}
		return false;
	}

	private void ShouldNotifyNamingScopeExit()
	{
		if (this.ExitScopeCallback != null)
		{
			while (_scopes.Count > 0 && _scopes.Peek() == _members.Count)
			{
				_scopes.Pop();
				this.ExitScopeCallback();
			}
		}
	}
}
