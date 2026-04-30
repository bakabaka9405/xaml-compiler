using System.Diagnostics;
using System.Xaml;
using System.Xaml.MS.Impl;
using System.Xml;
using MS.Internal.Xaml.Context;

namespace MS.Internal.Xaml.Parser;

[DebuggerDisplay("{Name.ScopedName}='{Value}'  {Kind}")]
internal class XamlAttribute
{
	private string _xmlnsDefinitionPrefix;

	private string _xmlnsDefinitionUri;

	public XamlPropertyName Name { get; private set; }

	public string Value { get; private set; }

	public ScannerAttributeKind Kind { get; private set; }

	public XamlMember Property { get; private set; }

	public int LineNumber { get; private set; }

	public int LinePosition { get; private set; }

	public string XmlNsPrefixDefined => _xmlnsDefinitionPrefix;

	public string XmlNsUriDefined => _xmlnsDefinitionUri;

	public XamlAttribute(XamlPropertyName propName, string val, IXmlLineInfo lineInfo)
	{
		Name = propName;
		Value = val;
		Kind = ScannerAttributeKind.Property;
		if (lineInfo != null)
		{
			LineNumber = lineInfo.LineNumber;
			LinePosition = lineInfo.LinePosition;
		}
		if (CheckIsXmlNamespaceDefinition(out _xmlnsDefinitionPrefix, out _xmlnsDefinitionUri))
		{
			Kind = ScannerAttributeKind.Namespace;
		}
	}

	public void Initialize(XamlParserContext context, XamlType tagType, string ownerNamespace, bool tagIsRoot)
	{
		if (Kind == ScannerAttributeKind.Namespace)
		{
			return;
		}
		Property = GetXamlAttributeProperty(context, Name, tagType, ownerNamespace, tagIsRoot);
		if (Property.IsUnknown)
		{
			Kind = ScannerAttributeKind.Unknown;
		}
		else if (Property.IsEvent)
		{
			Kind = ScannerAttributeKind.Event;
		}
		else if (Property.IsDirective)
		{
			if (Property == XamlLanguage.Space)
			{
				Kind = ScannerAttributeKind.XmlSpace;
			}
			else if (Property == XamlLanguage.FactoryMethod || Property == XamlLanguage.Arguments || Property == XamlLanguage.TypeArguments || Property == XamlLanguage.Base)
			{
				Kind = ScannerAttributeKind.CtorDirective;
			}
			else
			{
				Kind = ScannerAttributeKind.Directive;
			}
		}
		else if (Property.IsAttachable)
		{
			Kind = ScannerAttributeKind.AttachableProperty;
		}
		else if (Property == tagType.GetAliasedProperty(XamlLanguage.Name))
		{
			Kind = ScannerAttributeKind.Name;
		}
		else
		{
			Kind = ScannerAttributeKind.Property;
		}
	}

	internal bool CheckIsXmlNamespaceDefinition(out string definingPrefix, out string uri)
	{
		uri = string.Empty;
		definingPrefix = string.Empty;
		if (KS.Eq(Name.Prefix, "xmlns"))
		{
			uri = Value;
			definingPrefix = ((!Name.IsDotted) ? Name.Name : (Name.OwnerName + "." + Name.Name));
			return true;
		}
		if (string.IsNullOrEmpty(Name.Prefix) && KS.Eq(Name.Name, "xmlns"))
		{
			uri = Value;
			definingPrefix = string.Empty;
			return true;
		}
		return false;
	}

	private XamlMember GetXamlAttributeProperty(XamlParserContext context, XamlPropertyName propName, XamlType tagType, string tagNamespace, bool tagIsRoot)
	{
		string attributeNamespace = context.GetAttributeNamespace(propName, tagNamespace);
		if (attributeNamespace == null)
		{
			if (propName.IsDotted)
			{
				XamlType declaringType = new XamlType(string.Empty, propName.OwnerName, null, context.SchemaContext);
				return new XamlMember(propName.Name, declaringType, isAttachable: true);
			}
			return new XamlMember(propName.Name, tagType, isAttachable: false);
		}
		if (propName.IsDotted)
		{
			return context.GetDottedProperty(tagType, tagNamespace, propName, tagIsRoot);
		}
		return context.GetNoDotAttributeProperty(tagType, propName, tagNamespace, attributeNamespace, tagIsRoot);
	}
}
