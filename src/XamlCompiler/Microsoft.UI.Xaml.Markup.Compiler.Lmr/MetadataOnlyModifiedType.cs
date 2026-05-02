using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyModifiedType : MetadataOnlyCommonType
{
	private readonly MetadataOnlyCommonType m_type;

	private readonly string m_mod;

	public override string FullName
	{
		get
		{
			string fullName = m_type.FullName;
			if (fullName == null || m_type.IsGenericTypeDefinition)
			{
				return null;
			}
			return fullName + m_mod;
		}
	}

	internal override MetadataOnlyModule Resolver => m_type.Resolver;

	public override Type BaseType => null;

	public override Type UnderlyingSystemType => this;

	public override Guid GUID => Guid.Empty;

	public override int MetadataToken => 33554432;

	public override GenericParameterAttributes GenericParameterAttributes
	{
		get
		{
			throw new InvalidOperationException(Resources.ValidOnGenericParameterTypeOnly);
		}
	}

	public override MemberTypes MemberType => MemberTypes.TypeInfo;

	public override Type DeclaringType => null;

	public override string Name => m_type.Name + m_mod;

	public override Assembly Assembly => m_type.Assembly;

	public override Type ReflectedType
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override string Namespace => m_type.Namespace;

	public MetadataOnlyModifiedType(MetadataOnlyCommonType type, string mod)
	{
		m_type = type;
		m_mod = mod;
	}

	public override bool Equals(Type t)
	{
		if (t == null)
		{
			return false;
		}
		if (base.IsByRef)
		{
			if (!t.IsByRef)
			{
				return false;
			}
		}
		else if (base.IsPointer && !t.IsPointer)
		{
			return false;
		}
		Type elementType = t.GetElementType();
		return m_type.Equals(elementType);
	}

	protected override bool IsByRefImpl()
	{
		return m_mod.Equals("&");
	}

	protected override bool IsPointerImpl()
	{
		return m_mod.Equals("*");
	}

	public override bool IsAssignableFrom(Type c)
	{
		if (c == null)
		{
			return false;
		}
		if ((base.IsPointer && c.IsPointer) || (base.IsByRef && c.IsByRef))
		{
			Type elementType = c.GetElementType();
			if (m_type.IsAssignableFrom(elementType) && !elementType.IsValueType)
			{
				return true;
			}
		}
		return MetadataOnlyTypeDef.IsAssignableFromHelper(this, c);
	}

	public override Type GetElementType()
	{
		return m_type;
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

	public override FieldInfo GetField(string name, BindingFlags bindingAttr)
	{
		return null;
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
		return TypeAttributes.NotPublic;
	}

	protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		return null;
	}

	protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		return null;
	}

	protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		return null;
	}

	public override Type[] GetInterfaces()
	{
		return new Type[0];
	}

	public override Type GetInterface(string name, bool ignoreCase)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return null;
	}

	public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
	{
		return MetadataOnlyTypeDef.GetMembersHelper(this, bindingAttr);
	}

	protected override bool HasElementTypeImpl()
	{
		return true;
	}

	public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
	{
		throw new NotSupportedException();
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return new CustomAttributeData[0];
	}

	public override string ToString()
	{
		return m_type.ToString() + m_mod;
	}

	public override Type[] GetGenericArguments()
	{
		return m_type.GetGenericArguments();
	}

	public override Type GetGenericTypeDefinition()
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

	protected override TypeCode GetTypeCodeImpl()
	{
		return TypeCode.Object;
	}
}
