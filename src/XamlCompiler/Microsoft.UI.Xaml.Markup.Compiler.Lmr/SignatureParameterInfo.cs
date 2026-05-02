using System;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class SignatureParameterInfo : SimpleParameterInfo
{
	private CustomModifiers m_modifiers;

	public SignatureParameterInfo(MemberInfo member, Type paramType, int position, CustomModifiers modifiers)
		: base(member, paramType, position)
	{
		m_modifiers = modifiers;
	}

	public override Type[] GetOptionalCustomModifiers()
	{
		return m_modifiers.OptionalCustomModifiers;
	}

	public override Type[] GetRequiredCustomModifiers()
	{
		return m_modifiers.RequiredCustomModifiers;
	}
}
