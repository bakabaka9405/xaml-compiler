using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using Microsoft.UI.Xaml.Markup.Compiler.Lmr;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

internal class ProxyDirectUIXamlType : DirectUIXamlType
{
	private readonly TypeProxyMetadata _metadata;

	public ProxyDirectUIXamlType(TypeProxyMetadata metadata, DirectUISchemaContext schemaContext)
		: base(metadata.Name, null, schemaContext)
	{
		_metadata = metadata;
	}

	protected override XamlMember LookupAliasedProperty(XamlDirective directive)
	{
		return null;
	}

	protected override IList<XamlType> LookupAllowedContentTypes()
	{
		return null;
	}

	protected override XamlType LookupBaseType()
	{
		Type proxyType = GetProxyType(_metadata.BaseTypeName);
		return base.SchemaContext.GetXamlType(proxyType);
	}

	protected override XamlCollectionKind LookupCollectionKind()
	{
		return _metadata.CollectionKind;
	}

	protected override bool LookupConstructionRequiresArguments()
	{
		return _metadata.ConstructionRequiresArguments;
	}

	protected override XamlMember LookupContentProperty()
	{
		return null;
	}

	protected override IList<XamlType> LookupContentWrappers()
	{
		return null;
	}

	protected override XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
	{
		return null;
	}

	protected override bool LookupIsConstructible()
	{
		return _metadata.IsConstructible;
	}

	protected override XamlTypeInvoker LookupInvoker()
	{
		return null;
	}

	protected override bool LookupIsMarkupExtension()
	{
		return _metadata.IsMarkupExtension;
	}

	protected override bool LookupIsNameScope()
	{
		return _metadata.IsNameScope;
	}

	protected override bool LookupIsNullable()
	{
		return _metadata.IsNullable;
	}

	protected override bool LookupIsUnknown()
	{
		return _metadata.IsUnknown;
	}

	protected override bool LookupIsWhitespaceSignificantCollection()
	{
		return _metadata.IsWhitespaceSignificantCollection;
	}

	protected override XamlType LookupKeyType()
	{
		return null;
	}

	protected override XamlType LookupItemType()
	{
		return null;
	}

	protected override XamlType LookupMarkupExtensionReturnType()
	{
		return null;
	}

	protected override IEnumerable<XamlMember> LookupAllAttachableMembers()
	{
		return null;
	}

	protected override IEnumerable<XamlMember> LookupAllMembers()
	{
		List<XamlMember> list = new List<XamlMember>();
		if (_metadata.MemberNamesAndMetadata != null)
		{
			DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
			foreach (string key in _metadata.MemberNamesAndMetadata.Keys)
			{
				XamlTypeName xamlTypeName = _metadata.MemberNamesAndMetadata[key];
				XamlType xamlType = directUISchemaContext.GetXamlType(xamlTypeName);
				MemberProxyMetadata metadata = new MemberProxyMetadata(key, xamlType);
				list.Add(new ProxyDirectUIXamlMember(metadata, this));
			}
		}
		return list;
	}

	protected override XamlMember LookupMember(string name, bool skipReadOnlyCheck)
	{
		XamlMember result = null;
		if (_metadata.MemberNamesAndMetadata != null && _metadata.MemberNamesAndMetadata.TryGetValue(name, out var value))
		{
			DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
			XamlType xamlType = directUISchemaContext.GetXamlType(value);
			MemberProxyMetadata metadata = new MemberProxyMetadata(name, xamlType);
			result = new ProxyDirectUIXamlMember(metadata, this);
		}
		return result;
	}

	protected override XamlMember LookupAttachableMember(string name)
	{
		return null;
	}

	protected override IList<XamlType> LookupPositionalParameters(int parameterCount)
	{
		return null;
	}

	protected override Type LookupUnderlyingType()
	{
		return GetProxyType(_metadata.UnderlyingTypeName);
	}

	protected override bool LookupIsPublic()
	{
		return _metadata.IsPublic;
	}

	protected override bool LookupIsXData()
	{
		return _metadata.IsXData;
	}

	protected override bool LookupIsAmbient()
	{
		return _metadata.IsAmbient;
	}

	protected override XamlValueConverter<TypeConverter> LookupTypeConverter()
	{
		return null;
	}

	protected override XamlValueConverter<ValueSerializer> LookupValueSerializer()
	{
		return null;
	}

	private Type GetProxyType(string typeName)
	{
		DirectUISchemaContext directUISchemaContext = base.SchemaContext as DirectUISchemaContext;
		Type type = null;
		foreach (XamlTypeUniverse xamlTypeUniverse in directUISchemaContext.DirectUISystem.XamlTypeUniverses)
		{
			Assembly xamlProxyAssembly = xamlTypeUniverse.GetXamlProxyAssembly();
			type = xamlProxyAssembly.GetType(typeName);
			if (type != null)
			{
				break;
			}
		}
		return type;
	}
}
