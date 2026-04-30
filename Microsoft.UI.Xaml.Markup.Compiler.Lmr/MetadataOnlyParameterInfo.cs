using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyParameterInfo : ParameterInfo
{
	private readonly MetadataOnlyModule m_resolver;

	private readonly int m_parameterToken;

	private readonly ParameterAttributes m_attrib;

	private readonly Type m_paramType;

	private readonly CustomModifiers m_customModifiers;

	private string m_name;

	private uint m_nameLength;

	private readonly int m_position;

	private readonly int m_parentMemberToken;

	public override ParameterAttributes Attributes => m_attrib;

	public override string Name
	{
		get
		{
			InitializeName();
			return m_name;
		}
	}

	public override MemberInfo Member => m_resolver.ResolveMethod(m_parentMemberToken);

	public override int Position => m_position;

	public override Type ParameterType => m_paramType;

	public override int MetadataToken => m_parameterToken;

	public override object DefaultValue
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public override object RawDefaultValue
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	internal MetadataOnlyParameterInfo(MetadataOnlyModule resolver, Token parameterToken, Type paramType, CustomModifiers customModifiers)
	{
		m_resolver = resolver;
		m_parameterToken = parameterToken;
		m_paramType = paramType;
		m_customModifiers = customModifiers;
		IMetadataImport rawImport = m_resolver.RawImport;
		rawImport.GetParamProps(m_parameterToken, out m_parentMemberToken, out var pulSequence, null, 0u, out m_nameLength, out var pdwAttr, out var _, out var _, out var _);
		m_position = (int)(pulSequence - 1);
		m_attrib = (ParameterAttributes)pdwAttr;
	}

	private void InitializeName()
	{
		if (string.IsNullOrEmpty(m_name))
		{
			IMetadataImport rawImport = m_resolver.RawImport;
			StringBuilder builder = StringBuilderPool.Get((int)m_nameLength);
			rawImport.GetParamProps(m_parameterToken, out var _, out var _, builder, (uint)builder.Capacity, out var _, out var _, out var _, out var _, out var _);
			m_name = builder.ToString();
			StringBuilderPool.Release(ref builder);
		}
	}

	public override Type[] GetOptionalCustomModifiers()
	{
		if (m_customModifiers == null)
		{
			return Type.EmptyTypes;
		}
		return m_customModifiers.OptionalCustomModifiers;
	}

	public override Type[] GetRequiredCustomModifiers()
	{
		if (m_customModifiers == null)
		{
			return Type.EmptyTypes;
		}
		return m_customModifiers.RequiredCustomModifiers;
	}

	public override bool Equals(object obj)
	{
		if (obj is MetadataOnlyParameterInfo metadataOnlyParameterInfo)
		{
			if (metadataOnlyParameterInfo.m_resolver.Equals(m_resolver))
			{
				return metadataOnlyParameterInfo.m_parameterToken.Equals(m_parameterToken);
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_resolver.GetHashCode() * 32767 + m_parameterToken.GetHashCode();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return m_resolver.GetCustomAttributeData(MetadataToken);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} {1}", MetadataOnlyCommonType.TypeSigToString(ParameterType), Name);
	}

	public MarshalAsAttribute GetMarshalInfo()
	{
		m_resolver.RawImport.GetFieldMarshal(m_parameterToken, out var pNativeType, out var cbNativeType);
		byte[] array = m_resolver.RawMetadata.ReadEmbeddedBlob(pNativeType, cbNativeType);
		int index = 0;
		UnmanagedType unmanagedType = SignatureUtil.ExtractUnmanagedType(array, ref index);
		MarshalAsAttribute marshalAsAttribute = new MarshalAsAttribute(unmanagedType);
		if (unmanagedType == UnmanagedType.LPArray)
		{
			UnmanagedType arraySubType = SignatureUtil.ExtractUnmanagedType(array, ref index);
			marshalAsAttribute.ArraySubType = arraySubType;
			if (index < array.Length)
			{
				int num = SignatureUtil.ExtractInt(array, ref index);
				marshalAsAttribute.SizeParamIndex = checked((short)num);
				if (index < array.Length)
				{
					int sizeConst = SignatureUtil.ExtractInt(array, ref index);
					marshalAsAttribute.SizeConst = sizeConst;
				}
			}
			else
			{
				marshalAsAttribute.SizeParamIndex = -1;
			}
		}
		return marshalAsAttribute;
	}
}
