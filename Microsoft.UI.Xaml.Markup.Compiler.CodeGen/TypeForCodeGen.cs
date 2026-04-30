using System;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class TypeForCodeGen
{
	private DirectUIXamlType _xamlType;

	private string _standardName;

	private string _systemName;

	private string _memberFriendlyName;

	private LanguageSpecificString _languageSpecificString;

	public Type UnderlyingType => _xamlType.UnderlyingType;

	public string StandardName
	{
		get
		{
			if (_standardName == null)
			{
				_standardName = XamlSchemaCodeInfo.GetFullGenericNestedName(_xamlType.UnderlyingType, "WinRT", globalized: false);
			}
			return _standardName;
		}
	}

	public string SystemName
	{
		get
		{
			if (_systemName == null)
			{
				_systemName = _xamlType.UnderlyingType.FullName;
			}
			return _systemName;
		}
	}

	public LanguageSpecificString FullName
	{
		get
		{
			if (_languageSpecificString == null)
			{
				_languageSpecificString = new LanguageSpecificString(() => XamlSchemaCodeInfo.GetFullGenericNestedName(_xamlType.UnderlyingType, "C++", globalized: true), () => XamlSchemaCodeInfo.GetFullGenericNestedName(_xamlType.UnderlyingType, "CppWinRT", globalized: true), () => XamlSchemaCodeInfo.GetFullGenericNestedName(_xamlType.UnderlyingType, "C#", globalized: true), () => XamlSchemaCodeInfo.GetFullGenericNestedName(_xamlType.UnderlyingType, "VB", globalized: true));
			}
			return _languageSpecificString;
		}
	}

	public string MemberFriendlyName
	{
		get
		{
			if (_memberFriendlyName == null)
			{
				_memberFriendlyName = StandardName.GetMemberFriendlyName();
			}
			return _memberFriendlyName;
		}
	}

	public TypeForCodeGen(XamlType xamlType)
	{
		_xamlType = (DirectUIXamlType)xamlType;
	}
}
