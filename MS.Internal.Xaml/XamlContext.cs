using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xaml;
using System.Xaml.Schema;
using MS.Internal.Xaml.Parser;

namespace MS.Internal.Xaml;

internal abstract class XamlContext
{
	private XamlSchemaContext _schemaContext;

	private Func<string, string> _resolvePrefixCachedDelegate;

	protected Assembly _localAssembly;

	public XamlSchemaContext SchemaContext => _schemaContext;

	public virtual Assembly LocalAssembly
	{
		get
		{
			return _localAssembly;
		}
		protected set
		{
			_localAssembly = value;
		}
	}

	internal Func<string, string> ResolvePrefixCachedDelegate
	{
		get
		{
			if (_resolvePrefixCachedDelegate == null)
			{
				_resolvePrefixCachedDelegate = FindNamespaceByPrefix;
			}
			return _resolvePrefixCachedDelegate;
		}
	}

	protected XamlContext(XamlSchemaContext schemaContext)
	{
		_schemaContext = schemaContext;
	}

	public XamlMember GetXamlProperty(XamlType xamlType, string propertyName, XamlType rootObjectType)
	{
		if (xamlType.IsUnknown)
		{
			return null;
		}
		XamlMember member = xamlType.GetMember(propertyName);
		if (!IsVisible(member, rootObjectType))
		{
			return null;
		}
		return member;
	}

	public XamlMember GetXamlAttachableProperty(XamlType xamlType, string propertyName)
	{
		if (xamlType.IsUnknown)
		{
			return null;
		}
		XamlMember attachableMember = xamlType.GetAttachableMember(propertyName);
		if (!IsVisible(attachableMember, null))
		{
			return null;
		}
		return attachableMember;
	}

	public XamlMember GetDottedProperty(XamlType tagType, string tagNamespace, XamlPropertyName propName, bool tagIsRoot)
	{
		if (tagType == null)
		{
			throw new XamlInternalException(SR.Get("ParentlessPropertyElement", propName.ScopedName));
		}
		XamlMember xamlMember = null;
		XamlType xamlType = null;
		string text = ResolveXamlNameNS(propName);
		if (text == null)
		{
			throw new XamlParseException(SR.Get("PrefixNotFound", propName.Prefix));
		}
		XamlType rootTagType = (tagIsRoot ? tagType : null);
		bool flag = false;
		if (tagType.IsGeneric)
		{
			flag = PropertyTypeMatchesGenericTagType(tagType, tagNamespace, text, propName.OwnerName);
			if (flag)
			{
				xamlMember = GetInstanceOrAttachableProperty(tagType, propName.Name, rootTagType);
				if (xamlMember != null)
				{
					return xamlMember;
				}
			}
		}
		XamlTypeName typeName = new XamlTypeName(text, propName.Owner.Name);
		xamlType = GetXamlType(typeName, returnUnknownTypesOnFailure: true);
		bool flag2 = tagType.CanAssignTo(xamlType);
		xamlMember = ((!flag2) ? GetXamlAttachableProperty(xamlType, propName.Name) : GetInstanceOrAttachableProperty(xamlType, propName.Name, rootTagType));
		if (xamlMember == null)
		{
			XamlType declaringType = (flag ? tagType : xamlType);
			xamlMember = ((!(flag || flag2)) ? CreateUnknownAttachableMember(declaringType, propName.Name) : CreateUnknownMember(declaringType, propName.Name));
		}
		return xamlMember;
	}

	public string GetAttributeNamespace(XamlPropertyName propName, string tagNamespace)
	{
		if (string.IsNullOrEmpty(propName.Prefix) && !propName.IsDotted)
		{
			return tagNamespace;
		}
		return ResolveXamlNameNS(propName);
	}

	public string IgnoreAPIInformation(string namespaceName)
	{
		if (HasAPIInformation(namespaceName))
		{
			return namespaceName.Substring(0, namespaceName.IndexOf('?'));
		}
		return namespaceName;
	}

	public bool HasAPIInformation(string namespaceName)
	{
		return namespaceName?.Contains("?") ?? false;
	}

	public XamlMember GetNoDotAttributeProperty(XamlType tagType, XamlPropertyName propName, string tagNamespace, string propUsageNamespace, bool tagIsRoot)
	{
		XamlMember xamlMember = null;
		if (IgnoreAPIInformation(propUsageNamespace) == IgnoreAPIInformation(tagNamespace) || (tagNamespace == null && propUsageNamespace != null && tagType.GetXamlNamespaces().Contains(IgnoreAPIInformation(propUsageNamespace))))
		{
			XamlType rootObjectType = (tagIsRoot ? tagType : null);
			XamlType xamlType = ((tagNamespace != propUsageNamespace && HasAPIInformation(propUsageNamespace)) ? GetXamlType(new XamlTypeName(propUsageNamespace, tagType.Name), returnUnknownTypesOnFailure: true) : tagType);
			xamlMember = GetXamlProperty(xamlType, propName.Name, rootObjectType);
			if (xamlMember == null)
			{
				xamlMember = GetXamlAttachableProperty(xamlType, propName.Name);
			}
		}
		if (xamlMember == null && propUsageNamespace != null)
		{
			XamlDirective xamlDirective = SchemaContext.GetXamlDirective(propUsageNamespace, propName.Name);
			if (xamlDirective != null)
			{
				if ((xamlDirective.AllowedLocation & AllowedMemberLocations.Attribute) == 0)
				{
					xamlDirective = new XamlDirective(propUsageNamespace, propName.Name);
				}
				xamlMember = xamlDirective;
			}
		}
		if (xamlMember == null)
		{
			xamlMember = ((!(tagNamespace == propUsageNamespace)) ? new XamlDirective(propUsageNamespace, propName.Name) : new XamlMember(propName.Name, tagType, isAttachable: false));
		}
		return xamlMember;
	}

