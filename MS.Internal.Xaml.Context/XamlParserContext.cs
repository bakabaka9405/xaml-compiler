using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xaml;
using MS.Internal.Xaml.Parser;

namespace MS.Internal.Xaml.Context;

internal class XamlParserContext : XamlContext
{
	private XamlContextStack<XamlParserFrame> _stack;

	private Dictionary<string, string> _prescopeNamespaces;

	public bool AllowProtectedMembersOnRoot { get; set; }

	public Func<string, string> XmlNamespaceResolver { get; set; }

	public XamlType CurrentType
	{
		get
		{
			return _stack.CurrentFrame.XamlType;
		}
		set
		{
			_stack.CurrentFrame.XamlType = value;
		}
	}

	internal BracketModeParseParameters CurrentBracketModeParseParameters
	{
		get
		{
			return _stack.CurrentFrame.BracketModeParseParameters;
		}
		set
		{
			_stack.CurrentFrame.BracketModeParseParameters = value;
		}
	}

	internal ParameterInfo[] CurrentLongestConstructorOfMarkupExtension
	{
		get
		{
			return _stack.CurrentFrame.LongestConstructorOfCurrentMarkupExtensionType;
		}
		set
		{
			_stack.CurrentFrame.LongestConstructorOfCurrentMarkupExtensionType = value;
		}
	}

	internal Dictionary<string, SpecialBracketCharacters> CurrentEscapeCharacterMapForMarkupExtension
	{
		get
		{
			return _stack.CurrentFrame.EscapeCharacterMapForMarkupExtension;
		}
		set
		{
			_stack.CurrentFrame.EscapeCharacterMapForMarkupExtension = value;
		}
	}

	public string CurrentTypeNamespace
	{
		get
		{
			return _stack.CurrentFrame.TypeNamespace;
		}
		set
		{
			_stack.CurrentFrame.TypeNamespace = value;
		}
	}

	public bool CurrentInContainerDirective
	{
		get
		{
			return _stack.CurrentFrame.InContainerDirective;
		}
		set
		{
			_stack.CurrentFrame.InContainerDirective = value;
		}
	}

	public XamlMember CurrentMember
	{
		get
		{
			return _stack.CurrentFrame.Member;
		}
		set
		{
			_stack.CurrentFrame.Member = value;
		}
	}

	public int CurrentArgCount
	{
		get
		{
			return _stack.CurrentFrame.CtorArgCount;
		}
		set
		{
			_stack.CurrentFrame.CtorArgCount = value;
		}
	}

	public bool CurrentForcedToUseConstructor
	{
		get
		{
			return _stack.CurrentFrame.ForcedToUseConstructor;
		}
		set
		{
			_stack.CurrentFrame.ForcedToUseConstructor = value;
		}
	}

	public bool CurrentInItemsProperty => _stack.CurrentFrame.Member == XamlLanguage.Items;

	public bool CurrentInInitProperty => _stack.CurrentFrame.Member == XamlLanguage.Initialization;

	public bool CurrentInUnknownContent => _stack.CurrentFrame.Member == XamlLanguage.UnknownContent;

	public bool CurrentInImplicitArray
	{
		get
		{
			return _stack.CurrentFrame.InImplicitArray;
		}
		set
		{
			_stack.CurrentFrame.InImplicitArray = value;
		}
	}

	public bool CurrentInCollectionFromMember
	{
		get
		{
			return _stack.CurrentFrame.InCollectionFromMember;
		}
		set
		{
			_stack.CurrentFrame.InCollectionFromMember = value;
		}
	}

	public XamlType CurrentPreviousChildType
	{
		get
		{
			return _stack.CurrentFrame.PreviousChildType;
		}
		set
		{
			_stack.CurrentFrame.PreviousChildType = value;
		}
	}

	public bool CurrentTypeIsRoot
	{
		get
		{
			if (_stack.CurrentFrame.XamlType != null)
			{
				return _stack.Depth == 1;
			}
			return false;
		}
	}

	public XamlParserContext(XamlSchemaContext schemaContext, Assembly localAssembly)
		: base(schemaContext)
	{
		_stack = new XamlContextStack<XamlParserFrame>(() => new XamlParserFrame());
		_prescopeNamespaces = new Dictionary<string, string>();
		_localAssembly = localAssembly;
	}

