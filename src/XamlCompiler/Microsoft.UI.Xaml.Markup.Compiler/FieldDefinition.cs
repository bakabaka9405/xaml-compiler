using System;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class FieldDefinition
{
	public LanguageSpecificString _fieldModifier;

	private XamlType _xamlType;

	public string FieldName { get; set; }

	public string FieldTypePath { get; private set; }

	public string FieldTypeShortName { get; private set; }

	public string FieldTypeName => FieldTypePath + "." + FieldTypeShortName;

	public XamlType FieldXamlType => _xamlType;

	public TypeForCodeGen FieldType { get; set; }

	public bool IsValueType { get; private set; }

	public bool IsDeprecated { get; private set; }

	public LanguageSpecificString FieldModifier
	{
		get
		{
			if (_fieldModifier == null)
			{
				_fieldModifier = new LanguageSpecificString(() => "private", () => "protected", () => "private", () => "private");
			}
			return _fieldModifier;
		}
		set
		{
			_fieldModifier = value;
		}
	}

	private IDirectUIXamlLanguage DirectUIXamlLanguage
	{
		get
		{
			if (_xamlType == null)
			{
				return null;
			}
			DirectUISchemaContext directUISchemaContext = (DirectUISchemaContext)_xamlType.SchemaContext;
			return directUISchemaContext.DirectUIXamlLanguage;
		}
	}

	private FieldDefinition()
	{
	}

	public FieldDefinition(XamlDomObject namedObject)
	{
		InitFromType((DirectUIXamlType)namedObject.Type);
		XamlDomMember aliasedMemberNode = DomHelper.GetAliasedMemberNode(namedObject, XamlLanguage.Name);
		FieldName = DomHelper.GetStringValueOfProperty(aliasedMemberNode);
		XamlDomMember memberNode = namedObject.GetMemberNode(XamlLanguage.FieldModifier);
		if (memberNode != null)
		{
			string modifier = DomHelper.GetStringValueOfProperty(memberNode);
			_fieldModifier = new LanguageSpecificString(() => modifier.ToLower(), () => modifier.ToLower(), () => modifier.ToLower(), () => modifier.ToTitleCase());
		}
	}

	private void InitFromType(DirectUIXamlType xamlType)
	{
		_xamlType = xamlType;
		FieldType = new TypeForCodeGen(xamlType);
		FieldTypePath = xamlType.UnderlyingType.Namespace;
		FieldTypeShortName = xamlType.UnderlyingType.Name;
		IsValueType = xamlType.IsValueType;
		IsDeprecated = xamlType.IsDeprecated;
	}

	public FieldDefinition(XamlDomObject namedObject, string clrPath)
	{
		_xamlType = namedObject.Type;
		FieldType = null;
		FieldTypePath = clrPath;
		FieldTypeShortName = namedObject.Type.Name;
		XamlDomMember aliasedMemberNode = DomHelper.GetAliasedMemberNode(namedObject, XamlLanguage.Name);
		FieldName = DomHelper.GetStringValueOfProperty(aliasedMemberNode);
		XamlDomMember memberNode = namedObject.GetMemberNode(XamlLanguage.FieldModifier);
		if (memberNode != null)
		{
			string modifier = DomHelper.GetStringValueOfProperty(memberNode);
			_fieldModifier = new LanguageSpecificString(() => modifier);
		}
	}

	public bool HasSameAttributes(FieldDefinition that)
	{
		if (!CanBeMerged(that))
		{
			return false;
		}
		if (FieldType != null)
		{
			if (FieldType != that.FieldType)
			{
				return false;
			}
		}
		else if (FieldTypeName != that.FieldTypeName)
		{
			return false;
		}
		return true;
	}

	public bool CanBeMerged(FieldDefinition that)
	{
		if (FieldName != that.FieldName)
		{
			return false;
		}
		if (FieldModifier != that.FieldModifier)
		{
			return false;
		}
		return true;
	}

	public static FieldDefinition CreateMerged(FieldDefinition a, FieldDefinition b)
	{
		FieldDefinition fieldDefinition = new FieldDefinition();
		if (!a.CanBeMerged(b))
		{
			throw new InvalidOperationException("Field definitions cannot be merged");
		}
		fieldDefinition.FieldName = a.FieldName;
		fieldDefinition.FieldModifier = a.FieldModifier;
		if (a.FieldType == null || b.FieldType == null)
		{
			fieldDefinition.FieldType = new TypeForCodeGen(a.DirectUIXamlLanguage.Object);
		}
		DirectUIXamlType xamlType = (DirectUIXamlType)FindCommonBaseClass(a._xamlType, b._xamlType);
		fieldDefinition.InitFromType(xamlType);
		return fieldDefinition;
	}

	private static XamlType FindCommonBaseClass(XamlType a, XamlType b)
	{
		if (a == b)
		{
			return a;
		}
		if (a.UnderlyingType.IsAssignableFrom(b.UnderlyingType))
		{
			return a;
		}
		if (b.UnderlyingType.IsAssignableFrom(a.UnderlyingType))
		{
			return b;
		}
		return FindCommonBaseClass(a.BaseType, b.BaseType);
	}
}