	public abstract void AddNamespacePrefix(string prefix, string xamlNamespace);

	public abstract string FindNamespaceByPrefix(string prefix);

	public abstract IEnumerable<NamespaceDeclaration> GetNamespacePrefixes();

	private XamlType GetXamlTypeOrUnknown(XamlTypeName typeName)
	{
		return GetXamlType(typeName, returnUnknownTypesOnFailure: true);
	}

	internal XamlType GetXamlType(XamlName typeName)
	{
		return GetXamlType(typeName, returnUnknownTypesOnFailure: false);
	}

	internal XamlType GetXamlType(XamlName typeName, bool returnUnknownTypesOnFailure)
	{
		XamlTypeName xamlTypeName = GetXamlTypeName(typeName);
		return GetXamlType(xamlTypeName, returnUnknownTypesOnFailure);
	}

	internal XamlTypeName GetXamlTypeName(XamlName typeName)
	{
		string text = ResolveXamlNameNS(typeName);
		if (text == null)
		{
			throw new XamlParseException(SR.Get("PrefixNotFound", typeName.Prefix));
		}
		return new XamlTypeName(text, typeName.Name);
	}

	internal XamlType GetXamlType(XamlTypeName typeName)
	{
		return GetXamlType(typeName, returnUnknownTypesOnFailure: false, skipVisibilityCheck: false);
	}

	internal XamlType GetXamlType(XamlTypeName typeName, bool returnUnknownTypesOnFailure)
	{
		return GetXamlType(typeName, returnUnknownTypesOnFailure, skipVisibilityCheck: false);
	}

	internal XamlType GetXamlType(XamlTypeName typeName, bool returnUnknownTypesOnFailure, bool skipVisibilityCheck)
	{
		XamlType xamlType = _schemaContext.GetXamlType(typeName);
		if (xamlType != null && !skipVisibilityCheck && !xamlType.IsVisibleTo(LocalAssembly))
		{
			xamlType = null;
		}
		if (xamlType == null && returnUnknownTypesOnFailure)
		{
			XamlType[] typeArguments = null;
			if (typeName.HasTypeArgs)
			{
				typeArguments = ArrayHelper.ConvertArrayType(typeName.TypeArguments, GetXamlTypeOrUnknown);
			}
			xamlType = new XamlType(typeName.Namespace, typeName.Name, typeArguments, SchemaContext);
		}
		return xamlType;
	}

	private string ResolveXamlNameNS(XamlName name)
	{
		return name.Namespace ?? FindNamespaceByPrefix(name.Prefix);
	}

	internal XamlType ResolveXamlType(string qName, bool skipVisibilityCheck)
	{
		string error;
		XamlTypeName xamlTypeName = XamlTypeName.ParseInternal(qName, ResolvePrefixCachedDelegate, out error);
		if (xamlTypeName == null)
		{
			throw new XamlParseException(error);
		}
		return GetXamlType(xamlTypeName, returnUnknownTypesOnFailure: false, skipVisibilityCheck);
	}

	internal XamlMember ResolveDirectiveProperty(string xamlNS, string name)
	{
		if (xamlNS != null)
		{
			return SchemaContext.GetXamlDirective(xamlNS, name);
		}
		return null;
	}

	internal virtual bool IsVisible(XamlMember member, XamlType rootObjectType)
	{
		return true;
	}

	private XamlMember CreateUnknownMember(XamlType declaringType, string name)
	{
		return new XamlMember(name, declaringType, isAttachable: false);
	}

	private XamlMember CreateUnknownAttachableMember(XamlType declaringType, string name)
	{
		return new XamlMember(name, declaringType, isAttachable: true);
	}

	private bool PropertyTypeMatchesGenericTagType(XamlType tagType, string tagNs, string propNs, string propTypeName)
	{
		if (tagNs != propNs && tagType.Name != propTypeName && !tagType.GetXamlNamespaces().Contains(propNs))
		{
			return false;
		}
		XamlType xamlType = GetXamlType(propNs, propTypeName, tagType.TypeArguments);
		return tagType == xamlType;
	}

	private XamlMember GetInstanceOrAttachableProperty(XamlType tagType, string propName, XamlType rootTagType)
	{
		XamlMember xamlMember = GetXamlProperty(tagType, propName, rootTagType);
		if (xamlMember == null)
		{
			xamlMember = GetXamlAttachableProperty(tagType, propName);
		}
		return xamlMember;
	}

	private XamlType GetXamlType(string ns, string name, IList<XamlType> typeArguments)
	{
		XamlType[] array = new XamlType[typeArguments.Count];
		typeArguments.CopyTo(array, 0);
		XamlType xamlType = _schemaContext.GetXamlType(ns, name, array);
		if (xamlType != null && !xamlType.IsVisibleTo(LocalAssembly))
		{
			xamlType = null;
		}
		return xamlType;
	}
}
