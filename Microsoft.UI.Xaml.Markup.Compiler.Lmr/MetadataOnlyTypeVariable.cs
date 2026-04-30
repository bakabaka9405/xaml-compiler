using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyTypeVariable : MetadataOnlyCommonType
{
	private readonly int m_ownerMethodToken;

	private readonly int m_ownerTypeToken;

	private readonly string m_name;

	private readonly uint m_position;

	private readonly MetadataOnlyModule m_resolver;

	private readonly int m_Token;

	private readonly GenericParameterAttributes m_gpAttributes;

	public override string FullName => null;

	internal override MetadataOnlyModule Resolver => m_resolver;

	public override Type BaseType
	{
		get
		{
			Type[] genericParameterConstraints = GetGenericParameterConstraints();
			Type[] array = genericParameterConstraints;
			foreach (Type type in array)
			{
				if (type.IsClass)
				{
					return type;
				}
			}
			return m_resolver.AssemblyResolver.GetBuiltInType(CorElementType.Object);
		}
	}

	public override Type UnderlyingSystemType => this;

	public override int MetadataToken => m_Token;

	public override bool IsGenericParameter => true;

	public override Guid GUID => Guid.Empty;

	public override GenericParameterAttributes GenericParameterAttributes => m_gpAttributes;

	public override int GenericParameterPosition => (int)m_position;

	public override MemberTypes MemberType => MemberTypes.TypeInfo;

	public override Type DeclaringType
	{
		get
		{
			if (m_ownerTypeToken != 0)
			{
				return m_resolver.ResolveType(m_ownerTypeToken);
			}
			if (DeclaringMethod != null)
			{
				return DeclaringMethod.DeclaringType;
			}
			return null;
		}
	}

	public override MethodBase DeclaringMethod
	{
		get
		{
			if (m_ownerMethodToken != 0)
			{
				return m_resolver.ResolveMethod(m_ownerMethodToken);
			}
			return null;
		}
	}

	public override string Name => m_name;

	public override string Namespace
	{
		get
		{
			if (DeclaringType != null)
			{
				return DeclaringType.Namespace;
			}
			if (DeclaringMethod != null)
			{
				return DeclaringMethod.DeclaringType.Namespace;
			}
			return null;
		}
	}

	public override Assembly Assembly => m_resolver.Assembly;

	public override Type ReflectedType
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	internal MetadataOnlyTypeVariable(MetadataOnlyModule resolver, Token token)
	{
		m_Token = token.Value;
		m_resolver = resolver;
		m_resolver.GetGenericParameterProps(m_Token, out m_ownerTypeToken, out m_ownerMethodToken, out m_name, out m_gpAttributes, out m_position);
	}

	public override bool Equals(Type txOther)
	{
		if (txOther is MetadataOnlyTypeVariableRef)
		{
			if (m_ownerMethodToken != 0)
			{
				return m_position == txOther.GenericParameterPosition;
			}
			return false;
		}
		MetadataOnlyTypeVariable metadataOnlyTypeVariable = txOther as MetadataOnlyTypeVariable;
		if (metadataOnlyTypeVariable == null)
		{
			return false;
		}
		if (Name != metadataOnlyTypeVariable.Name)
		{
			return false;
		}
		if (m_ownerTypeToken == metadataOnlyTypeVariable.m_ownerTypeToken && m_ownerMethodToken == metadataOnlyTypeVariable.m_ownerMethodToken)
		{
			return Module.Equals(metadataOnlyTypeVariable.Module);
		}
		return false;
	}

	public override bool IsAssignableFrom(Type c)
	{
		return MetadataOnlyTypeDef.IsAssignableFromHelper(this, c);
	}

	public override Type GetElementType()
	{
		return null;
	}

	public override MethodInfo[] GetMethods(BindingFlags flags)
	{
		return new MethodInfo[0];
	}

	public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
	{
		return new ConstructorInfo[0];
	}

	public override FieldInfo[] GetFields(BindingFlags flags)
	{
		return new FieldInfo[0];
	}

	public override FieldInfo GetField(string name, BindingFlags flags)
	{
		return null;
	}

	public override PropertyInfo[] GetProperties(BindingFlags flags)
	{
		return new PropertyInfo[0];
	}

	public override EventInfo[] GetEvents(BindingFlags flags)
	{
		return new EventInfo[0];
	}

	public override EventInfo GetEvent(string name, BindingFlags flags)
	{
		return null;
	}

	public override Type MakeGenericType(params Type[] argTypes)
	{
		throw new InvalidOperationException();
	}

	public override Type GetNestedType(string name, BindingFlags bindingAttr)
	{
		return null;
	}

	public override Type[] GetNestedTypes(BindingFlags bindingAttr)
	{
		return Type.EmptyTypes;
	}

	protected override TypeAttributes GetAttributeFlagsImpl()
	{
		return TypeAttributes.Public;
	}

	protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		return null;
	}

	protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		return null;
	}

	public override Type[] GetGenericArguments()
	{
		return Type.EmptyTypes;
	}

	public override Type[] GetGenericParameterConstraints()
	{
		List<Type> list = new List<Type>(m_resolver.GetConstraintTypes(m_Token));
		return list.ToArray();
	}

	public override Type GetGenericTypeDefinition()
	{
		throw new InvalidOperationException();
	}

	protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		return null;
	}

	public override Type[] GetInterfaces()
	{
		return MetadataOnlyTypeDef.GetAllInterfacesHelper(this);
	}

	public override Type GetInterface(string name, bool ignoreCase)
	{
		return MetadataOnlyModule.GetInterfaceHelper(GetInterfaces(), name, ignoreCase);
	}

	public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
	{
		return MetadataOnlyTypeDef.GetMembersHelper(this, bindingAttr);
	}

	protected override bool HasElementTypeImpl()
	{
		return false;
	}

	public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
	{
		throw new NotSupportedException();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return Resolver.GetCustomAttributeData(MetadataToken);
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

	public override string ToString()
	{
		return Name;
	}

	protected override TypeCode GetTypeCodeImpl()
	{
		return TypeCode.Object;
	}
}
