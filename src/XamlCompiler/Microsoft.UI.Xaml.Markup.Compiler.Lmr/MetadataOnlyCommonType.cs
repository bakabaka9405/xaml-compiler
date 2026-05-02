using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

[DebuggerDisplay("\\{Name = {Name} FullName = {FullName}\\}")]
internal abstract class MetadataOnlyCommonType : Type
{
	private Dictionary<BindingFlags, MemberInfo[]> _membersCache = new Dictionary<BindingFlags, MemberInfo[]>();

	internal abstract MetadataOnlyModule Resolver { get; }

	internal virtual GenericContext GenericContext => new GenericContext(GetGenericArguments(), null);

	public override Module Module => Resolver;

	public override bool ContainsGenericParameters
	{
		get
		{
			if (base.HasElementType)
			{
				return GetRootElementType().ContainsGenericParameters;
			}
			if (IsGenericParameter)
			{
				return true;
			}
			if (!IsGenericType)
			{
				return false;
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

	public override string AssemblyQualifiedName
	{
		get
		{
			string fullName = FullName;
			if (fullName == null)
			{
				return null;
			}
			Assembly assembly = Assembly;
			string assemblyName = assembly.GetName().ToString();
			return Assembly.CreateQualifiedName(assemblyName, fullName);
		}
	}

	public override bool IsSerializable
	{
		get
		{
			if ((GetAttributeFlagsImpl() & TypeAttributes.Serializable) == 0)
			{
				return QuickSerializationCastCheck();
			}
			return true;
		}
	}

	public override Type ReflectedType
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override bool IsEnum => false;

	public override bool IsGenericType => false;

	public override bool IsGenericParameter => false;

	public override MethodBase DeclaringMethod
	{
		get
		{
			throw new InvalidOperationException(Resources.ValidOnGenericParameterTypeOnly);
		}
	}

	public override StructLayoutAttribute StructLayoutAttribute => null;

	internal virtual IEnumerable<MethodBase> GetDeclaredMethods()
	{
		return new MethodInfo[0];
	}

	internal virtual IEnumerable<MethodBase> GetDeclaredConstructors()
	{
		return new MethodInfo[0];
	}

	internal virtual IEnumerable<PropertyInfo> GetDeclaredProperties()
	{
		return new PropertyInfo[0];
	}

	public override PropertyInfo[] GetProperties(BindingFlags flags)
	{
		return MetadataOnlyModule.GetPropertiesOnType(this, flags);
	}

	protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		return MetadataOnlyTypeDef.GetPropertyImplHelper(this, name, bindingAttr, binder, returnType, types, modifiers);
	}

	public override MethodInfo[] GetMethods(BindingFlags flags)
	{
		return MetadataOnlyModule.GetMethodsOnType(this, flags);
	}

	protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		return MetadataOnlyModule.GetMethodImplHelper(this, name, bindingAttr, binder, callConvention, types, modifiers);
	}

	public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
	{
		MemberInfo[] value = null;
		if (!_membersCache.TryGetValue(bindingAttr, out value))
		{
			value = MetadataOnlyTypeDef.GetMembersHelper(this, bindingAttr);
			_membersCache.Add(bindingAttr, value);
		}
		return value;
	}

	protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		return MetadataOnlyModule.GetConstructorOnType(this, bindingAttr, binder, callConvention, types, modifiers);
	}

