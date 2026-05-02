using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ProxyTypes;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

internal class ProxyDirectUIXamlMember : DirectUIXamlMember
{
	private readonly MemberProxyMetadata metadata;

	public ProxyDirectUIXamlMember(MemberProxyMetadata metadata, DirectUIXamlType declaringType)
		: base(metadata.Name, declaringType)
	{
		this.metadata = metadata;
	}

	protected override XamlMemberInvoker LookupInvoker()
	{
		return metadata.Invoker;
	}

	protected override XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
	{
		return metadata.DeferringLoader;
	}

	protected override IList<XamlMember> LookupDependsOn()
	{
		return metadata.DependsOn;
	}

	protected override bool LookupIsAmbient()
	{
		return metadata.IsAmbient;
	}

	protected override bool LookupIsEvent()
	{
		return metadata.IsEvent;
	}

	protected override bool LookupIsReadPublic()
	{
		return metadata.IsReadPublic;
	}

	protected override bool LookupIsReadOnly()
	{
		return metadata.IsReadOnly;
	}

	protected override bool LookupIsUnknown()
	{
		return metadata.IsUnknown;
	}

	protected override bool LookupIsWriteOnly()
	{
		return metadata.IsWriteOnly;
	}

	protected override bool LookupIsWritePublic()
	{
		return metadata.IsWritePublic;
	}

	protected override XamlType LookupTargetType()
	{
		return metadata.TargetType;
	}

	protected override XamlValueConverter<TypeConverter> LookupTypeConverter()
	{
		return metadata.TypeConverter;
	}

	protected override XamlValueConverter<ValueSerializer> LookupValueSerializer()
	{
		return metadata.ValueSerializer;
	}

	protected override XamlType LookupType()
	{
		return metadata.Type;
	}

	protected override MethodInfo LookupUnderlyingGetter()
	{
		return null;
	}

	protected override MethodInfo LookupUnderlyingSetter()
	{
		return null;
	}

	protected override MemberInfo LookupUnderlyingMember()
	{
		return null;
	}

	public override IList<string> GetXamlNamespaces()
	{
		return metadata.XamlNamespaces;
	}

	protected override IReadOnlyDictionary<char, char> LookupMarkupExtensionBracketCharacters()
	{
		if (base.DeclaringType.UnderlyingType.FullName == typeof(BindExtension).FullName && base.Name == "Path")
		{
			return new Dictionary<char, char>
			{
				{ '(', ')' },
				{ '[', ']' }
			};
		}
		return null;
	}
}
