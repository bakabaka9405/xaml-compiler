using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyPropertyInfo : PropertyInfo
{
	private readonly MetadataOnlyModule m_resolver;

	private readonly Token m_PropertyToken;

	private readonly PropertyAttributes m_attrib;

	private readonly Token m_declaringClassToken;

	private readonly Type m_propertyType;

	private readonly GenericContext m_context;

	private string m_name;

	private int m_nameLength;

	private readonly Token m_setterToken;

	private readonly Token m_getterToken;

	public override PropertyAttributes Attributes => m_attrib;

	public override MemberTypes MemberType => MemberTypes.Property;

	public override string Name
	{
		get
		{
			InitializeName();
			return m_name;
		}
	}

	public override Type ReflectedType
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override Type PropertyType => m_propertyType;

	public override Type DeclaringType => m_resolver.GetGenericType(m_declaringClassToken, m_context);

	public override int MetadataToken => m_PropertyToken;

	public override bool CanRead => !m_getterToken.IsNil;

	public override bool CanWrite => !m_setterToken.IsNil;

	public override Module Module => m_resolver;

	public MetadataOnlyPropertyInfo(MetadataOnlyModule resolver, Token propToken, Type[] typeArgs, Type[] methodArgs)
	{
		m_resolver = resolver;
		m_PropertyToken = propToken;
		m_context = new GenericContext(typeArgs, methodArgs);
		IMetadataImport rawImport = m_resolver.RawImport;
		rawImport.GetPropertyProps(m_PropertyToken, out m_declaringClassToken, null, 0, out m_nameLength, out var pdwPropFlags, out var ppvSig, out var pbSig, out var _, out var _, out var _, out var pmdSetter, out var pmdGetter, out var _, 1u, out var _);
		m_attrib = pdwPropFlags;
		byte[] sig = m_resolver.ReadEmbeddedBlob(ppvSig, pbSig);
		int index = 0;
		CorCallingConvention corCallingConvention = SignatureUtil.ExtractCallingConvention(sig, ref index);
		SignatureUtil.ExtractInt(sig, ref index);
		m_propertyType = SignatureUtil.ExtractType(sig, ref index, m_resolver, m_context);
		m_setterToken = pmdSetter;
		m_getterToken = pmdGetter;
	}

	private void InitializeName()
	{
		if (string.IsNullOrEmpty(m_name))
		{
			IMetadataImport rawImport = m_resolver.RawImport;
			StringBuilder builder = StringBuilderPool.Get(m_nameLength);
			rawImport.GetPropertyProps(m_PropertyToken, out var _, builder, builder.Capacity, out m_nameLength, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, out var _, 1u, out var _);
			m_name = builder.ToString();
			StringBuilderPool.Release(ref builder);
		}
	}

	public override string ToString()
	{
		return DeclaringType.ToString() + "." + Name;
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		throw new NotSupportedException();
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		throw new NotSupportedException();
	}

	public override bool IsDefined(Type attributeType, bool inherit)
	{
		throw new NotSupportedException();
	}

	public override object GetConstantValue()
	{
		throw new NotImplementedException();
	}

	public override MethodInfo[] GetAccessors(bool nonPublic)
	{
		List<MethodInfo> list = new List<MethodInfo>();
		MethodInfo getMethod = GetGetMethod(nonPublic);
		if (getMethod != null)
		{
			list.Add(getMethod);
		}
		MethodInfo setMethod = GetSetMethod(nonPublic);
		if (setMethod != null)
		{
			list.Add(setMethod);
		}
		return list.ToArray();
	}

	public override MethodInfo GetGetMethod(bool nonPublic)
	{
		if (m_getterToken.IsNil)
		{
			return null;
		}
		MethodInfo genericMethodInfo = m_resolver.GetGenericMethodInfo(m_getterToken, m_context);
		if (nonPublic || genericMethodInfo.IsPublic)
		{
			return genericMethodInfo;
		}
		return null;
	}

	public override MethodInfo GetSetMethod(bool nonPublic)
	{
		if (m_setterToken.IsNil)
		{
			return null;
		}
		MethodInfo genericMethodInfo = m_resolver.GetGenericMethodInfo(m_setterToken, m_context);
		if (nonPublic || genericMethodInfo.IsPublic)
		{
			return genericMethodInfo;
		}
		return null;
	}

	public override ParameterInfo[] GetIndexParameters()
	{
		MethodInfo getMethod = GetGetMethod(nonPublic: true);
		if (getMethod != null)
		{
			return getMethod.GetParameters();
		}
		return new ParameterInfo[0];
	}

	public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	public override bool Equals(object obj)
	{
		MetadataOnlyPropertyInfo metadataOnlyPropertyInfo = obj as MetadataOnlyPropertyInfo;
		if (metadataOnlyPropertyInfo != null)
		{
			if (metadataOnlyPropertyInfo.m_resolver.Equals(m_resolver) && metadataOnlyPropertyInfo.m_PropertyToken.Equals(m_PropertyToken))
			{
				return DeclaringType.Equals(metadataOnlyPropertyInfo.DeclaringType);
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_resolver.GetHashCode() * 32767 + m_PropertyToken.GetHashCode();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return m_resolver.GetCustomAttributeData(MetadataToken);
	}
}
