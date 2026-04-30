using System;
using System.Diagnostics;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

[DebuggerDisplay("{StandardName} Sys={IsSystemType}")]
internal class InternalTypeEntry
{
	private string _name;

	private int _typeIndex;

	private TypeForCodeGen _type;

	public Type UnderlyingType => _type.UnderlyingType;

	public string SystemName => _type.SystemName;

	public string StandardName => _type.StandardName;

	public LanguageSpecificString FullName => _type.FullName;

	public string MemberFriendlyName => _type.MemberFriendlyName;

	public InternalXamlUserTypeInfo UserTypeInfo { get; set; }

	public bool IsSystemType => UserTypeInfo == null;

	public int TypeIndex
	{
		get
		{
			return _typeIndex;
		}
		set
		{
			_typeIndex = value;
		}
	}

	public bool IsValueType { get; set; }

	public string RefHat
	{
		get
		{
			if (!IsValueType)
			{
				return "^";
			}
			return string.Empty;
		}
	}

	public string Name
	{
		get
		{
			if (_name == null)
			{
				int num = StandardName.IndexOf('`');
				_name = ((num == -1) ? StandardName : StandardName.Substring(0, num));
				num = _name.LastIndexOf('.');
				if (num != -1)
				{
					_name = _name.Substring(num + 1);
				}
			}
			return _name;
		}
	}

	public bool IsDeprecated
	{
		get
		{
			if (UserTypeInfo != null)
			{
				return UserTypeInfo.IsDeprecated;
			}
			return false;
		}
	}

	public bool IsHandledByOtherProviders { get; set; }

	public InternalTypeEntry(XamlType xamlType)
	{
		UserTypeInfo = null;
		_typeIndex = -1;
		_type = new TypeForCodeGen(xamlType);
		DirectUIXamlType directUIXamlType = xamlType as DirectUIXamlType;
		if (directUIXamlType != null)
		{
			IsValueType = directUIXamlType.IsValueType;
		}
	}
}
