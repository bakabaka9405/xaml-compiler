using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler.Utilities;

internal class NamedElementsStore
{
	private Stack<HashSet<string>> _scopeStack = new Stack<HashSet<string>>();

	internal NamedElementsStore()
	{
		_scopeStack.Push(new HashSet<string>());
	}

	internal void EnterNewScope(XamlDomObject member)
	{
		_scopeStack.Push(new HashSet<string>());
	}

	internal void ExitCurrentScope()
	{
		_scopeStack.Pop();
	}

	internal void AddNamedElement(string name)
	{
		_scopeStack.Peek().Add(name);
	}

	internal bool IsNameAlreadyUsed(string name)
	{
		return _scopeStack.Peek().Contains(name);
	}
}
