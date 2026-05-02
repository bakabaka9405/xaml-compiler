using System.Collections.Generic;
using System.Globalization;

namespace System.Reflection.Adds;

internal abstract class ConstructorInfoProxy : ConstructorInfo
{
	private ConstructorInfo m_cachedResolved;

	public override MethodAttributes Attributes => GetResolvedConstructor().Attributes;

	public override CallingConventions CallingConvention => GetResolvedConstructor().CallingConvention;

	public override bool IsGenericMethodDefinition => GetResolvedConstructor().IsGenericMethodDefinition;

	public override bool ContainsGenericParameters => GetResolvedConstructor().ContainsGenericParameters;

	public override RuntimeMethodHandle MethodHandle
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override MemberTypes MemberType => MemberTypes.Constructor;

	public override Type DeclaringType => GetResolvedConstructor().DeclaringType;

	public override string Name => GetResolvedConstructor().Name;

	public override int MetadataToken => GetResolvedConstructor().MetadataToken;

	public override Module Module => GetResolvedConstructor().Module;

	public override Type ReflectedType => GetResolvedConstructor().ReflectedType;

	protected abstract ConstructorInfo GetResolvedWorker();

	public ConstructorInfo GetResolvedConstructor()
	{
		if (m_cachedResolved == null)
		{
			m_cachedResolved = GetResolvedWorker();
		}
		return m_cachedResolved;
	}

	public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
	{
		return GetResolvedConstructor().Invoke(invokeAttr, binder, parameters, culture);
	}

	public override ParameterInfo[] GetParameters()
	{
		return GetResolvedConstructor().GetParameters();
	}

	public override Type[] GetGenericArguments()
	{
		return GetResolvedConstructor().GetGenericArguments();
	}

	public override MethodBody GetMethodBody()
	{
		return GetResolvedConstructor().GetMethodBody();
	}

	public override MethodImplAttributes GetMethodImplementationFlags()
	{
		return GetResolvedConstructor().GetMethodImplementationFlags();
	}

	public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
	{
		return GetResolvedConstructor().Invoke(obj, invokeAttr, binder, parameters, culture);
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		return GetResolvedConstructor().GetCustomAttributes(inherit);
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		return GetResolvedConstructor().GetCustomAttributes(attributeType, inherit);
	}

	public override bool IsDefined(Type attributeType, bool inherit)
	{
		return GetResolvedConstructor().IsDefined(attributeType, inherit);
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return GetResolvedConstructor().GetCustomAttributesData();
	}
}
