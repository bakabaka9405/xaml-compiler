using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Adds;
using System.Windows.Markup;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

[DebuggerDisplay("{Member.Name}")]
[ContentProperty("ItemNodes")]
internal class XamlDomMember : XamlDomNode, IXamlDomMember
{
	private XamlMember member;

	private XamlNodeCollection<XamlDomItem> items;

	private XamlDomObject parent;

	private XamlSchemaContext schemaContext;

	private string unresolvedMemberName;

	private Type unresolvedDeclaringType;

	public ApiInformation ApiInformation { get; }

	public virtual XamlSchemaContext SchemaContext
	{
		get
		{
			return schemaContext;
		}
		set
		{
			CheckSealed();
			if (Member != null && !Member.IsDirective && Member.Type.SchemaContext != value)
			{
				throw new InvalidOperationException(ResourceUtilities.FormatString(XamlCompilerResources.XamlDom_MemberDifferentSchemas));
			}
			schemaContext = value;
		}
	}

	[DefaultValue(null)]
	public virtual XamlMember Member
	{
		get
		{
			return member;
		}
		set
		{
			CheckSealed();
			member = value;
			schemaContext = member.Type.SchemaContext;
			Resolve();
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public XamlDomObject Parent
	{
		get
		{
			return parent;
		}
		set
		{
			CheckSealed();
			parent = value;
		}
	}

	public virtual XamlDomItem Item
	{
		get
		{
			return Internal_Item;
		}
		set
		{
			Internal_Item = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public virtual IList<XamlDomItem> Items => Internal_Items;

	private XamlDomItem Internal_Item
	{
		get
		{
			if (Internal_Items.Count > 1)
			{
				throw new NotSupportedException(ResourceUtilities.FormatString(XamlCompilerResources.XamlDom_MemberHasMoreThanOneItem, Member.Name));
			}
			if (Internal_Items.Count == 0)
			{
				return null;
			}
			return Internal_Items[0];
		}
		set
		{
			Internal_Items.Clear();
			Internal_Items.Add(value);
		}
	}

	private IList<XamlDomItem> Internal_Items
	{
		get
		{
			if (items == null)
			{
				items = new XamlNodeCollection<XamlDomItem>(this);
				if (base.IsSealed)
				{
					items.Seal();
				}
			}
			return items;
		}
	}

	public XamlDomMember(XamlMember xamlMember, string sourceFilePath)
		: base(sourceFilePath)
	{
		member = xamlMember;
		if (xamlMember != null)
		{
			try
			{
				schemaContext = xamlMember.Type.SchemaContext;
			}
			catch (UnresolvedAssemblyException)
			{
				schemaContext = xamlMember.DeclaringType.SchemaContext;
			}
			catch (TypeLoadException)
			{
				schemaContext = xamlMember.DeclaringType.SchemaContext;
			}
			ApiInformation = (xamlMember as DirectUIXamlMember)?.ApiInformation;
		}
	}

	public override void Seal()
	{
		base.Seal();
		if (items != null)
		{
			items.Seal();
		}
	}

	internal void Resolve()
	{
		if (schemaContext == null && Parent != null && Parent.SchemaContext != null)
		{
			schemaContext = Parent.SchemaContext;
		}
		if (member == null && unresolvedMemberName != null)
		{
			if (unresolvedDeclaringType != null)
			{
				member = schemaContext.GetXamlType(unresolvedDeclaringType).GetAttachableMember(unresolvedMemberName);
			}
			else
			{
				member = Parent.Type.GetMember(unresolvedMemberName);
			}
			unresolvedMemberName = null;
			unresolvedDeclaringType = null;
		}
		foreach (XamlDomItem internal_Item in Internal_Items)
		{
			if (internal_Item is XamlDomObject xamlDomObject)
			{
				xamlDomObject.Resolve();
			}
		}
	}

	internal string LookupNamespaceByPrefix(string prefix)
	{
		return Parent.GetNamespace(prefix);
	}
}
