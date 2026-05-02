using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyTypeVariableRef : MetadataOnlyCommonType
{
	private readonly MetadataOnlyModule m_resolver;

	private readonly Token m_ownerToken;

	private readonly int m_position;

	private bool IsMethodVar
	{
		get
		{
			if (!m_ownerToken.IsType(TokenType.MemberRef))
			{
				return m_ownerToken.IsType(TokenType.MethodDef);
			}
			return true;
		}
	}

	public override string FullName => null;

	internal override MetadataOnlyModule Resolver => m_resolver;

	public override Type BaseType
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public override Type UnderlyingSystemType
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public override int MetadataToken
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public override bool IsGenericParameter => true;

	public override Guid GUID
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public override GenericParameterAttributes GenericParameterAttributes
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public override int GenericParameterPosition => m_position;

	public override MemberTypes MemberType => MemberTypes.TypeInfo;

	public override Type DeclaringType
	{
		get
		{
			if (!IsMethodVar)
			{
				if (m_ownerToken.IsType(TokenType.TypeDef))
				{
					return m_resolver.Factory.CreateSimpleType(m_resolver, m_ownerToken);
				}
				return m_resolver.Factory.CreateTypeRef(m_resolver, m_ownerToken);
			}
			return null;
		}
	}

	public override MethodBase DeclaringMethod
	{
		get
		{
			if (IsMethodVar)
			{
				return m_resolver.Factory.CreateMethodOrConstructor(m_resolver, m_ownerToken, null, null);
			}
			return null;
		}
	}

	public override string Name => null;

	public override string Namespace => null;

	public override Assembly Assembly => m_resolver.Assembly;

	public override Type ReflectedType
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	internal MetadataOnlyTypeVariableRef(MetadataOnlyModule resolver, Token ownerToken, int position)
	{
		m_resolver = resolver;
		m_ownerToken = ownerToken;
		m_position = position;
	}

	public override bool Equals(Type other)
	{
		MetadataOnlyTypeVariableRef metadataOnlyTypeVariableRef = other as MetadataOnlyTypeVariableRef;
		if (metadataOnlyTypeVariableRef != null)
		{
			if (Resolver.Equals(metadataOnlyTypeVariableRef.Resolver) && m_ownerToken.Value == metadataOnlyTypeVariableRef.m_ownerToken.Value)
			{
				return m_position == metadataOnlyTypeVariableRef.m_position;
			}
			return false;
		}
		if (other.IsGenericParameter)
		{
			bool flag = IsMethodVar == (other.DeclaringMethod != null);
			return m_position == other.GenericParameterPosition && flag;
		}
		return false;
	}

	public override bool IsAssignableFrom(Type c)
	{
		throw new InvalidOperationException();
	}

	public override Type GetElementType()
	{
		throw new InvalidOperationException();
	}

	public override MethodInfo[] GetMethods(BindingFlags flags)
	{
		throw new InvalidOperationException();
	}

	public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
	{
		throw new InvalidOperationException();
	}

	public override FieldInfo[] GetFields(BindingFlags flags)
	{
		throw new InvalidOperationException();
	}

	public override FieldInfo GetField(string name, BindingFlags flags)
	{
		throw new InvalidOperationException();
	}

	public override PropertyInfo[] GetProperties(BindingFlags flags)
	{
		throw new InvalidOperationException();
	}

	public override EventInfo[] GetEvents(BindingFlags flags)
	{
		throw new InvalidOperationException();
	}

	public override EventInfo GetEvent(string name, BindingFlags flags)
	{
		throw new InvalidOperationException();
	}

	public override Type MakeGenericType(params Type[] argTypes)
	{
		throw new InvalidOperationException();
	}

	public override Type GetNestedType(string name, BindingFlags bindingAttr)
	{
		throw new InvalidOperationException();
	}

	public override Type[] GetNestedTypes(BindingFlags bindingAttr)
	{
		throw new InvalidOperationException();
	}

	protected override TypeAttributes GetAttributeFlagsImpl()
	{
		throw new InvalidOperationException();
	}

	protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		throw new InvalidOperationException();
	}

	protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		throw new InvalidOperationException();
	}

	public override Type[] GetGenericArguments()
	{
		throw new InvalidOperationException();
	}

	public override Type[] GetGenericParameterConstraints()
	{
		throw new InvalidOperationException();
	}

	public override Type GetGenericTypeDefinition()
	{
		throw new InvalidOperationException();
	}

	protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		throw new InvalidOperationException();
	}

	public override Type[] GetInterfaces()
	{
		throw new InvalidOperationException();
	}

	public override Type GetInterface(string name, bool ignoreCase)
	{
		throw new InvalidOperationException();
	}

	public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
	{
		throw new InvalidOperationException();
	}

	protected override bool HasElementTypeImpl()
	{
		throw new InvalidOperationException();
	}

	public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
	{
		throw new NotSupportedException();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		throw new InvalidOperationException();
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
		if (IsMethodVar)
		{
			return "MVar!!" + GenericParameterPosition.ToString(CultureInfo.InvariantCulture);
		}
		return "Var!" + GenericParameterPosition.ToString(CultureInfo.InvariantCulture);
	}

	protected override TypeCode GetTypeCodeImpl()
	{
		throw new InvalidOperationException();
	}
}
