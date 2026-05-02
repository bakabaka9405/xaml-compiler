using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

public class SealableNamespaceCollection : KeyedCollection<string, XamlDomNamespace>
{
	private bool _isSealed;

	public bool IsSealed => _isSealed;

	public void Seal()
	{
		_isSealed = true;
		using IEnumerator<XamlDomNamespace> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			XamlDomNamespace current = enumerator.Current;
			current.Seal();
		}
	}

	protected override string GetKeyForItem(XamlDomNamespace item)
	{
		return item.NamespaceDeclaration.Prefix;
	}

	protected override void InsertItem(int index, XamlDomNamespace item)
	{
		CheckSealed();
		base.InsertItem(index, item);
	}

	protected override void RemoveItem(int index)
	{
		CheckSealed();
		base.RemoveItem(index);
	}

	protected override void SetItem(int index, XamlDomNamespace item)
	{
		CheckSealed();
		base.SetItem(index, item);
	}

	protected override void ClearItems()
	{
		CheckSealed();
		base.ClearItems();
	}

	private void CheckSealed()
	{
		if (IsSealed)
		{
			throw new NotSupportedException(ResourceUtilities.FormatString(XamlCompilerResources.XamlDom_SealedNamespaceCollection));
		}
	}
}