	public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
	{
		return MetadataOnlyModule.GetConstructorsOnType(this, bindingAttr);
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

	public override int GetHashCode()
	{
		return MetadataToken;
	}

	private Type GetRootElementType()
	{
		Type type = this;
		while (type.HasElementType)
		{
			type = type.GetElementType();
		}
		return type;
	}

	public override bool IsSubclassOf(Type c)
	{
		Type type = this;
		if (type.Equals(c))
		{
			return false;
		}
		while (type != null)
		{
			if (type.Equals(c))
			{
				return true;
			}
			type = type.BaseType;
		}
		return false;
	}

	protected override bool IsContextfulImpl()
	{
		Type typeXFromName = Resolver.AssemblyResolver.GetTypeXFromName("System.ContextBoundObject");
		if (typeXFromName != null)
		{
			return typeXFromName.IsAssignableFrom(this);
		}
		return false;
	}

	protected override bool IsMarshalByRefImpl()
	{
		Type typeXFromName = Resolver.AssemblyResolver.GetTypeXFromName("System.MarshalByRefObject");
		if (typeXFromName != null)
		{
			return typeXFromName.IsAssignableFrom(this);
		}
		return false;
	}

	public override MemberInfo[] GetDefaultMembers()
	{
		Type typeXFromName = Resolver.AssemblyResolver.GetTypeXFromName("System.Reflection.DefaultMemberAttribute");
		if (typeXFromName == null)
		{
			return new MemberInfo[0];
		}
		CustomAttributeData customAttributeData = null;
		Type type = this;
		while (type != null)
		{
			IList<CustomAttributeData> customAttributesData = type.GetCustomAttributesData();
			for (int i = 0; i < customAttributesData.Count; i++)
			{
				if (customAttributesData[i].Constructor.DeclaringType.Equals(typeXFromName))
				{
					customAttributeData = customAttributesData[i];
					break;
				}
			}
			if (customAttributeData != null)
			{
				break;
			}
			type = type.BaseType;
		}
		if (customAttributeData == null)
		{
			return new MemberInfo[0];
		}
		string name = customAttributeData.ConstructorArguments[0].Value as string;
		MemberInfo[] array = GetMember(name);
		if (array == null)
		{
			array = new MemberInfo[0];
		}
		return array;
	}

	public override bool IsInstanceOfType(object o)
	{
		return false;
	}

	private bool QuickSerializationCastCheck()
	{
		ITypeUniverse typeUniverse = Helpers.Universe(this);
		Type typeXFromName = typeUniverse.GetTypeXFromName("System.Enum");
		Type typeXFromName2 = typeUniverse.GetTypeXFromName("System.Delegate");
		Type type = UnderlyingSystemType;
		while (type != null)
		{
			if (type.Equals(typeXFromName) || type.Equals(typeXFromName2))
			{
				return true;
			}
			type = type.BaseType;
		}
		return false;
	}

	protected override bool IsArrayImpl()
	{
		return false;
	}

	protected override bool IsByRefImpl()
	{
		return false;
	}

	protected override bool IsPointerImpl()
	{
		return false;
	}

	protected override bool IsPrimitiveImpl()
	{
		return false;
	}

	protected override bool IsCOMObjectImpl()
	{
		throw new NotImplementedException();
	}

	public override int GetArrayRank()
	{
		throw new ArgumentException(Resources.OperationValidOnArrayTypeOnly);
	}

	public override Type MakeGenericType(params Type[] argTypes)
	{
		throw new InvalidOperationException();
	}

	public override Type MakeByRefType()
	{
		return Resolver.Factory.CreateByRefType(this);
	}

	public override Type MakePointerType()
	{
		return Resolver.Factory.CreatePointerType(this);
	}

	public override Type MakeArrayType()
	{
		return Resolver.Factory.CreateVectorType(this);
	}

	public override Type MakeArrayType(int rank)
	{
		return MakeArrayTypeHelper(rank);
	}

	private Type MakeArrayTypeHelper(int rank)
	{
		if (rank <= 0)
		{
			throw new IndexOutOfRangeException();
		}
		return Resolver.Factory.CreateArrayType(this, rank);
	}

	internal static string TypeSigToString(Type pThis)
	{
		StringBuilder builder = StringBuilderPool.Get();
		TypeSigToString(pThis, builder);
		string result = builder.ToString();
		StringBuilderPool.Release(ref builder);
		return result;
	}

	internal static void TypeSigToString(Type pThis, StringBuilder sb)
	{
		Type type = pThis;
		while (type.HasElementType)
		{
			type = type.GetElementType();
		}
		if (type.IsNested)
		{
			sb.Append(pThis.Name);
			return;
		}
		string text = pThis.ToString();
		if (type.IsPrimitive || type.FullName == "System.Void")
		{
			text = text.Substring("System.".Length);
		}
		sb.Append(text);
	}
}