	public override void AddNamespacePrefix(string prefix, string xamlNS)
	{
		_prescopeNamespaces.Add(prefix, xamlNS);
	}

	public string FindNamespaceByPrefixInParseStack(string prefix)
	{
		if (_prescopeNamespaces != null && _prescopeNamespaces.TryGetValue(prefix, out var value))
		{
			return value;
		}
		XamlParserFrame xamlParserFrame = _stack.CurrentFrame;
		while (xamlParserFrame.Depth > 0)
		{
			if (xamlParserFrame.TryGetNamespaceByPrefix(prefix, out value))
			{
				return value;
			}
			xamlParserFrame = (XamlParserFrame)xamlParserFrame.Previous;
		}
		return null;
	}

	public override string FindNamespaceByPrefix(string prefix)
	{
		if (XmlNamespaceResolver != null)
		{
			return XmlNamespaceResolver(prefix);
		}
		return FindNamespaceByPrefixInParseStack(prefix);
	}

	public override IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
	{
		XamlParserFrame frame = _stack.CurrentFrame;
		Dictionary<string, string> keys = new Dictionary<string, string>();
		while (frame.Depth > 0)
		{
			if (frame._namespaces != null)
			{
				foreach (NamespaceDeclaration namespacePrefix in frame.GetNamespacePrefixes())
				{
					if (!keys.ContainsKey(namespacePrefix.Prefix))
					{
						keys.Add(namespacePrefix.Prefix, null);
						yield return namespacePrefix;
					}
				}
			}
			frame = (XamlParserFrame)frame.Previous;
		}
		if (_prescopeNamespaces == null)
		{
			yield break;
		}
		foreach (KeyValuePair<string, string> prescopeNamespace in _prescopeNamespaces)
		{
			if (!keys.ContainsKey(prescopeNamespace.Key))
			{
				keys.Add(prescopeNamespace.Key, null);
				yield return new NamespaceDeclaration(prescopeNamespace.Value, prescopeNamespace.Key);
			}
		}
	}

	internal override bool IsVisible(XamlMember member, XamlType rootObjectType)
	{
		if (member == null)
		{
			return false;
		}
		Type accessingType = null;
		if (AllowProtectedMembersOnRoot && rootObjectType != null)
		{
			accessingType = rootObjectType.UnderlyingType;
		}
		if (member.IsWriteVisibleTo(LocalAssembly, accessingType))
		{
			return true;
		}
		if (member.IsReadOnly || (member.Type != null && member.Type.IsUsableAsReadOnly))
		{
			return member.IsReadVisibleTo(LocalAssembly, accessingType);
		}
		return false;
	}

	public void PushScope()
	{
		_stack.PushScope();
		if (_prescopeNamespaces.Count > 0)
		{
			_stack.CurrentFrame.SetNamespaces(_prescopeNamespaces);
			_prescopeNamespaces = new Dictionary<string, string>();
		}
	}

	public void PopScope()
	{
		_stack.PopScope();
	}

	internal void InitBracketCharacterCacheForType(XamlType extensionType)
	{
		CurrentEscapeCharacterMapForMarkupExtension = base.SchemaContext.InitBracketCharacterCacheForType(extensionType);
	}

	internal void InitLongestConstructor(XamlType xamlType)
	{
		IEnumerable<ConstructorInfo> constructors = xamlType.GetConstructors();
		ParameterInfo[] currentLongestConstructorOfMarkupExtension = null;
		int num = 0;
		foreach (ConstructorInfo item in constructors)
		{
			ParameterInfo[] parameters = item.GetParameters();
			if (parameters.Length >= num)
			{
				num = parameters.Length;
				currentLongestConstructorOfMarkupExtension = parameters;
			}
		}
		CurrentLongestConstructorOfMarkupExtension = currentLongestConstructorOfMarkupExtension;
	}

	public bool CurrentMemberIsWriteVisible()
	{
		Type accessingType = null;
		if (AllowProtectedMembersOnRoot && _stack.Depth == 1)
		{
			accessingType = CurrentType.UnderlyingType;
		}
		return CurrentMember.IsWriteVisibleTo(LocalAssembly, accessingType);
	}
}
