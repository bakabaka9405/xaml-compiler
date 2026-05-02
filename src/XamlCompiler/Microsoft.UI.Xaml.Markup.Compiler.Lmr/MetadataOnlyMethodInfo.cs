using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyMethodInfo : MethodInfo
{
	private readonly Token m_methodDef;

	private string m_name;

	private uint m_nameLength;

	private Type m_tOwner;

	private MethodSignatureDescriptor m_descriptor;

	private ParameterInfo m_returnParameter;

	private MethodBody m_methodBody;

	private MethodAttributes m_attrs;

	private readonly Type[] m_typeArgs;

	private readonly Type[] m_methodArgs;

	private GenericContext m_context;

	private readonly MetadataOnlyModule m_resolver;

	private Token m_declaringTypeDef;

	private SignatureBlob m_sigBlob;

	private bool m_fullyInitialized;

	public override int MetadataToken => m_methodDef.Value;

	internal MetadataOnlyModule Resolver => m_resolver;

	public override Module Module => m_resolver;

	public override Type ReturnType
	{
		get
		{
			if (!m_fullyInitialized)
			{
				Initialize();
			}
			return m_descriptor.ReturnParameter.Type;
		}
	}

	public override Type DeclaringType
	{
		get
		{
			if (!m_fullyInitialized)
			{
				Initialize();
			}
			return m_tOwner;
		}
	}

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

	public override ParameterInfo ReturnParameter
	{
		get
		{
			if (m_returnParameter == null)
			{
				GetParameters();
			}
			if (m_returnParameter == null)
			{
				return Resolver.Policy.GetFakeParameterInfo(this, ReturnType, -1);
			}
			return m_returnParameter;
		}
	}

	public override MethodAttributes Attributes => m_attrs;

	public override CallingConventions CallingConvention
	{
		get
		{
			if (!m_fullyInitialized)
			{
				Initialize();
			}
			CorCallingConvention callingConvention = m_descriptor.CallingConvention;
			CallingConventions callingConventions = (((callingConvention & CorCallingConvention.Mask) != CorCallingConvention.VarArg) ? CallingConventions.Standard : CallingConventions.VarArgs);
			if ((callingConvention & CorCallingConvention.HasThis) != CorCallingConvention.Default)
			{
				callingConventions |= CallingConventions.HasThis;
			}
			if ((callingConvention & CorCallingConvention.ExplicitThis) != CorCallingConvention.Default)
			{
				callingConventions |= CallingConventions.ExplicitThis;
			}
			return callingConventions;
		}
	}

	public override MemberTypes MemberType => MemberTypes.Method;

	public override bool IsGenericMethodDefinition
	{
		get
		{
			if (!m_fullyInitialized)
			{
				Initialize();
			}
			if ((m_descriptor.CallingConvention & CorCallingConvention.Generic) == 0)
			{
				return false;
			}
			if (GenericContext.IsNullOrEmptyMethodArgs(m_context))
			{
				return true;
			}
			MethodInfo methodInfo = Resolver.Factory.CreateMethodOrConstructor(Resolver, m_methodDef, null, null) as MethodInfo;
			Type[] methodArgs = m_context.MethodArgs;
			foreach (Type type in methodArgs)
			{
				if (!type.IsGenericParameter)
				{
					return false;
				}
				if (!methodInfo.Equals(type.DeclaringMethod))
				{
					return false;
				}
			}
			return true;
		}
	}

	public override bool IsGenericMethod
	{
		get
		{
			if (!m_fullyInitialized)
			{
				Initialize();
			}
			return !GenericContext.IsNullOrEmptyMethodArgs(m_context);
		}
	}

	public override bool ContainsGenericParameters
	{
		get
		{
			if (DeclaringType.ContainsGenericParameters)
			{
				return true;
			}
			Type[] genericArguments = GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (genericArguments[i].ContainsGenericParameters)
				{
					return true;
				}
			}
			return false;
		}
	}

	public override RuntimeMethodHandle MethodHandle
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override ICustomAttributeProvider ReturnTypeCustomAttributes
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	internal static MethodBase Create(MetadataOnlyModule resolver, Token methodDef, GenericContext context)
	{
		Type[] typeArgs = Type.EmptyTypes;
		Type[] methodArgs = Type.EmptyTypes;
		if (context != null)
		{
			typeArgs = context.TypeArgs;
			methodArgs = context.MethodArgs;
		}
		return resolver.Factory.CreateMethodOrConstructor(resolver, methodDef, typeArgs, methodArgs);
	}

	public MetadataOnlyMethodInfo(MetadataOnlyMethodInfo method)
	{
		m_resolver = method.m_resolver;
		m_methodDef = method.m_methodDef;
		m_tOwner = method.m_tOwner;
		m_descriptor = method.m_descriptor;
		m_name = method.m_name;
		m_nameLength = method.m_nameLength;
		m_attrs = method.m_attrs;
		m_returnParameter = method.m_returnParameter;
		m_methodBody = method.m_methodBody;
		m_declaringTypeDef = method.m_declaringTypeDef;
		m_sigBlob = method.m_sigBlob;
		m_typeArgs = method.m_typeArgs;
		m_methodArgs = method.m_methodArgs;
		m_context = method.m_context;
		m_fullyInitialized = method.m_fullyInitialized;
	}

	public MetadataOnlyMethodInfo(MetadataOnlyModule resolver, Token methodDef, Type[] typeArgs, Type[] methodArgs)
	{
		m_resolver = resolver;
		m_methodDef = methodDef;
		m_typeArgs = typeArgs;
		m_methodArgs = methodArgs;
		resolver.GetMethodAttrs(methodDef, out m_declaringTypeDef, out m_attrs, out m_nameLength);
	}

	private void InitializeName()
	{
		if (string.IsNullOrEmpty(m_name))
		{
			m_resolver.GetMethodName(m_methodDef, m_nameLength, out m_name);
		}
	}

	private void Initialize()
	{
		Type ownerType = null;
		Type[] typeArgs = null;
		if (!m_declaringTypeDef.IsNil)
		{
			GetOwnerTypeAndTypeArgs(out ownerType, out typeArgs);
		}
		else
		{
			typeArgs = m_typeArgs;
		}
		Type[] genericMethodArgs = GetGenericMethodArgs();
		GenericContext context = new GenericContext(typeArgs, genericMethodArgs);
		m_resolver.GetMethodSig(m_methodDef, out m_sigBlob);
		MethodSignatureDescriptor descriptor = SignatureUtil.ExtractMethodSignature(m_sigBlob, m_resolver, context);
		m_tOwner = ownerType;
		m_context = context;
		m_descriptor = descriptor;
		m_fullyInitialized = true;
	}

	private void GetOwnerTypeAndTypeArgs(out Type ownerType, out Type[] typeArgs)
	{
		Type type = m_resolver.ResolveTypeDefToken(m_declaringTypeDef);
		GenericContext genericContext = new GenericContext(m_typeArgs, m_methodArgs);
		if (type.IsGenericType && GenericContext.IsNullOrEmptyTypeArgs(genericContext))
		{
			genericContext = new GenericContext(type.GetGenericArguments(), m_methodArgs);
		}
		type = m_resolver.GetGenericType(new Token(type.MetadataToken), genericContext);
		ownerType = type;
		typeArgs = genericContext.TypeArgs;
	}

	private Type[] GetGenericMethodArgs()
	{
		Type[] array = null;
		int num = m_resolver.CountGenericParams(m_methodDef);
		bool flag = m_methodArgs != null && m_methodArgs.Length != 0;
		if (num > 0)
		{
			if (!flag)
			{
				array = new Type[num];
				int num2 = 0;
				foreach (int genericParameterToken in m_resolver.GetGenericParameterTokens(m_methodDef))
				{
					array[num2++] = m_resolver.Factory.CreateTypeVariable(m_resolver, new Token(genericParameterToken));
				}
			}
			else
			{
				if (num != m_methodArgs.Length)
				{
					throw new ArgumentException(Resources.WrongNumberOfGenericArguments);
				}
				array = m_methodArgs;
			}
		}
		if (array != null)
		{
			return array;
		}
		return Type.EmptyTypes;
	}

	public override bool Equals(object obj)
	{
		MetadataOnlyMethodInfo metadataOnlyMethodInfo = obj as MetadataOnlyMethodInfo;
		if (metadataOnlyMethodInfo == null)
		{
			return false;
		}
		if (!DeclaringType.Equals(metadataOnlyMethodInfo.DeclaringType))
		{
			return false;
		}
		if (!IsGenericMethod)
		{
			return metadataOnlyMethodInfo.GetHashCode() == GetHashCode();
		}
		if (!metadataOnlyMethodInfo.IsGenericMethod)
		{
			return false;
		}
		Type[] genericArguments = GetGenericArguments();
		Type[] genericArguments2 = metadataOnlyMethodInfo.GetGenericArguments();
		if (genericArguments.Length != genericArguments2.Length)
		{
			return false;
		}
		for (int i = 0; i < genericArguments.Length; i++)
		{
			if (!genericArguments[i].Equals(genericArguments2[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return m_resolver.GetHashCode() * 32767 + m_methodDef.GetHashCode();
	}

	public override string ToString()
	{
		return CommonToString(this);
	}

	internal static string CommonToString(MethodInfo m)
	{
		StringBuilder builder = StringBuilderPool.Get();
		MetadataOnlyCommonType.TypeSigToString(m.ReturnType, builder);
		builder.Append(" ");
		ConstructMethodString(m, builder);
		string result = builder.ToString();
		StringBuilderPool.Release(ref builder);
		return result;
	}

	private static void ConstructMethodString(MethodInfo m, StringBuilder sb)
	{
		sb.Append(m.Name);
		string value = "";
		if (m.IsGenericMethod)
		{
			sb.Append("[");
			Type[] genericArguments = m.GetGenericArguments();
			foreach (Type pThis in genericArguments)
			{
				sb.Append(value);
				MetadataOnlyCommonType.TypeSigToString(pThis, sb);
				value = ",";
			}
			sb.Append("]");
		}
		sb.Append("(");
		ConstructParameters(sb, m.GetParameters(), m.CallingConvention);
		sb.Append(")");
	}

	private static void ConstructParameters(StringBuilder sb, ParameterInfo[] parameters, CallingConventions callingConvention)
	{
		Type[] array = new Type[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			array[i] = parameters[i].ParameterType;
		}
		ConstructParameters(sb, array, callingConvention);
	}

	private static void ConstructParameters(StringBuilder sb, Type[] parameters, CallingConventions callingConvention)
	{
		string value = "";
		foreach (Type type in parameters)
		{
			sb.Append(value);
			MetadataOnlyCommonType.TypeSigToString(type, sb);
			if (type.IsByRef)
			{
				sb.Length--;
				sb.Append(" ByRef");
			}
			value = ", ";
		}
		if ((callingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
		{
			sb.Append(value);
			sb.Append("...");
		}
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
		if (!m_fullyInitialized)
		{
			Initialize();
		}
		int num = m_descriptor.Parameters.Length;
		ParameterInfo[] array = new ParameterInfo[num];
		Type[] array2 = new Type[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = m_descriptor.Parameters[i].Type;
		}
		int[] array3 = new int[num + 1];
		IMetadataImport rawImport = m_resolver.RawImport;
		HCORENUM phEnum = default(HCORENUM);
		uint pcTokens;
		int num2 = rawImport.EnumParams(ref phEnum, m_methodDef.Value, array3, array3.Length, out pcTokens);
		if (num2 == 1)
		{
			for (int j = 0; j < num; j++)
			{
				array[j] = Resolver.Policy.GetFakeParameterInfo(this, array2[j], j);
			}
			return array;
		}
		phEnum.Close(rawImport);
		if (pcTokens == 0)
		{
			return array;
		}
		ParameterInfo parameterInfo = null;
		for (int k = 0; k < pcTokens; k++)
		{
			int num3 = array3[k];
			rawImport.GetParamProps(num3, out var _, out var pulSequence, null, 0u, out var _, out var _, out var _, out var _, out var _);
			if (pulSequence == 0)
			{
				parameterInfo = new MetadataOnlyParameterInfo(m_resolver, new Token(num3), ReturnType, m_descriptor.ReturnParameter.CustomModifiers);
				continue;
			}
			uint num4 = pulSequence - 1;
			array[num4] = new MetadataOnlyParameterInfo(m_resolver, new Token(num3), array2[num4], m_descriptor.Parameters[num4].CustomModifiers);
		}
		if (parameterInfo == null)
		{
			parameterInfo = Resolver.Policy.GetFakeParameterInfo(this, ReturnType, -1);
		}
		m_returnParameter = parameterInfo;
		for (int l = 0; l < num; l++)
		{
			if (array[l] == null)
			{
				array[l] = Resolver.Policy.GetFakeParameterInfo(this, array2[l], l);
			}
		}
		return array;
	}

	public override MethodInfo MakeGenericMethod(params Type[] types)
	{
		if (!IsGenericMethodDefinition)
		{
			throw new InvalidOperationException();
		}
		Type[] typeArgs = m_context.TypeArgs;
		GenericContext context = new GenericContext(typeArgs, types);
		return (MethodInfo)Create(m_resolver, m_methodDef, context);
	}

	public override Type[] GetGenericArguments()
	{
		if (!m_fullyInitialized)
		{
			Initialize();
		}
		return (Type[])m_context.MethodArgs.Clone();
	}

	public override MethodInfo GetGenericMethodDefinition()
	{
		if (!IsGenericMethod)
		{
			throw new InvalidOperationException();
		}
		if (IsGenericMethodDefinition)
		{
			return this;
		}
		return Resolver.Factory.CreateMethodOrConstructor(Resolver, m_methodDef, m_context.TypeArgs, null) as MethodInfo;
	}

	public override MethodBody GetMethodBody()
	{
		if (m_methodBody == null)
		{
			m_methodBody = MetadataOnlyMethodBody.TryCreate(this);
		}
		return m_methodBody;
	}

	public override MethodImplAttributes GetMethodImplementationFlags()
	{
		return m_resolver.GetMethodImplFlags(m_methodDef);
	}

	public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
	{
		throw new NotSupportedException();
	}

	public override MethodInfo GetBaseDefinition()
	{
		if (!base.IsVirtual || base.IsStatic || DeclaringType == null || DeclaringType.IsInterface)
		{
			return this;
		}
		Type baseType = DeclaringType.BaseType;
		if (baseType == null)
		{
			return this;
		}
		List<Type> list = new List<Type>();
		ParameterInfo[] parameters = GetParameters();
		foreach (ParameterInfo parameterInfo in parameters)
		{
			list.Add(parameterInfo.ParameterType);
		}
		MethodInfo method = baseType.GetMethod(Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy, null, CallingConvention, list.ToArray(), null);
		if (method == null)
		{
			return this;
		}
		return method.GetBaseDefinition();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return Resolver.GetCustomAttributeData(MetadataToken);
	}
}
