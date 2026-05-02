using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyConstructorInfo : ConstructorInfo
{
	private readonly MethodBase m_method;

	public override int MetadataToken => m_method.MetadataToken;

	public override Module Module => m_method.Module;

	public override Type DeclaringType => m_method.DeclaringType;

	public override string Name => m_method.Name;

	public override Type ReflectedType
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override MethodAttributes Attributes => m_method.Attributes;

	public override CallingConventions CallingConvention => m_method.CallingConvention;

	public override MemberTypes MemberType => MemberTypes.Constructor;

	public override bool IsGenericMethodDefinition => m_method.IsGenericMethodDefinition;

	public override RuntimeMethodHandle MethodHandle
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public MetadataOnlyConstructorInfo(MethodBase method)
	{
		m_method = method;
	}

	public override string ToString()
	{
		return m_method.ToString();
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

	public override ParameterInfo[] GetParameters()
	{
		return m_method.GetParameters();
	}

	public override MethodBody GetMethodBody()
	{
		return m_method.GetMethodBody();
	}

	public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	public override MethodImplAttributes GetMethodImplementationFlags()
	{
		return m_method.GetMethodImplementationFlags();
	}

	public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return m_method.GetCustomAttributesData();
	}

	public override bool Equals(object obj)
	{
		MetadataOnlyConstructorInfo metadataOnlyConstructorInfo = obj as MetadataOnlyConstructorInfo;
		if (metadataOnlyConstructorInfo == null)
		{
			return false;
		}
		return m_method.Equals(metadataOnlyConstructorInfo.m_method);
	}

	public override int GetHashCode()
	{
		return m_method.GetHashCode();
	}
}
