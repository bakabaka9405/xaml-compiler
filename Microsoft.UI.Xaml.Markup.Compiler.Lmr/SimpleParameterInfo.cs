using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class SimpleParameterInfo : ParameterInfo
{
	private readonly MemberInfo m_member;

	private readonly Type m_paramType;

	private readonly int m_position;

	public override int Position => m_position;

	public override Type ParameterType => m_paramType;

	public override MemberInfo Member => m_member;

	public override int MetadataToken => 134217728;

	public override ParameterAttributes Attributes => ParameterAttributes.None;

	public override string Name => string.Empty;

	public override object DefaultValue => null;

	public override object RawDefaultValue => null;

	internal SimpleParameterInfo(MemberInfo member, Type paramType, int position)
	{
		m_member = member;
		m_paramType = paramType;
		m_position = position;
	}

	public override string ToString()
	{
		StringBuilder builder = StringBuilderPool.Get();
		MetadataOnlyCommonType.TypeSigToString(ParameterType, builder);
		builder.Append(' ');
		string result = builder.ToString();
		StringBuilderPool.Release(ref builder);
		return result;
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return new CustomAttributeData[0];
	}

	public override Type[] GetOptionalCustomModifiers()
	{
		return Type.EmptyTypes;
	}

	public override Type[] GetRequiredCustomModifiers()
	{
		return Type.EmptyTypes;
	}
}
