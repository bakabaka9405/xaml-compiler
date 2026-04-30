using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyTypeDef : MetadataOnlyCommonType
{
	private enum TriState
	{
		Yes,
		No,
		Maybe
	}

	internal class InterfaceImpl
	{
		private MetadataOnlyTypeDef m_owningType;

		private Token m_interfaceImplToken;

		public Type OwningType => m_owningType;

		public int MetadataToken => m_interfaceImplToken.Value;

		internal InterfaceImpl(MetadataOnlyTypeDef owningType, Token interfaceImplToken)
		{
			m_owningType = owningType;
			m_interfaceImplToken = interfaceImplToken;
		}

		public IList<CustomAttributeData> GetCustomAttributesData()
		{
			return m_owningType.Resolver.GetCustomAttributeData(MetadataToken);
		}

		public Type GetInterfaceType()
		{
			return m_owningType.Resolver.GetInterfaceTypeFromInterfaceImpl(m_owningType, m_interfaceImplToken);
		}
	}

	private readonly MetadataOnlyModule m_resolver;

	private readonly Token m_tokenTypeDef;

	private readonly Type[] m_typeParameters;

	private readonly Token m_tokenExtends;

	private string m_fullName;

	private int m_nameLength;

	private readonly TypeAttributes m_typeAttributes;

	private Type m_baseType;

	private TriState m_fIsValueType = TriState.Maybe;

	private static readonly string[] PrimitiveTypeNames = new string[14]
	{
		"System.Boolean", "System.Char", "System.SByte", "System.Byte", "System.Int16", "System.UInt16", "System.Int32", "System.UInt32", "System.Int64", "System.UInt64",
		"System.Single", "System.Double", "System.IntPtr", "System.UIntPtr"
	};

	private Type[] _interfacesCache;

	private string LocalFullName
	{
		get
		{
			if (string.IsNullOrEmpty(m_fullName))
			{
				m_resolver.GetTypeName(m_tokenTypeDef, m_nameLength, out m_fullName);
			}
			return m_fullName;
		}
	}

	internal override MetadataOnlyModule Resolver => m_resolver;

	public override int MetadataToken => m_tokenTypeDef.Value;

	public override string FullName
	{
		get
		{
			if ((!IsGenericType || IsGenericTypeDefinition) && DeclaringType == null)
			{
				return LocalFullName;
			}
			StringBuilder builder = StringBuilderPool.Get();
			GetSimpleName(builder);
			if (!IsGenericType || IsGenericTypeDefinition)
			{
				string result = builder.ToString();
				StringBuilderPool.Release(ref builder);
				return result;
			}
			builder.Append("[");
			Type[] genericArguments = GetGenericArguments();
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if (i > 0)
				{
					builder.Append(",");
				}
				builder.Append('[');
				if (genericArguments[i].FullName == null || genericArguments[i].IsGenericTypeDefinition)
				{
					return null;
				}
				builder.Append(genericArguments[i].AssemblyQualifiedName);
				builder.Append(']');
			}
			builder.Append("]");
			string result2 = builder.ToString();
			StringBuilderPool.Release(ref builder);
			return result2;
		}
	}

	public override string Namespace
	{
		get
		{
			if (DeclaringType != null)
			{
				return DeclaringType.Namespace;
			}
			return Utility.GetNamespaceHelper(LocalFullName);
		}
	}

	public override Type BaseType
	{
		get
		{
			if (m_baseType == null)
			{
				if (m_tokenExtends.IsNil)
				{
					return null;
				}
				m_baseType = m_resolver.ResolveTypeTokenInternal(m_tokenExtends, GenericContext);
			}
			return m_baseType;
		}
	}

	public override bool IsEnum
	{
		get
		{
			Type typeXFromName = m_resolver.AssemblyResolver.GetTypeXFromName("System.Enum");
			return typeXFromName.Equals(BaseType);
		}
	}

	public override Type UnderlyingSystemType => this;

	public override bool IsGenericType => m_typeParameters.Length != 0;

	public override bool IsGenericTypeDefinition
	{
		get
		{
			if (!IsGenericType)
			{
				return false;
			}
			Type[] genericArguments = GetGenericArguments();
			Type[] array = genericArguments;
			foreach (Type type in array)
			{
				if (!type.IsGenericParameter)
				{
					return false;
				}
				if (!type.DeclaringType.Equals(this))
				{
					return false;
				}
			}
			return true;
		}
	}

	public override Guid GUID
	{
		get
		{
			IList<CustomAttributeData> customAttributesData = GetCustomAttributesData();
			foreach (CustomAttributeData item in customAttributesData)
			{
				if (item.Constructor.DeclaringType.FullName.Equals("System.Runtime.InteropServices.GuidAttribute"))
				{
					string g = (string)item.ConstructorArguments[0].Value;
					return new Guid(g);
				}
			}
			return Guid.Empty;
		}
	}

	public override StructLayoutAttribute StructLayoutAttribute
	{
		get
		{
			if (base.IsInterface)
			{
				return null;
			}
			uint ulClassSize = 0u;
			uint dwPackSize;
			uint classLayout = m_resolver.RawImport.GetClassLayout(m_tokenTypeDef, out dwPackSize, UnusedIntPtr.Zero, 0u, UnusedIntPtr.Zero, ref ulClassSize);
			if (dwPackSize == 0)
			{
				dwPackSize = 8u;
			}
			LayoutKind layoutKind = (m_typeAttributes & TypeAttributes.LayoutMask) switch
			{
				TypeAttributes.SequentialLayout => LayoutKind.Sequential, 
				TypeAttributes.NotPublic => LayoutKind.Auto, 
				TypeAttributes.ExplicitLayout => LayoutKind.Explicit, 
				_ => throw new InvalidOperationException(Resources.IllegalLayoutMask), 
			};
			CharSet charSet = CharSet.None;
			switch (m_typeAttributes & TypeAttributes.StringFormatMask)
			{
			case TypeAttributes.NotPublic:
				charSet = CharSet.Ansi;
				break;
			case TypeAttributes.UnicodeClass:
				charSet = CharSet.Unicode;
				break;
			case TypeAttributes.AutoClass:
				charSet = CharSet.Auto;
				break;
			}
			StructLayoutAttribute structLayoutAttribute = new StructLayoutAttribute(layoutKind);
			structLayoutAttribute.Size = (int)ulClassSize;
			structLayoutAttribute.Pack = (int)dwPackSize;
			structLayoutAttribute.CharSet = charSet;
			return structLayoutAttribute;
		}
	}

	public override MemberTypes MemberType
	{
		get
		{
			if (base.IsNested)
			{
				return MemberTypes.NestedType;
			}
			return MemberTypes.TypeInfo;
		}
	}

	public override Type DeclaringType
	{
		get
		{
			Type enclosingType = m_resolver.GetEnclosingType(new Token(MetadataToken));
			if (enclosingType != null)
			{
				return enclosingType;
			}
			return null;
		}
	}

	public override string Name => Utility.GetTypeNameFromFullNameHelper(LocalFullName, base.IsNested);

	public override Assembly Assembly => m_resolver.Assembly;

	public override GenericParameterAttributes GenericParameterAttributes
	{
		get
		{
			throw new InvalidOperationException(Resources.ValidOnGenericParameterTypeOnly);
		}
	}

	public MetadataOnlyTypeDef(MetadataOnlyModule scope, Token tokenTypeDef)
		: this(scope, tokenTypeDef, null)
	{
	}

	public MetadataOnlyTypeDef(MetadataOnlyModule scope, Token tokenTypeDef, Type[] typeParameters)
	{
		ValidateConstructorArguments(scope, tokenTypeDef);
		m_resolver = scope;
		m_tokenTypeDef = tokenTypeDef;
		m_typeParameters = null;
		m_resolver.GetTypeAttributes(m_tokenTypeDef, out m_tokenExtends, out m_typeAttributes, out m_nameLength);
		if ((m_typeAttributes & TypeAttributes.Import) != TypeAttributes.NotPublic)
		{
			m_typeAttributes = (m_typeAttributes & ~TypeAttributes.Import) | TypeAttributes.Public;
		}
		int num = m_resolver.CountGenericParams(m_tokenTypeDef);
		bool flag = typeParameters != null && typeParameters.Length != 0;
		if (num > 0)
		{
			if (!flag)
			{
				m_typeParameters = new Type[num];
				int num2 = 0;
				{
					foreach (int genericParameterToken in m_resolver.GetGenericParameterTokens(m_tokenTypeDef))
					{
						m_typeParameters[num2++] = m_resolver.Factory.CreateTypeVariable(m_resolver, new Token(genericParameterToken));
					}
					return;
				}
			}
			if (num != typeParameters.Length)
			{
				throw new ArgumentException(Resources.WrongNumberOfGenericArguments);
			}
			m_typeParameters = typeParameters;
		}
		else
		{
			m_typeParameters = Type.EmptyTypes;
		}
	}

	private static void ValidateConstructorArguments(MetadataOnlyModule scope, Token tokenTypeDef)
	{
		if (scope == null)
		{
			throw new ArgumentNullException("scope");
		}
		if (!tokenTypeDef.IsType(TokenType.TypeDef))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.ExpectedTokenType, TokenType.TypeDef.ToString()));
		}
	}

	private void GetSimpleName(StringBuilder sb)
	{
		Type declaringType = DeclaringType;
		if (declaringType != null)
		{
			sb.Append(declaringType.FullName);
			sb.Append('+');
		}
		sb.Append(LocalFullName);
	}

	public override string ToString()
	{
		if (!IsGenericType)
		{
			return FullName;
		}
		StringBuilder builder = StringBuilderPool.Get();
		GetSimpleName(builder);
		builder.Append("[");
		Type[] genericArguments = GetGenericArguments();
		for (int i = 0; i < genericArguments.Length; i++)
		{
			if (i != 0)
			{
				builder.Append(",");
			}
			builder.Append(genericArguments[i].ToString());
		}
		builder.Append("]");
		string result = builder.ToString();
		StringBuilderPool.Release(ref builder);
		return result;
	}

	public override bool Equals(Type other)
	{
		if (other == null)
		{
			return false;
		}
		if (!Module.Equals(other.Module))
		{
			return false;
		}
		bool isGenericType = IsGenericType;
		bool isGenericType2 = other.IsGenericType;
		if (isGenericType != isGenericType2)
		{
			return false;
		}
		if (MetadataToken != other.MetadataToken)
		{
			return false;
		}
		if (!isGenericType && !isGenericType2)
		{
			return true;
		}
		Type[] genericArguments = GetGenericArguments();
		Type[] genericArguments2 = other.GetGenericArguments();
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

	public override Type MakeGenericType(params Type[] argTypes)
	{
		if (argTypes == null)
		{
			throw new ArgumentNullException("argTypes");
		}
		if (IsGenericTypeDefinition)
		{
			if (argTypes.Length == m_typeParameters.Length)
			{
				return Resolver.Factory.CreateGenericType(Resolver, m_tokenTypeDef, argTypes);
			}
			throw new ArgumentException(Resources.WrongNumberOfGenericArguments);
		}
		throw new InvalidOperationException();
	}

	public override bool IsAssignableFrom(Type c)
	{
		return IsAssignableFromHelper(this, c);
	}

	internal static bool IsAssignableFromHelper(Type current, Type target)
	{
		if (target == null)
		{
			return false;
		}
		if (current.Equals(target))
		{
			return true;
		}
		if (target.IsSubclassOf(current))
		{
			return true;
		}
		Type[] interfaces = target.GetInterfaces();
		for (int i = 0; i < interfaces.Length; i++)
		{
			if (interfaces[i].Equals(current))
			{
				return true;
			}
			if (current.IsAssignableFrom(interfaces[i]))
			{
				return true;
			}
		}
		if (target.IsGenericParameter)
		{
			Type[] genericParameterConstraints = target.GetGenericParameterConstraints();
			for (int j = 0; j < genericParameterConstraints.Length; j++)
			{
				if (IsAssignableFromHelper(current, genericParameterConstraints[j]))
				{
					return true;
				}
			}
		}
		ITypeUniverse typeUniverse = Helpers.Universe(current);
		if (typeUniverse != null && current.Equals(typeUniverse.GetTypeXFromName("System.Object")))
		{
			if (!target.IsPointer && !target.IsInterface)
			{
				return target.IsArray;
			}
			return true;
		}
		return false;
	}

	protected override bool IsValueTypeImpl()
	{
		if (m_fIsValueType == TriState.Maybe)
		{
			if (IsValueTypeHelper())
			{
				m_fIsValueType = TriState.Yes;
			}
			else
			{
				m_fIsValueType = TriState.No;
			}
		}
		if (m_fIsValueType == TriState.Yes)
		{
			return true;
		}
		_ = m_fIsValueType;
		_ = 1;
		return false;
	}

	private bool IsValueTypeHelper()
	{
		MetadataOnlyModule resolver = Resolver;
		Type typeXFromName = resolver.AssemblyResolver.GetTypeXFromName("System.Enum");
		if (Equals(typeXFromName))
		{
			return false;
		}
		Type typeXFromName2 = resolver.AssemblyResolver.GetTypeXFromName("System.ValueType");
		if (!typeXFromName2.Equals(BaseType))
		{
			return IsEnum;
		}
		return true;
	}

	protected override bool IsPrimitiveImpl()
	{
		if (!m_resolver.IsSystemModule())
		{
			return false;
		}
		string fullName = FullName;
		string[] primitiveTypeNames = PrimitiveTypeNames;
		foreach (string text in primitiveTypeNames)
		{
			if (text.Equals(fullName, StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	public override Type[] GetGenericArguments()
	{
		return (Type[])m_typeParameters.Clone();
	}

	public override Type GetGenericTypeDefinition()
	{
		if (!IsGenericType)
		{
			throw new InvalidOperationException();
		}
		if (IsGenericTypeDefinition)
		{
			return this;
		}
		return Resolver.Factory.CreateSimpleType(Resolver, m_tokenTypeDef);
	}

	public override Type GetElementType()
	{
		return null;
	}

	public override FieldInfo[] GetFields(BindingFlags flags)
	{
		return MetadataOnlyModule.GetFieldsOnType(this, flags);
	}

	public override FieldInfo GetField(string name, BindingFlags bindingAttr)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		StringComparison stringComparison = SignatureUtil.GetStringComparison(bindingAttr);
		FieldInfo[] fields = GetFields(bindingAttr);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (fieldInfo.Name.Equals(name, stringComparison))
			{
				return fieldInfo;
			}
		}
		return null;
	}

	internal static PropertyInfo GetPropertyImplHelper(MetadataOnlyCommonType type, string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
	{
		if (binder != null)
		{
			throw new NotSupportedException();
		}
		if (modifiers != null && modifiers.Length != 0)
		{
			throw new NotSupportedException();
		}
		StringComparison stringComparison = SignatureUtil.GetStringComparison(bindingAttr);
		PropertyInfo[] properties = type.GetProperties(bindingAttr);
		PropertyInfo[] array = properties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (propertyInfo.Name.Equals(name, stringComparison) && (!(returnType != null) || propertyInfo.PropertyType.Equals(returnType)) && PropertyParamTypesMatch(propertyInfo, types))
			{
				return propertyInfo;
			}
		}
		return null;
	}

	public override Type[] GetInterfaces()
	{
		if (_interfacesCache == null)
		{
			_interfacesCache = GetAllInterfacesHelper(this);
		}
		return _interfacesCache;
	}

	internal static Type[] GetAllInterfacesHelper(MetadataOnlyCommonType type)
	{
		HashSet<Type> hashSet = new HashSet<Type>();
		if (type.BaseType != null)
		{
			Type[] interfaces = type.BaseType.GetInterfaces();
			hashSet.UnionWith(interfaces);
		}
		IEnumerable<Type> interfacesOnType = type.Resolver.GetInterfacesOnType(type);
		foreach (Type item in interfacesOnType)
		{
			if (!hashSet.Contains(item))
			{
				hashSet.Add(item);
				Type[] interfaces2 = item.GetInterfaces();
				hashSet.UnionWith(interfaces2);
			}
		}
		Type[] array = new Type[hashSet.Count];
		hashSet.CopyTo(array);
		return array;
	}

	public override Type GetInterface(string name, bool ignoreCase)
	{
		return MetadataOnlyModule.GetInterfaceHelper(GetInterfaces(), name, ignoreCase);
	}

	public IEnumerable<InterfaceImpl> GetInterfaceImpls()
	{
		foreach (Token item in Resolver.EnumerateInterfaceImplsOnType(this))
		{
			yield return new InterfaceImpl(this, item);
		}
	}

	private static bool PropertyParamTypesMatch(PropertyInfo p, Type[] types)
	{
		if (types == null)
		{
			return true;
		}
		ParameterInfo[] indexParameters = p.GetIndexParameters();
		if (indexParameters.Length != types.Length)
		{
			return false;
		}
		int num = indexParameters.Length;
		for (int i = 0; i < num; i++)
		{
			if (!indexParameters[i].ParameterType.Equals(types[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override EventInfo[] GetEvents(BindingFlags flags)
	{
		return MetadataOnlyModule.GetEventsOnType(this, flags);
	}

	public override EventInfo GetEvent(string name, BindingFlags flags)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		StringComparison stringComparison = SignatureUtil.GetStringComparison(flags);
		EventInfo[] events = GetEvents(flags);
		EventInfo[] array = events;
		foreach (EventInfo eventInfo in array)
		{
			if (eventInfo.Name.Equals(name, stringComparison))
			{
				return eventInfo;
			}
		}
		return null;
	}

	public override Type GetNestedType(string name, BindingFlags bindingAttr)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		StringComparison stringComparison = SignatureUtil.GetStringComparison(bindingAttr);
		Type[] nestedTypes = GetNestedTypes(bindingAttr);
		Type[] array = nestedTypes;
		foreach (Type type in array)
		{
			if (type.Name.Equals(name, stringComparison))
			{
				return type;
			}
		}
		return null;
	}

	public override Type[] GetNestedTypes(BindingFlags bindingAttr)
	{
		List<Type> list = new List<Type>(m_resolver.GetNestedTypesOnType(this, bindingAttr));
		return list.ToArray();
	}

	public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
	{
		MemberInfo[] members = GetMembers(bindingAttr);
		List<MemberInfo> list = new List<MemberInfo>();
		StringComparison stringComparison = SignatureUtil.GetStringComparison(bindingAttr);
		MemberInfo[] array = members;
		foreach (MemberInfo memberInfo in array)
		{
			if (name.Equals(memberInfo.Name, stringComparison) && (type == memberInfo.MemberType || type == MemberTypes.All))
			{
				list.Add(memberInfo);
			}
		}
		return list.ToArray();
	}

	internal static MemberInfo[] GetMembersHelper(Type type, BindingFlags bindingAttr)
	{
		List<MemberInfo> list = new List<MemberInfo>(type.GetMethods(bindingAttr));
		list.AddRange(type.GetConstructors(bindingAttr));
		list.AddRange(type.GetFields(bindingAttr));
		list.AddRange(type.GetProperties(bindingAttr));
		list.AddRange(type.GetEvents(bindingAttr));
		list.AddRange(type.GetNestedTypes(bindingAttr));
		return list.ToArray();
	}

	protected override bool HasElementTypeImpl()
	{
		return false;
	}

	public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
	{
		throw new NotSupportedException();
	}

	protected override bool IsCOMObjectImpl()
	{
		throw new NotImplementedException();
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

	protected override TypeAttributes GetAttributeFlagsImpl()
	{
		return m_typeAttributes;
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return Resolver.GetCustomAttributeData(MetadataToken);
	}

	protected override TypeCode GetTypeCodeImpl()
	{
		return m_resolver.GetTypeCode(this);
	}

	internal override IEnumerable<PropertyInfo> GetDeclaredProperties()
	{
		return Resolver.GetPropertiesOnDeclaredTypeOnly(m_tokenTypeDef, GenericContext);
	}

	internal override IEnumerable<MethodBase> GetDeclaredMethods()
	{
		return Resolver.GetMethodBasesOnDeclaredTypeOnly(m_tokenTypeDef, GenericContext, MetadataOnlyModule.EMethodKind.Methods);
	}

	internal override IEnumerable<MethodBase> GetDeclaredConstructors()
	{
		return Resolver.GetMethodBasesOnDeclaredTypeOnly(m_tokenTypeDef, GenericContext, MetadataOnlyModule.EMethodKind.Constructor);
	}
}
