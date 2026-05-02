using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[DebuggerDisplay("TypeProxy")]
internal abstract class TypeProxy : MetadataOnlyCommonType, ITypeProxy
{
	protected readonly MetadataOnlyModule m_resolver;

	private Type m_cachedResolvedType;

	internal override MetadataOnlyModule Resolver => m_resolver;

	public ITypeUniverse TypeUniverse => m_resolver.AssemblyResolver;

	public override string FullName => GetResolvedType().FullName;

	public override string Namespace => GetResolvedType().Namespace;

	public override string Name => GetResolvedType().Name;

	public override string AssemblyQualifiedName => GetResolvedType().AssemblyQualifiedName;

	public override Module Module => GetResolvedType().Module;

	public override Type BaseType => GetResolvedType().BaseType;

	public override bool IsEnum => GetResolvedType().IsEnum;

	public override int MetadataToken => GetResolvedType().MetadataToken;

	public override bool ContainsGenericParameters => GetResolvedType().ContainsGenericParameters;

	public override Type UnderlyingSystemType => GetResolvedType().UnderlyingSystemType;

	public override bool IsGenericParameter => GetResolvedType().IsGenericParameter;

	public override bool IsGenericType => GetResolvedType().IsGenericType;

	public override bool IsGenericTypeDefinition => GetResolvedType().IsGenericTypeDefinition;

	public override Guid GUID => GetResolvedType().GUID;

	public override StructLayoutAttribute StructLayoutAttribute => GetResolvedType().StructLayoutAttribute;

	public override GenericParameterAttributes GenericParameterAttributes => GetResolvedType().GenericParameterAttributes;

	public override MethodBase DeclaringMethod => GetResolvedType().DeclaringMethod;

	public override int GenericParameterPosition => GetResolvedType().GenericParameterPosition;

	public override bool IsSerializable => GetResolvedType().IsSerializable;

	public override MemberTypes MemberType => GetResolvedType().MemberType;

	public override Type DeclaringType => GetResolvedType().DeclaringType;

	public override Assembly Assembly => GetResolvedType().Assembly;

	public override Type ReflectedType => GetResolvedType().ReflectedType;

	protected TypeProxy(MetadataOnlyModule resolver)
	{
		m_resolver = resolver;
	}

	public virtual Type GetResolvedType()
	{
		if (m_cachedResolvedType == null)
		{
			m_cachedResolvedType = GetResolvedTypeWorker();
		}
		return m_cachedResolvedType;
	}

	protected abstract Type GetResolvedTypeWorker();

	public override string ToString()
	{
		return GetResolvedType().ToString();
	}

	public override int GetHashCode()
	{
		return GetResolvedType().GetHashCode();
	}

	public override bool Equals(object objOther)
	{
		Type type = objOther as Type;
		if (type == null)
		{
			return false;
		}
		return Equals(type);
	}

	public override bool Equals(Type t)
	{
		if (t == null)
		{
			return false;
		}
		return GetResolvedType().Equals(t);
	}

	public override Type MakeByRefType()
	{
		return Resolver.Factory.CreateByRefType(this);
	}

	public override Type MakePointerType()
	{
		return Resolver.Factory.CreatePointerType(this);
	}

	public override int GetArrayRank()
	{
		return GetResolvedType().GetArrayRank();
	}

	public override Type MakeGenericType(params Type[] args)
	{
		return new ProxyGenericType(this, args);
	}

	public override Type MakeArrayType()
	{
		return Resolver.Factory.CreateVectorType(this);
	}

	public override Type MakeArrayType(int rank)
	{
		return Resolver.Factory.CreateArrayType(this, rank);
	}

	protected override bool IsArrayImpl()
	{
		return GetResolvedType().IsArray;
	}

	protected override bool IsByRefImpl()
	{
		return GetResolvedType().IsByRef;
	}

	protected override bool IsPointerImpl()
	{
		return GetResolvedType().IsPointer;
	}

	protected override bool IsValueTypeImpl()
	{
		return GetResolvedType().IsValueType;
	}

	protected override bool IsPrimitiveImpl()
	{
		return GetResolvedType().IsPrimitive;
	}

	public override Type GetElementType()
	{
		return GetResolvedType().GetElementType();
	}

	public override Type[] GetGenericArguments()
	{
		return GetResolvedType().GetGenericArguments();
	}

	public override Type GetGenericTypeDefinition()
	{
		return GetResolvedType().GetGenericTypeDefinition();
	}

	public override MethodInfo[] GetMethods(BindingFlags flags)
	{
		return GetResolvedType().GetMethods(flags);
	}

	public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
	{
		return GetResolvedType().GetConstructors(bindingAttr);
	}

