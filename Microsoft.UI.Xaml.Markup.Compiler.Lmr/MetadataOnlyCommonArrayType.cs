using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyCommonArrayType : MetadataOnlyCommonType
{
	private readonly MetadataOnlyCommonType m_elementType;

	private readonly Type m_baseType;

	public override string Namespace => m_elementType.Namespace;

	internal override MetadataOnlyModule Resolver => m_elementType.Resolver;

	public override Type BaseType => m_baseType;

	public override Type UnderlyingSystemType => this;

	public override int MetadataToken => 33554432;

	public override MemberTypes MemberType => MemberTypes.TypeInfo;

	public override Type DeclaringType => null;

	public override Assembly Assembly => m_elementType.Assembly;

	public override Guid GUID => Guid.Empty;

	public override GenericParameterAttributes GenericParameterAttributes
	{
		get
		{
			throw new InvalidOperationException(Resources.ValidOnGenericParameterTypeOnly);
		}
	}

	public override string FullName
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public override string Name
	{
		get
		{
			throw new InvalidOperationException();
		}
	}

	public MetadataOnlyCommonArrayType(MetadataOnlyCommonType elementType)
	{
		ITypeUniverse typeUniverse = Helpers.Universe(elementType);
		m_baseType = typeUniverse.GetTypeXFromName("System.Array");
		m_elementType = elementType;
	}

	protected override bool IsArrayImpl()
	{
		return true;
	}

	public override Type GetElementType()
	{
		return m_elementType;
	}

	public override int GetHashCode()
	{
		return m_elementType.GetHashCode();
	}

	public override Type[] GetGenericArguments()
	{
		return m_elementType.GetGenericArguments();
	}

	public override Type GetGenericTypeDefinition()
	{
		throw new InvalidOperationException();
	}

	internal override IEnumerable<MethodBase> GetDeclaredMethods()
	{
		return Resolver.Policy.GetExtraArrayMethods(this);
	}

	internal override IEnumerable<MethodBase> GetDeclaredConstructors()
	{
		return Resolver.Policy.GetExtraArrayConstructors(this);
	}

	public override FieldInfo[] GetFields(BindingFlags flags)
	{
		return new FieldInfo[0];
	}

	public override FieldInfo GetField(string name, BindingFlags bindingAttr)
	{
		return null;
	}

	public override EventInfo[] GetEvents(BindingFlags flags)
	{
		return new EventInfo[0];
	}

	public override EventInfo GetEvent(string name, BindingFlags flags)
	{
		return null;
	}

	protected override TypeAttributes GetAttributeFlagsImpl()
	{
		TypeAttributes typeAttributes = TypeAttributes.Public | TypeAttributes.Sealed;
		return typeAttributes | TypeAttributes.Serializable;
	}

	public override Type GetNestedType(string name, BindingFlags bindingAttr)
	{
		return null;
	}

	public override Type[] GetNestedTypes(BindingFlags bindingAttr)
	{
		return new Type[0];
	}

	public override Type[] GetInterfaces()
	{
		List<Type> list = new List<Type>(m_baseType.GetInterfaces());
		list.AddRange(Resolver.Policy.GetExtraArrayInterfaces(m_elementType));
		return list.ToArray();
	}

	public override Type GetInterface(string name, bool ignoreCase)
	{
		return MetadataOnlyModule.GetInterfaceHelper(GetInterfaces(), name, ignoreCase);
	}

	public override Type MakeGenericType(params Type[] argTypes)
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
		CustomAttributeData serializableAttribute = PseudoCustomAttributes.GetSerializableAttribute(Resolver, isRequired: false);
		if (serializableAttribute != null)
		{
			return new CustomAttributeData[1] { serializableAttribute };
		}
		return new CustomAttributeData[0];
	}

	protected override TypeCode GetTypeCodeImpl()
	{
		return TypeCode.Object;
	}

	public override bool IsAssignableFrom(Type c)
	{
		throw new InvalidOperationException();
	}

	public override bool Equals(Type o)
	{
		throw new InvalidOperationException();
	}
}
