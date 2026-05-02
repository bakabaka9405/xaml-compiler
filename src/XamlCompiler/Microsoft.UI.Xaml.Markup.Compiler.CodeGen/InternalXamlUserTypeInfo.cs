using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Markup;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

[DebuggerDisplay("{StandardName}")]
[ContentProperty("Members")]
internal class InternalXamlUserTypeInfo
{
	private List<InternalXamlUserMemberInfo> _members;

	private List<string> _enumValues;

	private XamlSchemaCodeInfo _schemaInfo;

	private InternalTypeEntry _simpleTypeEntry;

	public InternalTypeEntry BaseType { get; set; }

	public InternalTypeEntry BoxedType { get; set; }

	public InternalXamlUserMemberInfo ContentProperty { get; set; }

	public InternalTypeEntry TypeEntry => _simpleTypeEntry;

	public string Name => _simpleTypeEntry.Name;

	public int TypeIndex => _simpleTypeEntry.TypeIndex;

	public bool IsReturnTypeStub { get; set; }

	public string SystemName => _simpleTypeEntry.SystemName;

	public string StandardName => _simpleTypeEntry.StandardName;

	public LanguageSpecificString FullName => _simpleTypeEntry.FullName;

	public string MemberFriendlyName => _simpleTypeEntry.MemberFriendlyName;

	public CreateFromStringMethod CreateFromStringMethod { get; set; }

	public bool HasCreateFromStringMethod { get; set; }

	public bool IsArray { get; set; }

	public bool IsCollection { get; set; }

	public bool IsConstructible { get; set; }

	public bool IsDictionary { get; set; }

	public bool IsMarkupExtension { get; set; }

	public bool IsBindable { get; set; }

	public bool IsLocalType { get; set; }

	public bool IsDeprecated { get; set; }

	public InternalTypeEntry ItemType { get; set; }

	public InternalTypeEntry KeyType { get; set; }

	public string AddMethodName { get; set; }

	public List<InternalXamlUserMemberInfo> Members => _members;

	public bool HasMembers => _members.Count > 0;

	public List<string> EnumValues
	{
		get
		{
			if (_enumValues == null)
			{
				_enumValues = new List<string>();
			}
			return _enumValues;
		}
	}

	public bool HasEnumValues
	{
		get
		{
			if (_enumValues != null)
			{
				return _enumValues.Count > 0;
			}
			return false;
		}
	}

	public InternalXamlUserTypeInfo(InternalTypeEntry entry, XamlSchemaCodeInfo schemaInfo)
	{
		_simpleTypeEntry = entry;
		_members = new List<InternalXamlUserMemberInfo>();
		_schemaInfo = schemaInfo;
	}

	public void Init(XamlType xamlType)
	{
		IsArray = xamlType.IsArray;
		IsCollection = xamlType.IsCollection;
		IsDictionary = xamlType.IsDictionary;
		IsConstructible = xamlType.IsConstructible;
		IsMarkupExtension = xamlType.IsMarkupExtension;
		if (xamlType.BaseType != null)
		{
			BaseType = _schemaInfo.AddType(xamlType.BaseType);
		}
		if (xamlType.ContentProperty != null)
		{
			ContentProperty = _schemaInfo.AddMember(_simpleTypeEntry, xamlType.ContentProperty, declaringTypeAsStub: true);
		}
		if (xamlType.ItemType != null)
		{
			ItemType = _schemaInfo.AddTypeAndProperties(xamlType.ItemType);
		}
		if (xamlType.KeyType != null)
		{
			KeyType = _schemaInfo.AddTypeAndProperties(xamlType.KeyType);
		}
		if (xamlType.IsCollection || xamlType.IsDictionary)
		{
			AddMethodName = "Add";
			DirectUIXamlType directUIXamlType = xamlType as DirectUIXamlType;
			if (directUIXamlType != null && !string.IsNullOrEmpty(directUIXamlType.AddMethodName))
			{
				AddMethodName = directUIXamlType.AddMethodName;
			}
		}
		DirectUIXamlType directUIXamlType2 = xamlType as DirectUIXamlType;
		IsDeprecated = !(directUIXamlType2 == null) && directUIXamlType2.IsDeprecated;
		if (directUIXamlType2 != null && directUIXamlType2.IsNullableGeneric(out var innerType))
		{
			BoxedType = _schemaInfo.AddType(directUIXamlType2.SchemaContext.GetXamlType(innerType));
		}
		if (xamlType.UnderlyingType.IsEnum)
		{
			Type underlyingType = xamlType.UnderlyingType;
			string[] names = Enum.GetNames(underlyingType);
			foreach (string item in names)
			{
				EnumValues.Add(item);
			}
		}
		IsLocalType = DomHelper.IsLocalType(xamlType);
		CreateFromStringMethod = xamlType.GetCreateFromStringMethod();
		HasCreateFromStringMethod = xamlType.HasCreateFromStringMethod();
	}

	public void AddMember(InternalXamlUserMemberInfo userMember, InternalTypeEntry declaringType, XamlSchemaCodeInfo schemaInfo)
	{
		if (!TryFindMember(userMember.Name, declaringType, out var _))
		{
			_members.Add(userMember);
		}
	}

	private bool TryFindIndexOfMember(string name, InternalTypeEntry declaringType, out int idx)
	{
		for (int i = 0; i < _members.Count; i++)
		{
			InternalXamlUserMemberInfo internalXamlUserMemberInfo = _members[i];
			if (internalXamlUserMemberInfo.Name == name && internalXamlUserMemberInfo.DeclaringType == declaringType)
			{
				idx = i;
				return true;
			}
		}
		idx = -1;
		return false;
	}

	private bool TryFindMember(string name, InternalTypeEntry declaringType, out InternalXamlUserMemberInfo member)
	{
		if (TryFindIndexOfMember(name, declaringType, out var idx))
		{
			member = _members[idx];
			return true;
		}
		member = null;
		return false;
	}
}