	public override bool IsAssignableFrom(Type c)
	{
		return GetResolvedType().IsAssignableFrom(c);
	}

	protected override bool IsContextfulImpl()
	{
		return GetResolvedType().IsContextful;
	}

	protected override bool IsMarshalByRefImpl()
	{
		return GetResolvedType().IsMarshalByRef;
	}

	public override bool IsSubclassOf(Type c)
	{
		return GetResolvedType().IsSubclassOf(c);
	}

	protected override TypeAttributes GetAttributeFlagsImpl()
	{
		return GetResolvedType().Attributes;
	}

	protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		if (types == null && modifiers == null && binder == null && callConvention == CallingConventions.Any)
		{
			return GetResolvedType().GetMethod(name, bindingAttr);
		}
		return GetResolvedType().GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
	}

	protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		if (types == null && modifiers == null)
		{
			if (returnType != null)
			{
				return GetResolvedType().GetProperty(name, returnType);
			}
			return GetResolvedType().GetProperty(name, bindingAttr);
		}
		return GetResolvedType().GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
	}

	protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		return GetResolvedType().GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
	}

	public override FieldInfo[] GetFields(BindingFlags flags)
	{
		return GetResolvedType().GetFields(flags);
	}

	public override PropertyInfo[] GetProperties(BindingFlags flags)
	{
		return GetResolvedType().GetProperties(flags);
	}

	public override EventInfo[] GetEvents(BindingFlags flags)
	{
		return GetResolvedType().GetEvents(flags);
	}

	public override EventInfo GetEvent(string name, BindingFlags flags)
	{
		return GetResolvedType().GetEvent(name, flags);
	}

	public override FieldInfo GetField(string name, BindingFlags flags)
	{
		return GetResolvedType().GetField(name, flags);
	}

	public override Type[] GetNestedTypes(BindingFlags bindingAttr)
	{
		return GetResolvedType().GetNestedTypes(bindingAttr);
	}

	public override Type GetNestedType(string name, BindingFlags bindingAttr)
	{
		return GetResolvedType().GetNestedType(name, bindingAttr);
	}

	public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
	{
		return GetResolvedType().GetMember(name, type, bindingAttr);
	}

	public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
	{
		return GetResolvedType().GetMembers(bindingAttr);
	}

	public override bool IsInstanceOfType(object o)
	{
		return GetResolvedType().IsInstanceOfType(o);
	}

	public override Type[] GetInterfaces()
	{
		return GetResolvedType().GetInterfaces();
	}

	public override Type GetInterface(string name, bool ignoreCase)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return GetResolvedType().GetInterface(name, ignoreCase);
	}

	protected override bool HasElementTypeImpl()
	{
		if (!base.IsArray && !base.IsByRef)
		{
			return base.IsPointer;
		}
		return true;
	}

	public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
	{
		return GetResolvedType().InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
	}

	protected override bool IsCOMObjectImpl()
	{
		return GetResolvedType().IsCOMObject;
	}

	public override MemberInfo[] GetDefaultMembers()
	{
		return GetResolvedType().GetDefaultMembers();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return GetResolvedType().GetCustomAttributesData();
	}

	public override Type[] GetGenericParameterConstraints()
	{
		return GetResolvedType().GetGenericParameterConstraints();
	}

	public override InterfaceMapping GetInterfaceMap(Type interfaceType)
	{
		return GetResolvedType().GetInterfaceMap(interfaceType);
	}

	public override Type[] FindInterfaces(TypeFilter filter, object filterCriteria)
	{
		return GetResolvedType().FindInterfaces(filter, filterCriteria);
	}

	public override MemberInfo[] GetMember(string name, BindingFlags bindingAttr)
	{
		return GetResolvedType().GetMember(name, bindingAttr);
	}

	public override MemberInfo[] FindMembers(MemberTypes memberType, BindingFlags bindingAttr, MemberFilter filter, object filterCriteria)
	{
		return GetResolvedType().FindMembers(memberType, bindingAttr, filter, filterCriteria);
	}

	public override object[] GetCustomAttributes(bool inherit)
	{
		return GetResolvedType().GetCustomAttributes(inherit);
	}

	public override object[] GetCustomAttributes(Type attributeType, bool inherit)
	{
		return GetResolvedType().GetCustomAttributes(attributeType, inherit);
	}

	public override bool IsDefined(Type attributeType, bool inherit)
	{
		return GetResolvedType().IsDefined(attributeType, inherit);
	}

	protected override TypeCode GetTypeCodeImpl()
	{
		return Type.GetTypeCode(GetResolvedType());
	}
}
