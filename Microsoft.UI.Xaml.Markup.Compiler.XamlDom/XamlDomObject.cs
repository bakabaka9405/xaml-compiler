using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

[DebuggerDisplay("<{Type.Name}>")]
[ContentProperty("MemberNodes")]
internal class XamlDomObject : XamlDomItem, System.Windows.Markup.IXamlTypeResolver, IXamlNamespaceResolver
{
	private SealableNamespaceCollection namespaces;

	private XamlSchemaContext schemaContext;

	private XamlType type;

	private bool isGetObject;

	private Type unresolvedType;

	private XamlNodeCollection<XamlDomMember> memberNodes;

	public ApiInformation ApiInformation { get; }

	[DefaultValue(null)]
	public xPropertyInfo XPropertyInfo { get; set; }

	[DefaultValue(null)]
	public virtual XamlType Type
	{
		get
		{
			return type;
		}
		set
		{
			CheckSealed();
			type = value;
			SchemaContext = type.SchemaContext;
		}
	}

	[DefaultValue(false)]
	public virtual bool IsGetObject
	{
		get
		{
			return isGetObject;
		}
		set
		{
			CheckSealed();
			isGetObject = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public virtual KeyedCollection<string, XamlDomNamespace> Namespaces
	{
		get
		{
			if (namespaces == null)
			{
				namespaces = new SealableNamespaceCollection();
				if (base.IsSealed)
				{
					namespaces.Seal();
				}
			}
			return namespaces;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public virtual IList<XamlDomMember> MemberNodes => Internal_MemberNodes;

	private IList<XamlDomMember> Internal_MemberNodes
	{
		get
		{
			if (memberNodes == null)
			{
				memberNodes = new XamlNodeCollection<XamlDomMember>(this);
				if (base.IsSealed)
				{
					memberNodes.Seal();
				}
			}
			return memberNodes;
		}
	}

	public virtual XamlSchemaContext SchemaContext
	{
		get
		{
			return schemaContext;
		}
		set
		{
			CheckSealed();
			if (Type != null && Type.SchemaContext != XamlLanguage.Array.SchemaContext && Type.SchemaContext != value)
			{
				throw new InvalidOperationException(ResourceUtilities.FormatString(XamlCompilerResources.XamlDom_TypeDifferentSchemas));
			}
			schemaContext = value;
			Resolve();
		}
	}

	public IEnumerable<XamlDomNamespace> AllXmlnsDefinitions
	{
		get
		{
			for (XamlDomObject current = this; current != null; current = ((current.Parent == null) ? null : current.Parent.Parent))
			{
				foreach (XamlDomNamespace @namespace in current.Namespaces)
				{
					yield return @namespace;
				}
			}
		}
	}

	public XamlDomObject(bool isGetObject, XamlType xamlType, string sourceFilePath)
		: base(sourceFilePath)
	{
		type = xamlType;
		this.isGetObject = isGetObject;
		schemaContext = xamlType?.SchemaContext;
		ApiInformation = (xamlType as DirectUIXamlType)?.ApiInformation;
	}

	public override void Seal()
	{
		base.Seal();
		if (memberNodes != null)
		{
			memberNodes.Seal();
		}
		if (namespaces != null)
		{
			namespaces.Seal();
		}
	}

	public bool HasMember(string instanceMember)
	{
		if (instanceMember == null)
		{
			throw new ArgumentNullException("instanceMember");
		}
		if (instanceMember.Contains("."))
		{
			throw new NotSupportedException(ResourceUtilities.FormatString(XamlCompilerResources.XamlDom_UseHasAttachedMember));
		}
		return Logic_HasMember((XamlType)null, instanceMember);
	}

	public bool HasAttachableMember(XamlType declaringType, string attachableMember)
	{
		if (attachableMember == null)
		{
			throw new ArgumentNullException("attachableMember");
		}
		return HasMember(declaringType.GetAttachableMember(attachableMember));
	}

	public bool HasAttachableMember(Type declaringType, string attachableMember)
	{
		if (attachableMember == null)
		{
			throw new ArgumentNullException("attachableMember");
		}
		return Logic_HasMember(declaringType, attachableMember);
	}

	public virtual bool HasMember(XamlMember xamlMember)
	{
		if (xamlMember == null)
		{
			throw new ArgumentNullException("xamlMember");
		}
		return GetMemberNode(xamlMember) != null;
	}

	public XamlDomMember GetMemberNode(string instanceMember)
	{
		if (instanceMember == null)
		{
			throw new ArgumentNullException("instanceMember");
		}
		if (instanceMember.Contains("."))
		{
			throw new NotSupportedException(ResourceUtilities.FormatString(XamlCompilerResources.XamlDom_UseHasGetAttachedMember));
		}
		return Logic_GetMemberNode((XamlType)null, instanceMember);
	}

	public XamlDomMember GetAttachableMemberNode(XamlType declaringType, string attachableMember)
	{
		if (attachableMember == null)
		{
			throw new ArgumentNullException("attachableMember");
		}
		if (declaringType == null)
		{
			throw new ArgumentNullException("declaringType");
		}
		return Logic_GetMemberNode(declaringType, attachableMember);
	}

	public XamlDomMember GetAttachableMemberNode(Type declaringType, string attachableMember)
	{
		if (attachableMember == null)
		{
			throw new ArgumentNullException("attachableMember");
		}
		if (declaringType == null)
		{
			throw new ArgumentNullException("declaringType");
		}
		return Logic_GetMemberNode(declaringType, attachableMember);
	}

	public virtual XamlDomMember GetMemberNode(XamlMember xamlMember, bool allowPropertyAliasing = true)
	{
		if (xamlMember == null)
		{
			throw new ArgumentNullException("xamlMember");
		}
		XamlDirective xamlDirective = xamlMember as XamlDirective;
		XamlMember xamlMember2 = null;
		if (xamlDirective != null && Type != null && allowPropertyAliasing)
		{
			xamlMember2 = Type.GetAliasedProperty(xamlDirective);
		}
		if (memberNodes != null)
		{
			foreach (XamlDomMember memberNode in memberNodes)
			{
				if (memberNode.Member == xamlMember || (xamlMember2 != null && memberNode.Member == xamlMember2))
				{
					return memberNode;
				}
			}
		}
		return null;
	}

	public void SetAttachableMemberValue(Type declaringType, string attachableMember, object value)
	{
		if (declaringType == null)
		{
			throw new ArgumentNullException("declaringType");
		}
		if (attachableMember == null)
		{
			throw new ArgumentNullException("attachableMember");
		}
		XamlType xamlType = SchemaContext.GetXamlType(declaringType);
		Logic_SetMember(xamlType, attachableMember, value);
	}

	public void SetAttachableMemberValue(XamlType declaringType, string attachableMember, object value)
	{
		if (declaringType == null)
		{
			throw new ArgumentNullException("declaringType");
		}
		if (attachableMember == null)
		{
			throw new ArgumentNullException("attachableMember");
		}
		Logic_SetMember(declaringType, attachableMember, value);
	}

	public void SetMemberValue(string instanceMember, object value)
	{
		if (instanceMember == null)
		{
			throw new ArgumentNullException("instanceMember");
		}
		Logic_SetMember(null, instanceMember, value);
	}

	public virtual void SetMemberValue(XamlMember xamlMember, object value)
	{
		if (xamlMember == null)
		{
			throw new ArgumentNullException("xamlMember");
		}
		XamlDomMember xamlDomMember = GetMemberNode(xamlMember);
		if (xamlDomMember == null)
		{
			xamlDomMember = new XamlDomMember(xamlMember, base.SourceFilePath);
			MemberNodes.Add(xamlDomMember);
		}
		xamlDomMember.Item = new XamlDomValue(value, base.SourceFilePath);
	}

	public virtual XamlDomMember RemoveMember(XamlMember xamlMember)
	{
		if (xamlMember == null)
		{
			throw new ArgumentNullException("xamlMember");
		}
		return RemoveMemberNode(GetMemberNode(xamlMember));
	}

	public virtual XamlDomMember RemoveMemberNode(XamlDomMember node)
	{
		if (node == null)
		{
			throw new ArgumentNullException("node");
		}
		if (memberNodes != null && memberNodes.Remove(node))
		{
			return node;
		}
		return null;
	}

	public string ResolveXmlPrefix(string prefix)
	{
		foreach (XamlDomNamespace allXmlnsDefinition in AllXmlnsDefinitions)
		{
			if (allXmlnsDefinition.NamespaceDeclaration.Prefix == prefix)
			{
				return allXmlnsDefinition.NamespaceDeclaration.Namespace;
			}
		}
		return null;
	}

	public XamlTypeName ResolveXmlNameToTypeName(string xName)
	{
		SplitQualifiedName(xName, out var prefix, out var name);
		string text = ResolveXmlPrefix(prefix);
		if (text != null)
		{
			return new XamlTypeName(text, name);
		}
		return null;
	}

	public virtual XamlType ResolveXmlName(string xName)
	{
		SplitQualifiedName(xName, out var prefix, out var name);
		string text = ResolveXmlPrefix(prefix);
		if (text != null)
		{
			return SchemaContext.GetXamlType(new XamlTypeName(text, name));
		}
		return null;
	}

	public XamlMember ResolveMemberName(XamlType xamlTargetType, string longPropertyName)
	{
		XamlMember result = null;
		int num = longPropertyName.IndexOf('.');
		if (num == -1)
		{
			result = xamlTargetType.GetMember(longPropertyName);
		}
		else
		{
			string xName = longPropertyName.Substring(0, num);
			XamlType xamlType = ResolveXmlName(xName);
			if (xamlType != null)
			{
				string shortPropertyName = longPropertyName.Substring(num + 1);
				result = ResolveMemberName(xamlTargetType, xamlType, shortPropertyName);
			}
		}
		return result;
	}

	public XamlMember ResolveMemberName(string longPropertyName)
	{
		int num = longPropertyName.IndexOf('.');
		if (num == -1)
		{
			throw new ArgumentOutOfRangeException(longPropertyName);
		}
		string shortPropertyName = longPropertyName.Substring(num + 1);
		string xName = longPropertyName.Substring(0, num);
		XamlType xamlType = ResolveXmlName(xName);
		return ResolveMemberName(xamlType, xamlType, shortPropertyName);
	}

	public XamlMember ResolveMemberName(XamlType xamlTargetType, XamlType memberType, string shortPropertyName)
	{
		XamlMember xamlMember = null;
		if (xamlTargetType.CanAssignTo(memberType))
		{
			xamlMember = memberType.GetMember(shortPropertyName);
		}
		if (xamlMember == null)
		{
			xamlMember = memberType.GetAttachableMember(shortPropertyName);
		}
		return xamlMember;
	}

	public virtual Type Resolve(string qualifiedTypeName)
	{
		SplitQualifiedName(qualifiedTypeName, out var prefix, out var name);
		XamlType xamlType = SchemaContext.GetXamlType(new XamlTypeName(prefix, name));
		if (!(xamlType != null))
		{
			return null;
		}
		return xamlType.UnderlyingType;
	}

	public virtual string GetNamespace(string prefix)
	{
		if (namespaces != null && namespaces.Contains(prefix))
		{
			return namespaces[prefix].NamespaceDeclaration.Namespace;
		}
		if (base.Parent != null)
		{
			return base.Parent.LookupNamespaceByPrefix(prefix);
		}
		return null;
	}

	public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes()
	{
		XamlDomObject objectNode = this;
		List<string> prefixes = new List<string>();
		while (objectNode != null)
		{
			if (objectNode.namespaces != null)
			{
				foreach (XamlDomNamespace @namespace in objectNode.Namespaces)
				{
					if (!prefixes.Contains(@namespace.NamespaceDeclaration.Prefix))
					{
						prefixes.Add(@namespace.NamespaceDeclaration.Prefix);
						yield return @namespace.NamespaceDeclaration;
					}
				}
			}
			objectNode = ((objectNode.Parent == null) ? null : objectNode.Parent.Parent);
		}
	}

	internal void Resolve()
	{
		if (schemaContext == null && base.Parent != null && base.Parent.SchemaContext != null)
		{
			schemaContext = base.Parent.SchemaContext;
		}
		if (type == null && unresolvedType != null)
		{
			type = schemaContext.GetXamlType(unresolvedType);
			unresolvedType = null;
		}
		foreach (XamlDomMember internal_MemberNode in Internal_MemberNodes)
		{
			internal_MemberNode.Resolve();
		}
	}

	private XamlMember ResolveXamlMember(XamlType declaringType, string member)
	{
		if (declaringType != null)
		{
			return declaringType.GetAttachableMember(member);
		}
		if (!IsGetObject)
		{
			return Type.GetMember(member);
		}
		if (base.Parent != null)
		{
			base.Parent.Member.Type.GetMember(member);
		}
		return null;
	}

	private XamlDomMember Logic_GetMemberNode(Type declaringType, string member)
	{
		return Logic_GetMemberNode((declaringType != null) ? SchemaContext.GetXamlType(declaringType) : null, member);
	}

	private XamlDomMember Logic_GetMemberNode(XamlType declaringXamlType, string member)
	{
		XamlMember xamlMember = ResolveXamlMember(declaringXamlType, member);
		if (xamlMember == null)
		{
			return null;
		}
		return GetMemberNode(xamlMember);
	}

	private bool Logic_HasMember(Type declaringType, string member)
	{
		return Logic_HasMember((declaringType != null) ? SchemaContext.GetXamlType(declaringType) : null, member);
	}

	private bool Logic_HasMember(XamlType declaringXamlType, string member)
	{
		XamlMember xamlMember = ResolveXamlMember(declaringXamlType, member);
		if (xamlMember == null)
		{
			return false;
		}
		return HasMember(xamlMember);
	}

	private void Logic_SetMember(XamlType declaringXamlType, string member, object value)
	{
		XamlMember xamlMember = ResolveXamlMember(declaringXamlType, member);
		if (!(xamlMember == null))
		{
			SetMemberValue(xamlMember, value);
		}
	}

	private static void SplitQualifiedName(string qualifiedName, out string prefix, out string name)
	{
		prefix = string.Empty;
		name = qualifiedName;
		int num = qualifiedName.IndexOf(':');
		if (num != -1)
		{
			prefix = qualifiedName.Substring(0, num);
			name = qualifiedName.Substring(num + 1);
		}
	}
}
