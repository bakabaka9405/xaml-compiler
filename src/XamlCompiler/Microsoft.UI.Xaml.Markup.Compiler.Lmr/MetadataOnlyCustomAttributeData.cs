using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyCustomAttributeData : CustomAttributeData
{
	private readonly ConstructorInfo m_ctor;

	private readonly MetadataOnlyModule m_module;

	private readonly Token m_token;

	private IList<CustomAttributeTypedArgument> m_typedArguments;

	private IList<CustomAttributeNamedArgument> m_namedArguments;

	public override ConstructorInfo Constructor => m_ctor;

	public override IList<CustomAttributeTypedArgument> ConstructorArguments
	{
		get
		{
			if (m_typedArguments == null)
			{
				InitArgumentData();
			}
			return m_typedArguments;
		}
	}

	public override IList<CustomAttributeNamedArgument> NamedArguments
	{
		get
		{
			if (m_namedArguments == null)
			{
				InitArgumentData();
			}
			return m_namedArguments;
		}
	}

	public MetadataOnlyCustomAttributeData(MetadataOnlyModule module, Token token, ConstructorInfo ctor)
	{
		m_ctor = ctor;
		m_token = token;
		m_module = module;
	}

	public MetadataOnlyCustomAttributeData(ConstructorInfo ctor, IList<CustomAttributeTypedArgument> typedArguments, IList<CustomAttributeNamedArgument> namedArguments)
	{
		m_ctor = ctor;
		m_typedArguments = typedArguments;
		m_namedArguments = namedArguments;
	}

	private void InitArgumentData()
	{
		m_module.LazyAttributeParse(m_token, m_ctor, out var constructorArguments, out var namedArguments);
		m_typedArguments = constructorArguments;
		m_namedArguments = namedArguments;
	}
}
