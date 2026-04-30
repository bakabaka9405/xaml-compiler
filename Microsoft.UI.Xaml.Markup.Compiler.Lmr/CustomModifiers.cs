using System;
using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class CustomModifiers
{
	private readonly List<Type> m_optional;

	private readonly List<Type> m_required;

	public Type[] OptionalCustomModifiers
	{
		get
		{
			if (m_optional != null)
			{
				return m_optional.ToArray();
			}
			return Type.EmptyTypes;
		}
	}

	public Type[] RequiredCustomModifiers
	{
		get
		{
			if (m_required != null)
			{
				return m_required.ToArray();
			}
			return Type.EmptyTypes;
		}
	}

	public CustomModifiers(List<Type> optModifiers, List<Type> reqModifiers)
	{
		m_optional = optModifiers;
		m_required = reqModifiers;
	}
}
