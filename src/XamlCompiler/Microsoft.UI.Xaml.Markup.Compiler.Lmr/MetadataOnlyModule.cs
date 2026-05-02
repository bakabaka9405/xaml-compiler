using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Adds;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal class MetadataOnlyModule : Module, IModule2, IDisposable
{
	internal enum EMethodKind
	{
		Constructor,
		Methods
	}

	internal class NestedTypeCache
	{
		private readonly Dictionary<int, List<int>> m_cache;

		public NestedTypeCache(MetadataOnlyModule outer)
		{
			m_cache = new Dictionary<int, List<int>>();
			IEnumerable<int> typeTokenList = outer.GetTypeTokenList();
			foreach (int item in typeTokenList)
			{
				int num = outer.GetNestedClassProps(new Token(item));
				if (num != 0)
				{
					if (m_cache.ContainsKey(num))
					{
						m_cache[num].Add(item);
						continue;
					}
					List<int> value = new List<int> { item };
					m_cache.Add(num, value);
				}
			}
		}

		public IEnumerable<int> GetNestedTokens(Token tokenTypeDef)
		{
			if (m_cache.TryGetValue(tokenTypeDef, out var value))
			{
				return value;
			}
			return null;
		}
	}

	private readonly IMetadataExtensionsPolicy m_policy;

	private readonly IReflectionFactory m_factory;

	private readonly string m_modulePath;

	private readonly MetadataFile m_metadata;

	private IMetadataImport m_cachedThreadAffinityImporter;

	private string m_scopeName;

	private Token[] m_typeCodeMapping;

	private readonly ITypeUniverse m_assemblyResolver;

	private NestedTypeCache m_nestedTypeInfo;

	private Assembly m_assembly;

	private Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

	private const BindingFlags DefaultLookup = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

	private const BindingFlags MembersDeclaredOnTypeOnly = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private bool disposed;

	public override string FullyQualifiedName => m_modulePath;

	internal IMetadataExtensionsPolicy Policy => m_policy;

	internal IReflectionFactory Factory => m_factory;

	public ITypeUniverse AssemblyResolver => m_assemblyResolver;

	internal MetadataFile RawMetadata => m_metadata;

	internal IMetadataImport RawImport => GetThreadSafeImporter();

	public override string ScopeName
	{
		get
		{
			if (m_scopeName == null)
			{
				IMetadataImport threadSafeImporter = GetThreadSafeImporter();
				threadSafeImporter.GetScopeProps(null, 0, out var pchName, out var mvid);
				StringBuilder builder = StringBuilderPool.Get(pchName);
				threadSafeImporter.GetScopeProps(builder, builder.Capacity, out pchName, out mvid);
				builder.Length = pchName - 1;
				m_scopeName = builder.ToString();
				StringBuilderPool.Release(ref builder);
			}
			return m_scopeName;
		}
	}

	public override Guid ModuleVersionId
	{
		get
		{
			RawImport.GetScopeProps(null, 0, out var _, out var mvid);
			return mvid;
		}
	}

	public override string Name => Path.GetFileName(m_modulePath);

	public override Assembly Assembly => m_assembly;

	public override int MetadataToken
	{
		get
		{
			RawImport.GetModuleFromScope(out var mdModule);
			return mdModule;
		}
	}

	public override int MDStreamVersion
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public MetadataOnlyModule(ITypeUniverse universe, MetadataFile import, string modulePath)
		: this(universe, import, new DefaultFactory(), modulePath)
	{
	}

	public MetadataOnlyModule(ITypeUniverse universe, MetadataFile import, IReflectionFactory factory, string modulePath)
	{
		m_assemblyResolver = universe;
		m_metadata = import;
		m_factory = factory;
		m_policy = new MetadataExtensionsPolicy20(universe);
		m_modulePath = modulePath;
		object uniqueObjectForIUnknown = Marshal.GetUniqueObjectForIUnknown(m_metadata.RawPtr);
		m_cachedThreadAffinityImporter = (IMetadataImport)uniqueObjectForIUnknown;
	}

	public override string ToString()
	{
		if (m_metadata == null)
		{
			return "uninitialized";
		}
		return ScopeName;
	}

	public override bool Equals(object obj)
	{
		if (ReferenceEquals(this, obj))
		{
			return true;
		}
		MetadataOnlyModule metadataOnlyModule = obj as MetadataOnlyModule;
		if (metadataOnlyModule != null)
		{
			if (!m_assemblyResolver.Equals(metadataOnlyModule.AssemblyResolver))
			{
				return false;
			}
			return ScopeName == metadataOnlyModule.ScopeName;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return m_metadata.RawPtr.GetHashCode() + m_assemblyResolver.GetHashCode();
	}

	internal bool IsValidToken(int token)
	{
		return RawImport.IsValidToken((uint)token);
	}

	internal bool IsValidToken(Token token)
	{
		return IsValidToken(token.Value);
	}

	public byte[] ReadEmbeddedBlob(EmbeddedBlobPointer pointer, int countBytes)
	{
		return m_metadata.ReadEmbeddedBlob(pointer, countBytes);
	}

	private IMetadataImport GetThreadSafeImporter()
	{
		if (disposed)
		{
			throw new ObjectDisposedException(typeof(MetadataOnlyModule).Name);
		}
		return m_cachedThreadAffinityImporter;
	}

	internal MetadataOnlyCommonType ResolveTypeDefToken(Token token)
	{
		return m_factory.CreateSimpleType(this, token);
	}

	private void EnsureValidToken(Token token)
	{
		if (!IsValidToken(token))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.InvalidMetadataToken, token.ToString()));
		}
	}

	internal Type ResolveTypeTokenInternal(Token token, GenericContext context)
	{
		EnsureValidToken(token);
		if (token.IsType(TokenType.TypeDef))
		{
			return ResolveTypeDefToken(token);
		}
		if (token.IsType(TokenType.TypeRef))
		{
			return Factory.CreateTypeRef(this, token);
		}
		if (token.IsType(TokenType.TypeSpec))
		{
			Type[] typeArgs = null;
			Type[] methodArgs = null;
			if (context != null)
			{
				typeArgs = context.TypeArgs;
				methodArgs = context.MethodArgs;
			}
			return Factory.CreateTypeSpec(this, token, typeArgs, methodArgs);
		}
		throw new ArgumentException(Resources.TypeTokenExpected);
	}

	internal Type ResolveTypeTokenInternal(Token token, CorElementType elementType, GenericContext context)
	{
		if (token.IsType(TokenType.TypeRef))
		{
			return Factory.CreateSignatureTypeRef(this, token, elementType);
		}
		return ResolveTypeTokenInternal(token, context);
	}

	internal Type GetGenericType(Token token, GenericContext context)
	{
		Type[] array = null;
		Type[] methodArgs = null;
		if (context != null)
		{
			array = context.TypeArgs;
			methodArgs = context.MethodArgs;
		}
		if (token.IsType(TokenType.TypeDef))
		{
			if (array != null && array.Length != 0)
			{
				return m_factory.CreateGenericType(this, token, array);
			}
			return m_factory.CreateSimpleType(this, token);
		}
		if (token.IsType(TokenType.TypeRef))
		{
			Type type = m_factory.CreateTypeRef(this, token);
			if (array != null && array.Length != 0)
			{
				type = type.MakeGenericType(array);
			}
			return type;
		}
		if (token.IsType(TokenType.TypeSpec))
		{
			return m_factory.CreateTypeSpec(this, token, array, methodArgs);
		}
		throw new ArgumentException(Resources.TypeTokenExpected);
	}

	private MethodBase ResolveMethodTokenInternal(Token methodToken, GenericContext context)
	{
		EnsureValidToken(methodToken);
		if (methodToken.IsType(TokenType.MethodDef))
		{
			return ResolveMethodDef(methodToken);
		}
		if (methodToken.IsType(TokenType.MemberRef))
		{
			return ResolveMethodRef(methodToken, context, null);
		}
		if (methodToken.IsType(TokenType.MethodSpec))
		{
			return ResolveMethodSpec(methodToken, context);
		}
		throw new ArgumentException(Resources.MethodTokenExpected);
	}

	private MethodInfo ResolveMethodSpec(Token methodToken, GenericContext context)
	{
		((IMetadataImport2)RawImport).GetMethodSpecProps(methodToken, out var tkParent, out var ppvSigBlob, out var pcbSigBlob);
		byte[] sig = ReadEmbeddedBlob(ppvSigBlob, pcbSigBlob);
		int index = 0;
		CorCallingConvention corCallingConvention = SignatureUtil.ExtractCallingConvention(sig, ref index);
		int num = SignatureUtil.ExtractInt(sig, ref index);
		Type[] array = new Type[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = SignatureUtil.ExtractType(sig, ref index, this, context);
		}
		Token token = new Token(tkParent);
		return token.TokenType switch
		{
			TokenType.MethodDef => GetGenericMethodInfo(token, new GenericContext(null, array)), 
			TokenType.MemberRef => (MethodInfo)ResolveMethodRef(token, context, array), 
			_ => throw new InvalidOperationException(), 
		};
	}

	private MethodBase ResolveMethodDef(Token methodToken)
	{
		List<Type> typeParameters = GetTypeParameters(methodToken.Value);
		GenericContext context = null;
		if (typeParameters.Count > 0)
		{
			context = new GenericContext(null, typeParameters.ToArray());
		}
		return MetadataOnlyMethodInfo.Create(this, methodToken, context);
	}

	internal MethodInfo GetGenericMethodInfo(Token methodToken, GenericContext genericContext)
	{
		return (MethodInfo)GetGenericMethodBase(methodToken, genericContext);
	}

	internal MethodBase GetGenericMethodBase(Token methodToken, GenericContext genericContext)
	{
		if (genericContext != null && (genericContext.TypeArgs == null || genericContext.TypeArgs.Length == 0) && (genericContext.MethodArgs == null || genericContext.MethodArgs.Length == 0))
		{
			genericContext = null;
		}
		return MetadataOnlyMethodInfo.Create(this, methodToken, genericContext);
	}

	internal MethodBase ResolveMethodRef(Token memberRef, GenericContext context, Type[] genericMethodParameters)
	{
		GetMemberRefData(memberRef, out var declaringTypeToken, out var nameMember, out var sig);
		byte[] signatureAsByteArray = sig.GetSignatureAsByteArray();
		int index = 0;
		CorCallingConvention corCallingConvention = SignatureUtil.ExtractCallingConvention(signatureAsByteArray, ref index);
		if (corCallingConvention == CorCallingConvention.VarArg)
		{
			throw new NotImplementedException(Resources.VarargSignaturesNotImplemented);
		}
		Type type = ResolveTypeTokenInternal(declaringTypeToken, context);
		MethodSignatureDescriptor expectedSignature;
		if (type.IsArray)
		{
			expectedSignature = SignatureUtil.ExtractMethodSignature(sig, this, context);
		}
		else
		{
			GenericContext context2 = new OpenGenericContext(this, type, memberRef);
			expectedSignature = SignatureUtil.ExtractMethodSignature(sig, this, context2);
		}
		GenericContext context3 = new GenericContext(type.GetGenericArguments(), genericMethodParameters);
		MethodBase methodBase = SignatureComparer.FindMatchingMethod(nameMember, type, expectedSignature, context3);
		if (methodBase == null)
		{
			throw new MissingMethodException(type.Name, nameMember);
		}
		return methodBase;
	}

	internal FieldInfo ResolveFieldRef(Token memberRef, GenericContext context)
	{
		GetMemberRefData(memberRef, out var declaringTypeToken, out var nameMember, out var _);
		Type type = ResolveTypeTokenInternal(declaringTypeToken, context);
		return type.GetField(nameMember, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	internal FieldInfo ResolveFieldTokenInternal(Token fieldToken, GenericContext context)
	{
		if (fieldToken.IsType(TokenType.FieldDef))
		{
			return Factory.CreateField(this, fieldToken, null, null);
		}
		if (fieldToken.IsType(TokenType.MemberRef))
		{
			return ResolveFieldRef(fieldToken, context);
		}
		throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidMetadataToken, fieldToken.ToString()));
	}

	public override string ResolveString(int metadataToken)
	{
		Token token = new Token(metadataToken);
		IMetadataImport rawImport = RawImport;
		rawImport.GetUserString(token, null, 0, out var pchString);
		char[] array = new char[pchString];
		rawImport.GetUserString(token, array, array.Length, out pchString);
		return new string(array);
	}

	public override IList<CustomAttributeData> GetCustomAttributesData()
	{
		return GetCustomAttributeData(MetadataToken);
	}

	internal Type ResolveTypeRef(ITypeReference typeReference)
	{
		Token resolutionScope = typeReference.ResolutionScope;
		string rawName = typeReference.RawName;
		switch (resolutionScope.TokenType)
		{
		case TokenType.TypeRef:
		{
			Type type = Factory.CreateTypeRef(this, resolutionScope);
			return type.GetNestedType(rawName, BindingFlags.Public | BindingFlags.NonPublic);
		}
		case TokenType.AssemblyRef:
		{
			Assembly assembly = m_assemblyResolver.ResolveAssembly(this, resolutionScope);
			if (assembly == null)
			{
				throw new UnresolvedAssemblyException(Resources.ResolverMustResolveToValidAssembly);
			}
			IAssembly2 assembly2 = (IAssembly2)assembly;
			if (assembly2.TypeUniverse != m_assemblyResolver)
			{
				throw new UnresolvedAssemblyException(Resources.ResolvedAssemblyMustBeWithinSameUniverse);
			}
			return assembly.GetType(rawName, throwOnError: true);
		}
		case TokenType.ModuleRef:
		{
			Module module = ResolveModuleRef(resolutionScope);
			return module.GetType(typeReference.FullName);
		}
		case TokenType.Module:
			return GetType(typeReference.FullName);
		default:
			throw new InvalidOperationException(Resources.InvalidMetadata);
		}
	}

	internal Module ResolveModuleRef(Token moduleRefToken)
	{
		if (Assembly == null)
		{
			throw new InvalidOperationException(Resources.CannotResolveModuleRefOnNetModule);
		}
		StringBuilder builder = StringBuilderPool.Get();
		IMetadataImport rawImport = RawImport;
		rawImport.GetModuleRefProps(moduleRefToken.Value, null, 0, out var pchName);
		builder.EnsureCapacity(pchName);
		rawImport.GetModuleRefProps(moduleRefToken.Value, builder, builder.Capacity, out pchName);
		string name = builder.ToString();
		StringBuilderPool.Release(ref builder);
		return Assembly.GetModule(name);
	}

	internal Token LookupTypeToken(string className)
	{
		return FindTypeDefByName(null, className, fThrow: true);
	}

	internal Token FindTypeDefByName(Type outerType, string className, bool fThrow)
	{
		Token outerTypeDefToken = new Token(0);
		if (outerType != null)
		{
			if (outerType.Module != this)
			{
				throw new InvalidOperationException(Resources.DifferentTokenResolverForOuterType);
			}
			outerTypeDefToken = new Token(outerType.MetadataToken);
		}
		return FindTypeDefByName(outerTypeDefToken, className, fThrow);
	}

	internal Token FindTypeDefByName(Token outerTypeDefToken, string className, bool fThrow)
	{
		_ = outerTypeDefToken.IsNil;
		int token;
		int num = RawImport.FindTypeDefByName(className, outerTypeDefToken, out token);
		if (!fThrow && num == -2146234064)
		{
			return Token.Nil;
		}
		if (num != 0)
		{
			throw Marshal.GetExceptionForHR(num);
		}
		return new Token(token);
	}

	internal void GetMemberRefData(Token token, out Token declaringTypeToken, out string nameMember, out SignatureBlob sig)
	{
		if (!IsValidToken(token))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidMetadataToken, token.ToString()));
		}
		IMetadataImport rawImport = RawImport;
		rawImport.GetMemberRefProps(token, out declaringTypeToken, null, 0, out var pchMember, out var ppvSigBlob, out var pbSig);
		StringBuilder builder = StringBuilderPool.Get((int)pchMember);
		rawImport.GetMemberRefProps(token, out declaringTypeToken, builder, builder.Capacity, out pchMember, out ppvSigBlob, out pbSig);
		nameMember = builder.ToString();
		StringBuilderPool.Release(ref builder);
		sig = SignatureBlob.ReadSignature(RawMetadata, ppvSigBlob, (int)pbSig);
	}

	internal uint GetMethodRva(int methodDef)
	{
		IMetadataImport rawImport = RawImport;
		rawImport.GetMethodProps((uint)methodDef, out var _, null, 0, out var _, out var _, out var _, out var _, out var pulCodeRVA, out var _);
		return pulCodeRVA;
	}

	internal MethodImplAttributes GetMethodImplFlags(int methodToken)
	{
		RawImport.GetRVA(methodToken, out var _, out var flags);
		return (MethodImplAttributes)flags;
	}

	internal void GetMethodAttrs(Token methodDef, out Token declaringTypeDef, out MethodAttributes attrs, out uint nameLength)
	{
		if (!IsValidToken(methodDef))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidMetadataToken, methodDef.ToString()));
		}
		uint value = (uint)methodDef.Value;
		IMetadataImport rawImport = RawImport;
		rawImport.GetMethodProps(value, out var pClass, null, 0, out nameLength, out attrs, out var _, out var _, out var _, out var _);
		declaringTypeDef = new Token(pClass);
	}

	internal void GetMethodSig(Token methodDef, out SignatureBlob signature)
	{
		if (!IsValidToken(methodDef))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidMetadataToken, methodDef.ToString()));
		}
		uint value = (uint)methodDef.Value;
		IMetadataImport rawImport = RawImport;
		rawImport.GetMethodProps(value, out var _, null, 0, out var _, out var _, out var ppvSigBlob, out var pcbSigBlob, out var _, out var _);
		signature = SignatureBlob.ReadSignature(RawMetadata, ppvSigBlob, (int)pcbSigBlob);
	}

	internal void GetMethodName(Token methodDef, uint nameLength, out string name)
	{
		if (!IsValidToken(methodDef))
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, Resources.InvalidMetadataToken, methodDef.ToString()));
		}
		uint value = (uint)methodDef.Value;
		IMetadataImport rawImport = RawImport;
		StringBuilder builder = StringBuilderPool.Get((int)nameLength);
		rawImport.GetMethodProps(value, out var _, builder, builder.Capacity, out nameLength, out var _, out var _, out var _, out var _, out var _);
		name = builder.ToString();
		StringBuilderPool.Release(ref builder);
	}

	internal CorElementType GetEnumUnderlyingType(Token tokenTypeDef)
	{
		IMetadataImport rawImport = RawImport;
		HCORENUM phEnum = default(HCORENUM);
		try
		{
			rawImport.EnumFields(ref phEnum, tokenTypeDef.Value, out var mdFieldDef, 1, out var pcTokens);
			while (pcTokens != 0)
			{
				rawImport.GetFieldProps(mdFieldDef, out var _, null, 0, out var _, out var pdwAttr, out var ppvSigBlob, out var pcbSigBlob, out var _, out var _, out var _);
				if ((pdwAttr & FieldAttributes.Static) == 0)
				{
					byte[] sig = ReadEmbeddedBlob(ppvSigBlob, pcbSigBlob);
					int index = 0;
					CorCallingConvention corCallingConvention = SignatureUtil.ExtractCallingConvention(sig, ref index);
					return SignatureUtil.ExtractElementType(sig, ref index);
				}
				rawImport.EnumFields(ref phEnum, tokenTypeDef.Value, out mdFieldDef, 1, out pcTokens);
			}
			throw new ArgumentException(Resources.OperationValidOnEnumOnly);
		}
		finally
		{
			phEnum.Close(rawImport);
		}
	}

	internal void GetTypeAttributes(Token tokenTypeDef, out Token tokenExtends, out TypeAttributes attr, out int nameLength)
	{
		IMetadataImport rawImport = RawImport;
		rawImport.GetTypeDefProps(tokenTypeDef.Value, null, 0, out nameLength, out attr, out var ptkExtends);
		tokenExtends = new Token(ptkExtends);
	}

	internal void GetTypeName(Token tokenTypeDef, int nameLength, out string name)
	{
		IMetadataImport rawImport = RawImport;
		StringBuilder builder = StringBuilderPool.Get(nameLength);
		rawImport.GetTypeDefProps(tokenTypeDef.Value, builder, builder.Capacity, out nameLength, out var _, out var _);
		name = TypeNameQuoter.GetQuotedTypeName(builder.ToString());
		StringBuilderPool.Release(ref builder);
	}

	internal static ConstructorInfo[] GetConstructorsOnType(MetadataOnlyCommonType type, BindingFlags flags)
	{
		CheckBindingFlagsInMethod(flags, "GetConstructorsOnType");
		List<ConstructorInfo> list = new List<ConstructorInfo>();
		IEnumerable<MethodBase> declaredConstructors = type.GetDeclaredConstructors();
		foreach (ConstructorInfo item in declaredConstructors)
		{
			if (Utility.IsBindingFlagsMatching(item, isInherited: false, flags))
			{
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	internal static ConstructorInfo GetConstructorOnType(MetadataOnlyCommonType type, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		CheckBinderAndModifiersforLMR(binder, modifiers);
		ConstructorInfo[] constructorsOnType = GetConstructorsOnType(type, bindingAttr);
		ConstructorInfo[] array = constructorsOnType;
		foreach (ConstructorInfo constructorInfo in array)
		{
			if (SignatureUtil.IsCallingConventionMatch(constructorInfo, callConvention) && SignatureUtil.IsParametersTypeMatch(constructorInfo, types))
			{
				return constructorInfo;
			}
		}
		return null;
	}

	private static void CheckBinderAndModifiersforLMR(Binder binder, ParameterModifier[] modifiers)
	{
		if (binder != null)
		{
			throw new NotSupportedException();
		}
		if (modifiers != null && modifiers.Length != 0)
		{
			throw new NotSupportedException();
		}
	}

	internal static MethodInfo GetMethodImplHelper(Type type, string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConv, Type[] types, ParameterModifier[] modifiers)
	{
		if (modifiers != null && modifiers.Length != 0)
		{
			throw new NotSupportedException();
		}
		MethodInfo[] methods = type.GetMethods(bindingAttr);
		if (binder == null)
		{
			return FilterMethod(methods, name, bindingAttr, callConv, types);
		}
		List<MethodBase> list = new List<MethodBase>();
		StringComparison stringComparison = SignatureUtil.GetStringComparison(bindingAttr);
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			if (methodInfo.Name.Equals(name, stringComparison) && SignatureUtil.IsCallingConventionMatch(methodInfo, callConv))
			{
				list.Add(methodInfo);
			}
		}
		return binder.SelectMethod(bindingAttr, list.ToArray(), types, modifiers) as MethodInfo;
	}

	private static MethodInfo FilterMethod(MethodInfo[] methods, string name, BindingFlags bindingAttr, CallingConventions callConv, Type[] types)
	{
		bool flag = false;
		MethodInfo methodInfo = null;
		StringComparison stringComparison = SignatureUtil.GetStringComparison(bindingAttr);
		foreach (MethodInfo methodInfo2 in methods)
		{
			if (flag && methodInfo.DeclaringType != null && !methodInfo.DeclaringType.Equals(methodInfo2.DeclaringType))
			{
				break;
			}
			if (methodInfo2.Name.Equals(name, stringComparison) && SignatureUtil.IsCallingConventionMatch(methodInfo2, callConv) && SignatureUtil.IsParametersTypeMatch(methodInfo2, types))
			{
				if (flag)
				{
					throw new AmbiguousMatchException();
				}
				methodInfo = methodInfo2;
				flag = true;
			}
		}
		return methodInfo;
	}

	internal static MethodInfo[] GetMethodsOnType(MetadataOnlyCommonType type, BindingFlags flags)
	{
		CheckBindingFlagsInMethod(flags, "GetMethodsOnType");
		List<MethodInfo> list = new List<MethodInfo>();
		foreach (MethodInfo declaredMethod in type.GetDeclaredMethods())
		{
			if (Utility.IsBindingFlagsMatching(declaredMethod, isInherited: false, flags))
			{
				list.Add(declaredMethod);
			}
		}
		if (WalkInheritanceChain(flags) && type.BaseType != null)
		{
			MethodInfo[] methods = type.BaseType.GetMethods(flags);
			List<MethodInfo> list2 = new List<MethodInfo>();
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo2 in array)
			{
				if (IncludeInheritedMethod(methodInfo2, list, flags))
				{
					list2.Add(methodInfo2);
				}
			}
			list.AddRange(list2);
		}
		return list.ToArray();
	}

	private static bool WalkInheritanceChain(BindingFlags flags)
	{
		if ((flags & BindingFlags.DeclaredOnly) != BindingFlags.Default)
		{
			return false;
		}
		return true;
	}

	private static IList<PropertyInfo> FilterInheritedProperties(IList<PropertyInfo> inheritedProperties, IList<PropertyInfo> properties, BindingFlags flags)
	{
		if (properties == null || properties.Count == 0)
		{
			return inheritedProperties;
		}
		List<PropertyInfo> list = new List<PropertyInfo>();
		List<MethodInfo> list2 = new List<MethodInfo>();
		List<MethodInfo> list3 = new List<MethodInfo>();
		foreach (PropertyInfo property in properties)
		{
			MethodInfo getMethod = property.GetGetMethod();
			if (getMethod != null)
			{
				list2.Add(getMethod);
			}
			MethodInfo setMethod = property.GetSetMethod();
			if (setMethod != null)
			{
				list3.Add(setMethod);
			}
		}
		foreach (PropertyInfo inheritedProperty in inheritedProperties)
		{
			MethodInfo getMethod2 = inheritedProperty.GetGetMethod();
			if (!(getMethod2 != null) || IncludeInheritedAccessor(getMethod2, list2, flags))
			{
				MethodInfo setMethod2 = inheritedProperty.GetSetMethod();
				if (!(setMethod2 != null) || IncludeInheritedAccessor(setMethod2, list3, flags))
				{
					list.Add(inheritedProperty);
				}
			}
		}
		return list;
	}

	private static IList<EventInfo> FilterInheritedEvents(IList<EventInfo> inheritedEvents, IList<EventInfo> events)
	{
		if (events == null || events.Count == 0)
		{
			return inheritedEvents;
		}
		List<EventInfo> list = new List<EventInfo>();
		foreach (EventInfo inheritedEvent in inheritedEvents)
		{
			bool flag = false;
			foreach (EventInfo @event in events)
			{
				if (inheritedEvent.Name.Equals(@event.Name, StringComparison.Ordinal))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(inheritedEvent);
			}
		}
		return list;
	}

	private static bool IncludeInheritedMethod(MethodInfo inheritedMethod, IEnumerable<MethodInfo> methods, BindingFlags flags)
	{
		if (!inheritedMethod.IsStatic)
		{
			if (inheritedMethod.IsVirtual)
			{
				return !IsOverride(methods, inheritedMethod);
			}
			return true;
		}
		if ((flags & BindingFlags.FlattenHierarchy) != BindingFlags.Default)
		{
			return true;
		}
		return false;
	}

	private static bool IncludeInheritedAccessor(MethodInfo inheritedMethod, IEnumerable<MethodInfo> methods, BindingFlags flags)
	{
		if (!inheritedMethod.IsStatic)
		{
			return !IsOverride(methods, inheritedMethod);
		}
		if ((flags & BindingFlags.FlattenHierarchy) != BindingFlags.Default)
		{
			return !IsOverride(methods, inheritedMethod);
		}
		return false;
	}

	private static bool IncludeInheritedField(FieldInfo inheritedField, BindingFlags flags)
	{
		if (inheritedField.IsPrivate)
		{
			return false;
		}
		if (!inheritedField.IsStatic)
		{
			return true;
		}
		if ((flags & BindingFlags.FlattenHierarchy) != BindingFlags.Default)
		{
			return true;
		}
		return false;
	}

	internal IEnumerable<MethodBase> GetMethodBasesOnDeclaredTypeOnly(Token tokenTypeDef, GenericContext context, EMethodKind kind)
	{
		IMetadataImport import = RawImport;
		HCORENUM hEnum = default(HCORENUM);
		try
		{
			while (true)
			{
				import.EnumMethods(ref hEnum, tokenTypeDef.Value, out var mdMethodDef, 1, out var pcTokens);
				if (pcTokens != 0)
				{
					List<Type> typeParameters = GetTypeParameters(mdMethodDef);
					GenericContext genericContext = new GenericContext(context.TypeArgs, typeParameters.ToArray());
					MethodBase genericMethodBase = GetGenericMethodBase(new Token(mdMethodDef), genericContext);
					if (genericMethodBase is ConstructorInfo == (kind == EMethodKind.Constructor))
					{
						yield return genericMethodBase;
					}
					continue;
				}
				break;
			}
		}
		finally
		{
			hEnum.Close(import);
		}
	}

	private List<Type> GetTypeParameters(int token)
	{
		List<Type> list = new List<Type>();
		foreach (int genericParameterToken in GetGenericParameterTokens(token))
		{
			Token typeVariableToken = new Token(genericParameterToken);
			if (typeVariableToken.IsType(TokenType.GenericPar))
			{
				list.Add(Factory.CreateTypeVariable(this, typeVariableToken));
			}
		}
		return list;
	}

	private static bool MatchSignatures(MethodBase m1, MethodBase methodCandidate)
	{
		if (m1.Name != methodCandidate.Name && (m1.Name.Length <= methodCandidate.Name.Length || m1.Name[m1.Name.Length - methodCandidate.Name.Length - 1] != '.' || !m1.Name.EndsWith(methodCandidate.Name, StringComparison.Ordinal)))
		{
			return false;
		}
		if (m1.IsStatic != methodCandidate.IsStatic)
		{
			return false;
		}
		ParameterInfo[] parameters = m1.GetParameters();
		ParameterInfo[] parameters2 = methodCandidate.GetParameters();
		if (parameters.Length != parameters2.Length)
		{
			return false;
		}
		if (m1.IsGenericMethodDefinition)
		{
			Type[] genericArguments = methodCandidate.GetGenericArguments();
			m1 = (m1 as MethodInfo).MakeGenericMethod(genericArguments);
			parameters = m1.GetParameters();
		}
		for (int i = 0; i < parameters.Length; i++)
		{
			Type parameterType = parameters[i].ParameterType;
			Type parameterType2 = parameters2[i].ParameterType;
			if (!parameterType.Equals(parameterType2))
			{
				return false;
			}
		}
		MethodInfo methodInfo = m1 as MethodInfo;
		MethodInfo methodInfo2 = methodCandidate as MethodInfo;
		if ((methodInfo != null && methodInfo2 == null) || (methodInfo == null && methodInfo2 != null))
		{
			return false;
		}
		if (methodInfo != null)
		{
			Type returnType = methodInfo.ReturnType;
			if (!returnType.Equals(methodInfo2.ReturnType))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsOverride(IEnumerable<MethodInfo> methods, MethodInfo m)
	{
		foreach (MethodInfo method in methods)
		{
			if (IsOverride(method, m))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsOverride(MethodInfo m1, MethodInfo m2)
	{
		return MatchSignatures(m1, m2);
	}

	internal static FieldInfo[] GetFieldsOnType(MetadataOnlyCommonType type, BindingFlags flags)
	{
		CheckBindingFlagsInMethod(flags, "GetFieldsOnType");
		List<FieldInfo> list = new List<FieldInfo>();
		foreach (FieldInfo item in type.Resolver.GetFieldsOnDeclaredTypeOnly(new Token(type.MetadataToken), type.GenericContext))
		{
			if (Utility.IsBindingFlagsMatching(item, isInherited: false, flags))
			{
				list.Add(item);
			}
		}
		if (WalkInheritanceChain(flags) && type.BaseType != null)
		{
			FieldInfo[] fields = type.BaseType.GetFields(flags);
			List<FieldInfo> list2 = new List<FieldInfo>();
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (IncludeInheritedField(fieldInfo, flags))
				{
					list2.Add(fieldInfo);
				}
			}
			list.AddRange(list2);
		}
		return list.ToArray();
	}

	private IEnumerable<FieldInfo> GetFieldsOnDeclaredTypeOnly(Token typeDefToken, GenericContext context)
	{
		HCORENUM hEnum = default(HCORENUM);
		IMetadataImport import = RawImport;
		Type[] typeArgs = Type.EmptyTypes;
		Type[] methodArgs = Type.EmptyTypes;
		if (context != null)
		{
			typeArgs = context.TypeArgs;
			methodArgs = context.MethodArgs;
		}
		try
		{
			while (true)
			{
				import.EnumFields(ref hEnum, typeDefToken, out var mdFieldDef, 1, out var pcTokens);
				if (pcTokens != 0)
				{
					yield return Factory.CreateField(this, new Token(mdFieldDef), typeArgs, methodArgs);
					continue;
				}
				break;
			}
		}
		finally
		{
			hEnum.Close(import);
		}
	}

	internal static PropertyInfo[] GetPropertiesOnType(MetadataOnlyCommonType type, BindingFlags flags)
	{
		CheckBindingFlagsInMethod(flags, "GetPropertiesOnType");
		List<PropertyInfo> list = new List<PropertyInfo>();
		bool isInherited = false;
		foreach (PropertyInfo declaredProperty in type.GetDeclaredProperties())
		{
			bool isStatic = false;
			bool isPublic = false;
			CheckIsStaticAndIsPublicOnProperty(declaredProperty, ref isStatic, ref isPublic);
			if (Utility.IsBindingFlagsMatching(declaredProperty, isStatic, isPublic, isInherited, flags))
			{
				list.Add(declaredProperty);
			}
		}
		if (WalkInheritanceChain(flags) && type.BaseType != null)
		{
			PropertyInfo[] properties = type.BaseType.GetProperties(flags);
			IList<PropertyInfo> collection = FilterInheritedProperties(properties, list, flags);
			list.AddRange(collection);
		}
		return list.ToArray();
	}

	internal IEnumerable<PropertyInfo> GetPropertiesOnDeclaredTypeOnly(Token tokenTypeDef, GenericContext context)
	{
		HCORENUM hEnum = default(HCORENUM);
		IMetadataImport import = RawImport;
		try
		{
			while (true)
			{
				import.EnumProperties(ref hEnum, tokenTypeDef.Value, out var mdFieldDef, 1, out var pcTokens);
				if (pcTokens != 0)
				{
					yield return Factory.CreatePropertyInfo(this, new Token(mdFieldDef), context.TypeArgs, context.MethodArgs);
					continue;
				}
				break;
			}
		}
		finally
		{
			hEnum.Close(import);
		}
	}

	internal static EventInfo[] GetEventsOnType(MetadataOnlyCommonType type, BindingFlags flags)
	{
		CheckBindingFlagsInMethod(flags, "GetEventsOnType");
		List<EventInfo> list = new List<EventInfo>();
		foreach (EventInfo item in type.Resolver.GetEventsOnDeclaredTypeOnly(new Token(type.MetadataToken), type.GenericContext))
		{
			bool isStatic = false;
			bool isPublic = false;
			CheckIsStaticAndIsPublicOnEvent(item, ref isStatic, ref isPublic);
			if (Utility.IsBindingFlagsMatching(item, isStatic, isPublic, isInherited: false, flags))
			{
				list.Add(item);
			}
		}
		if (WalkInheritanceChain(flags) && type.BaseType != null)
		{
			EventInfo[] events = type.BaseType.GetEvents(flags);
			IList<EventInfo> collection = FilterInheritedEvents(events, list);
			list.AddRange(collection);
		}
		return list.ToArray();
	}

	private IEnumerable<EventInfo> GetEventsOnDeclaredTypeOnly(Token tokenTypeDef, GenericContext context)
	{
		HCORENUM hEnum = default(HCORENUM);
		IMetadataImport import = RawImport;
		try
		{
			while (true)
			{
				import.EnumEvents(ref hEnum, tokenTypeDef.Value, out var mdFieldDef, 1, out var pcEvents);
				if (pcEvents != 0)
				{
					yield return Factory.CreateEventInfo(this, new Token(mdFieldDef), context.TypeArgs, context.MethodArgs);
					continue;
				}
				break;
			}
		}
		finally
		{
			hEnum.Close(import);
		}
	}

	internal IEnumerable<Type> GetNestedTypesOnType(MetadataOnlyCommonType type, BindingFlags flags)
	{
		return GetNestedTypesOnType(new Token(type.MetadataToken), flags);
	}

	private void EnsureNestedTypeCacheExists()
	{
		if (m_nestedTypeInfo == null)
		{
			m_nestedTypeInfo = new NestedTypeCache(this);
		}
	}

	internal IEnumerable<Type> GetNestedTypesOnType(Token tokenTypeDef, BindingFlags flags)
	{
		CheckBindingFlagsInMethod(flags, "GetNestedTypesOnType");
		EnsureNestedTypeCacheExists();
		IEnumerable<int> nestedTokens = m_nestedTypeInfo.GetNestedTokens(tokenTypeDef);
		if (nestedTokens == null)
		{
			yield break;
		}
		foreach (int item in nestedTokens)
		{
			Type type = ResolveType(item);
			bool isPublic = type.IsPublic || type.IsNestedPublic;
			if (Utility.IsBindingFlagsMatching(type, isStatic: false, isPublic, isInherited: false, flags))
			{
				yield return type;
			}
		}
	}

	public IList<CustomAttributeData> GetCustomAttributeData(int memberTokenValue)
	{
		List<CustomAttributeData> list = new List<CustomAttributeData>();
		HCORENUM phEnum = default(HCORENUM);
		IMetadataImport rawImport = RawImport;
		try
		{
			while (true)
			{
				rawImport.EnumCustomAttributes(ref phEnum, memberTokenValue, 0, out var mdCustomAttribute, 1u, out var pcTokens);
				if (pcTokens == 0)
				{
					break;
				}
				rawImport.GetCustomAttributeProps(mdCustomAttribute, out var _, out var tkType, out var _, out var _);
				ConstructorInfo ctor = ResolveCustomAttributeConstructor(tkType);
				CustomAttributeData item = new MetadataOnlyCustomAttributeData(this, mdCustomAttribute, ctor);
				list.Add(item);
			}
		}
		finally
		{
			phEnum.Close(rawImport);
		}
		IEnumerable<CustomAttributeData> pseudoCustomAttributes = m_policy.GetPseudoCustomAttributes(this, new Token(memberTokenValue));
		list.AddRange(pseudoCustomAttributes);
		return list;
	}

	private ConstructorInfo ResolveCustomAttributeConstructor(Token customAttributeConstructorTokenValue)
	{
		Token token = customAttributeConstructorTokenValue;
		EnsureValidToken(token);
		if (token.IsType(TokenType.MethodDef))
		{
			MethodBase methodBase = ResolveMethodDef(token);
			return (ConstructorInfo)methodBase;
		}
		if (token.IsType(TokenType.MemberRef))
		{
			GetMemberRefData(token, out var declaringTypeToken, out var _, out var _);
			Type declaringType = ResolveTypeTokenInternal(declaringTypeToken, null);
			return new ConstructorInfoRef(declaringType, this, token);
		}
		throw new ArgumentException(Resources.MethodTokenExpected);
	}

	internal void LazyAttributeParse(Token token, ConstructorInfo constructorInfo, out IList<CustomAttributeTypedArgument> constructorArguments, out IList<CustomAttributeNamedArgument> namedArguments)
	{
		IMetadataImport rawImport = RawImport;
		rawImport.GetCustomAttributeProps(token, out var _, out var _, out var blob, out var cbSize);
		byte[] array = RawMetadata.ReadEmbeddedBlob(blob, cbSize);
		int num = 0;
		if (BitConverter.ToInt16(array, num) != 1)
		{
			throw new ArgumentException(Resources.InvalidCustomAttributeFormat);
		}
		num += 2;
		constructorArguments = GetConstructorArguments(constructorInfo, array, ref num);
		namedArguments = GetNamedArguments(constructorInfo, array, ref num);
	}

	private IList<CustomAttributeTypedArgument> GetConstructorArguments(ConstructorInfo constructorInfo, byte[] customAttributeBlob, ref int index)
	{
		ConstructorInfoRef constructorInfoRef = constructorInfo as ConstructorInfoRef;
		ParameterInfo[] array = ((!(constructorInfoRef != null)) ? constructorInfo.GetParameters() : constructorInfoRef.GetSignatureParameters());
		IList<CustomAttributeTypedArgument> list = new List<CustomAttributeTypedArgument>(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			Type parameterType = array[i].ParameterType;
			CorElementType typeId = SignatureUtil.GetTypeId(parameterType);
			object obj = null;
			Type argumentType = null;
			if (typeId != CorElementType.Object)
			{
				obj = GetCustomAttributeArgumentValue(typeId, parameterType, customAttributeBlob, ref index);
				argumentType = parameterType;
			}
			else
			{
				SignatureUtil.ExtractCustomAttributeArgumentType(AssemblyResolver, this, customAttributeBlob, ref index, out var argumentTypeId, out argumentType);
				obj = GetCustomAttributeArgumentValue(argumentTypeId, argumentType, customAttributeBlob, ref index);
			}
			CustomAttributeTypedArgument item = new CustomAttributeTypedArgument(argumentType, obj);
			list.Add(item);
		}
		return list;
	}

	private IList<CustomAttributeNamedArgument> GetNamedArguments(ConstructorInfo constructorInfo, byte[] customAttributeBlob, ref int index)
	{
		ushort num = BitConverter.ToUInt16(customAttributeBlob, index);
		index += 2;
		IList<CustomAttributeNamedArgument> list = new List<CustomAttributeNamedArgument>(num);
		if (num == 0 && index != customAttributeBlob.Length)
		{
			throw new ArgumentException(Resources.InvalidCustomAttributeFormat);
		}
		for (int i = 0; i < num; i++)
		{
			NamedArgumentType namedArgumentType = SignatureUtil.ExtractNamedArgumentType(customAttributeBlob, ref index);
			SignatureUtil.ExtractCustomAttributeArgumentType(AssemblyResolver, this, customAttributeBlob, ref index, out var argumentTypeId, out var argumentType);
			string name = SignatureUtil.ExtractStringValue(customAttributeBlob, ref index);
			if (argumentType == null)
			{
				SignatureUtil.ExtractCustomAttributeArgumentType(AssemblyResolver, this, customAttributeBlob, ref index, out argumentTypeId, out argumentType);
			}
			object customAttributeArgumentValue = GetCustomAttributeArgumentValue(argumentTypeId, argumentType, customAttributeBlob, ref index);
			MemberInfo memberInfo = ((namedArgumentType != NamedArgumentType.Field) ? ((MemberInfo)constructorInfo.DeclaringType.GetProperty(name)) : ((MemberInfo)constructorInfo.DeclaringType.GetField(name, BindingFlags.Instance | BindingFlags.Public)));
			CustomAttributeTypedArgument typedArgument = new CustomAttributeTypedArgument(argumentType, customAttributeArgumentValue);
			CustomAttributeNamedArgument item = new CustomAttributeNamedArgument(memberInfo, typedArgument);
			list.Add(item);
		}
		if (index != customAttributeBlob.Length)
		{
			throw new ArgumentException(Resources.InvalidCustomAttributeFormat);
		}
		return list;
	}

	private object GetCustomAttributeArgumentValue(CorElementType typeId, Type type, byte[] customAttributeBlob, ref int index)
	{
		object result = null;
		switch (typeId)
		{
		case CorElementType.Type:
			result = SignatureUtil.ExtractTypeValue(AssemblyResolver, this, customAttributeBlob, ref index);
			break;
		case CorElementType.SzArray:
		{
			uint num = SignatureUtil.ExtractUIntValue(customAttributeBlob, ref index);
			if (num != uint.MaxValue)
			{
				result = SignatureUtil.ExtractListOfValues(type.GetElementType(), AssemblyResolver, this, num, customAttributeBlob, ref index);
			}
			break;
		}
		case CorElementType.Enum:
		{
			Type underlyingType = GetUnderlyingType(type);
			CorElementType typeId2 = SignatureUtil.GetTypeId(underlyingType);
			result = SignatureUtil.ExtractValue(typeId2, customAttributeBlob, ref index);
			break;
		}
		default:
			result = SignatureUtil.ExtractValue(typeId, customAttributeBlob, ref index);
			break;
		}
		return result;
	}

	internal static Type GetUnderlyingType(Type enumType)
	{
		FieldInfo[] fields = enumType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		return fields[0].FieldType;
	}

	internal Type GetEnclosingType(Token tokenTypeDef)
	{
		Token token = new Token(GetNestedClassProps(tokenTypeDef));
		if (token.IsNil)
		{
			return null;
		}
		return ResolveTypeTokenInternal(token, null);
	}

	public AssemblyName GetAssemblyNameFromAssemblyRef(Token assemblyRefToken)
	{
		IMetadataAssemblyImport assemblyImport = (IMetadataAssemblyImport)RawImport;
		return AssemblyNameHelper.GetAssemblyNameFromRef(assemblyRefToken, this, assemblyImport);
	}

	internal Token GetNestedClassProps(Token tokenTypeDef)
	{
		int tdEnclosingClass;
		int nestedClassProps = RawImport.GetNestedClassProps(tokenTypeDef, out tdEnclosingClass);
		return nestedClassProps switch
		{
			0 => new Token(tdEnclosingClass), 
			-2146234064 => new Token(0), 
			_ => throw Marshal.GetExceptionForHR(nestedClassProps), 
		};
	}

	internal int CountGenericParams(Token token)
	{
		if (!(RawImport is IMetadataImport2 metadataImport))
		{
			return 0;
		}
		HCORENUM hEnum = default(HCORENUM);
		metadataImport.EnumGenericParams(ref hEnum, token.Value, out var _, 1u, out var _);
		try
		{
			metadataImport.CountEnum(hEnum, out var pulCount);
			return pulCount;
		}
		finally
		{
			hEnum.Close(metadataImport);
		}
	}

	internal IEnumerable<int> GetGenericParameterTokens(int typeOrMethodToken)
	{
		new Token(typeOrMethodToken);
		if (!(RawImport is IMetadataImport2 importer2))
		{
			yield break;
		}
		HCORENUM hEnum = default(HCORENUM);
		try
		{
			while (true)
			{
				importer2.EnumGenericParams(ref hEnum, typeOrMethodToken, out var rGenericParams, 1u, out var pcGenericParams);
				if (pcGenericParams == 1)
				{
					yield return rGenericParams;
					continue;
				}
				break;
			}
		}
		finally
		{
			hEnum.Close(importer2);
		}
	}

	internal IEnumerable<Type> GetConstraintTypes(int gpToken)
	{
		new Token(gpToken);
		if (!(RawImport is IMetadataImport2 importer2))
		{
			yield break;
		}
		HCORENUM hEnum = default(HCORENUM);
		try
		{
			while (true)
			{
				importer2.EnumGenericParamConstraints(ref hEnum, gpToken, out var rGenericParamConstraints, 1u, out var pcGenericParams);
				if (pcGenericParams == 1)
				{
					importer2.GetGenericParamConstraintProps(rGenericParamConstraints, out var _, out var ptkConstraintType);
					yield return ResolveTypeTokenInternal(new Token(ptkConstraintType), null);
					continue;
				}
				break;
			}
		}
		finally
		{
			hEnum.Close(importer2);
		}
	}

	internal void GetGenericParameterProps(int mdGenericParam, out int ownerTypeToken, out int ownerMethodToken, out string name, out GenericParameterAttributes attributes, out uint genIndex)
	{
		IMetadataImport2 metadataImport = RawImport as IMetadataImport2;
		HCORENUM hCORENUM = default(HCORENUM);
		try
		{
			metadataImport.GetGenericParamProps(mdGenericParam, out genIndex, out var pdwParamFlags, out var ptOwner, out var ptkKind, null, 0u, out var pchName);
			attributes = (GenericParameterAttributes)pdwParamFlags;
			StringBuilder builder = StringBuilderPool.Get((int)pchName);
			metadataImport.GetGenericParamProps(mdGenericParam, out genIndex, out pdwParamFlags, out ptOwner, out ptkKind, builder, (uint)builder.Capacity, out pchName);
			name = builder.ToString();
			StringBuilderPool.Release(ref builder);
			if (new Token(ptOwner).IsType(TokenType.MethodDef))
			{
				ownerMethodToken = ptOwner;
				ownerTypeToken = 0;
			}
			else
			{
				ownerTypeToken = ptOwner;
				ownerMethodToken = 0;
			}
		}
		finally
		{
			hCORENUM.Close(metadataImport);
		}
	}

	internal IEnumerable<Type> GetInterfacesOnType(Type type)
	{
		if (type.IsGenericParameter)
		{
			foreach (Type constraintType in GetConstraintTypes(type.MetadataToken))
			{
				if (constraintType.IsInterface)
				{
					yield return constraintType;
				}
			}
			yield break;
		}
		_ = RawImport;
		foreach (Token item in EnumerateInterfaceImplsOnType(type))
		{
			yield return GetInterfaceTypeFromInterfaceImpl(type, item);
		}
	}

	internal IEnumerable<Token> EnumerateInterfaceImplsOnType(Type type)
	{
		IMetadataImport import = RawImport;
		HCORENUM hEnum = default(HCORENUM);
		int cImpls = 1;
		while (true)
		{
			import.EnumInterfaceImpls(ref hEnum, type.MetadataToken, out var rImpls, 1, ref cImpls);
			if (cImpls != 1)
			{
				break;
			}
			yield return new Token(rImpls);
		}
		hEnum.Close(import);
	}

	internal Type GetInterfaceTypeFromInterfaceImpl(Type type, Token tImpl)
	{
		RawImport.GetInterfaceImplProps(tImpl.Value, out var pClass, out var ptkIface);
		Token token = new Token(pClass);
		Token token2 = new Token(ptkIface);
		return ResolveTypeTokenInternal(token2, new GenericContext(type.GetGenericArguments(), null));
	}

	public static Type GetInterfaceHelper(Type[] interfaces, string name, bool ignoreCase)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		Type type = null;
		foreach (Type type2 in interfaces)
		{
			if (Utility.Compare(name, type2.FullName, ignoreCase))
			{
				if (type != null)
				{
					throw new AmbiguousMatchException();
				}
				type = type2;
			}
		}
		return type;
	}

	public IEnumerable<Type> GetTypeList()
	{
		foreach (int typeToken in GetTypeTokenList())
		{
			yield return ResolveTypeTokenInternal(new Token(typeToken), null);
		}
	}

	private IEnumerable<int> GetTypeTokenList()
	{
		IMetadataImport import = RawImport;
		HCORENUM hEnum = default(HCORENUM);
		try
		{
			uint pcTypeDefs = 1u;
			while (true)
			{
				import.EnumTypeDefs(ref hEnum, out var rTypeDefs, 1u, out pcTypeDefs);
				if (pcTypeDefs == 1)
				{
					yield return rTypeDefs;
					continue;
				}
				break;
			}
		}
		finally
		{
			hEnum.Close(import);
		}
	}

	private static void CheckBindingFlagsInMethod(BindingFlags flags, string methodName)
	{
		if ((flags | (BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.ExactBinding)) != (BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy | BindingFlags.InvokeMethod | BindingFlags.CreateInstance | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.ExactBinding))
		{
			throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, Resources.MethodIsUsingUnsupportedBindingFlags, methodName, flags));
		}
	}

	private static void CheckIsStaticAndIsPublicOnProperty(PropertyInfo propertyInfo, ref bool isStatic, ref bool isPublic)
	{
		bool nonPublic = true;
		MethodInfo getMethod = propertyInfo.GetGetMethod(nonPublic);
		CheckIsStaticAndIsPublic(getMethod, ref isStatic, ref isPublic);
		MethodInfo setMethod = propertyInfo.GetSetMethod(nonPublic);
		CheckIsStaticAndIsPublic(setMethod, ref isStatic, ref isPublic);
	}

	private static void CheckIsStaticAndIsPublicOnEvent(EventInfo eventInfo, ref bool isStatic, ref bool isPublic)
	{
		bool nonPublic = true;
		MethodInfo addMethod = eventInfo.GetAddMethod(nonPublic);
		CheckIsStaticAndIsPublic(addMethod, ref isStatic, ref isPublic);
		MethodInfo removeMethod = eventInfo.GetRemoveMethod(nonPublic);
		CheckIsStaticAndIsPublic(removeMethod, ref isStatic, ref isPublic);
		MethodInfo raiseMethod = eventInfo.GetRaiseMethod(nonPublic);
		CheckIsStaticAndIsPublic(raiseMethod, ref isStatic, ref isPublic);
	}

	private static void CheckIsStaticAndIsPublic(MethodInfo methodInfo, ref bool isStatic, ref bool isPublic)
	{
		if (!(methodInfo == null))
		{
			if (methodInfo.IsStatic)
			{
				isStatic = true;
			}
			if (methodInfo.IsPublic)
			{
				isPublic = true;
			}
		}
	}

	internal void SetContainingAssembly(Assembly assembly)
	{
		m_assembly = assembly;
	}

	public override Type GetType(string className, bool throwOnError, bool ignoreCase)
	{
		if (ignoreCase)
		{
			throw new NotImplementedException(Resources.CaseInsensitiveTypeLookupNotImplemented);
		}
		if (_typeCache.TryGetValue(className, out var value))
		{
			return value;
		}
		Func<AssemblyName, Assembly> assemblyResolver = (AssemblyName assemblyName) => AssemblyResolver.ResolveAssembly(assemblyName);
		Func<Assembly, string, bool, Type> typeResolver = delegate(Assembly assembly, string simpleTypeName, bool ignoreCaseInCallback)
		{
			bool throwOnError2 = false;
			if (assembly != null)
			{
				Type type = assembly.GetType(simpleTypeName, throwOnError2, ignoreCaseInCallback);
				_typeCache[className] = type;
				return type;
			}
			Token token = FindTypeDefByName(null, simpleTypeName, fThrow: false);
			if (token.IsNil)
			{
				_typeCache[className] = null;
				return (Type)null;
			}
			Type type2 = ResolveType(token.Value);
			_typeCache[className] = type2;
			return type2;
		};
		return Type.GetType(className, assemblyResolver, typeResolver, throwOnError);
	}

	public override Type[] GetTypes()
	{
		List<Type> list = new List<Type>(GetTypeList());
		return list.ToArray();
	}

	public override Type[] FindTypes(TypeFilter filter, object filterCriteria)
	{
		List<Type> list = new List<Type>();
		foreach (Type type in GetTypeList())
		{
			if (filter(type, filterCriteria))
			{
				list.Add(type);
			}
		}
		return list.ToArray();
	}

	public override FieldInfo GetField(string name, BindingFlags bindingAttr)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		FieldInfo[] fields = GetFields(bindingAttr);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (fieldInfo.Name.Equals(name))
			{
				return fieldInfo;
			}
		}
		return null;
	}

	public override FieldInfo[] GetFields(BindingFlags bindingFlags)
	{
		CheckBindingFlagsInMethod(bindingFlags, "GetFields");
		IMetadataImport rawImport = RawImport;
		HCORENUM phEnum = default(HCORENUM);
		List<FieldInfo> list = new List<FieldInfo>();
		try
		{
			uint pcTokens = 1u;
			while (true)
			{
				rawImport.EnumFields(ref phEnum, MetadataToken, out var mdFieldDef, 1, out pcTokens);
				if (pcTokens != 1)
				{
					break;
				}
				FieldInfo fieldInfo = ResolveField(mdFieldDef);
				if (Utility.IsBindingFlagsMatching(fieldInfo, isInherited: false, bindingFlags))
				{
					list.Add(fieldInfo);
				}
			}
		}
		finally
		{
			phEnum.Close(rawImport);
		}
		return list.ToArray();
	}

	protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
	{
		CheckBinderAndModifiersforLMR(binder, modifiers);
		MethodInfo[] methods = GetMethods(bindingAttr);
		return FilterMethod(methods, name, bindingAttr, callConvention, types);
	}

	public override MethodInfo[] GetMethods(BindingFlags bindingFlags)
	{
		CheckBindingFlagsInMethod(bindingFlags, "GetMethods");
		IMetadataImport rawImport = RawImport;
		HCORENUM phEnum = default(HCORENUM);
		List<MethodInfo> list = new List<MethodInfo>();
		try
		{
			int pcTokens = 1;
			while (true)
			{
				rawImport.EnumMethods(ref phEnum, MetadataToken, out var mdMethodDef, 1, out pcTokens);
				if (pcTokens != 1)
				{
					break;
				}
				MethodBase methodBase = ResolveMethodTokenInternal(new Token(mdMethodDef), null);
				if (Utility.IsBindingFlagsMatching(methodBase, isInherited: false, bindingFlags))
				{
					MethodInfo methodInfo = methodBase as MethodInfo;
					if (methodInfo != null)
					{
						list.Add(methodInfo);
					}
				}
			}
		}
		finally
		{
			phEnum.Close(rawImport);
		}
		return list.ToArray();
	}

	public override bool IsResource()
	{
		return false;
	}

	public override Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		Type type = ResolveTypeTokenInternal(new Token(metadataToken), new GenericContext(genericTypeArguments, genericMethodArguments));
		Helpers.EnsureResolve(type);
		return type;
	}

	public override FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		return ResolveFieldTokenInternal(new Token(metadataToken), new GenericContext(genericTypeArguments, genericMethodArguments));
	}

	public override MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		return ResolveMethodTokenInternal(new Token(metadataToken), new GenericContext(genericTypeArguments, genericMethodArguments));
	}

	public override MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
	{
		throw new NotImplementedException();
	}

	public override byte[] ResolveSignature(int metadataToken)
	{
		throw new NotImplementedException();
	}

	internal bool IsSystemModule()
	{
		ITypeUniverse assemblyResolver = AssemblyResolver;
		return assemblyResolver.GetSystemAssembly().Equals(Assembly);
	}

	internal static bool IsWindowsRuntime(Module module)
	{
		return module.Assembly.GetName().ContentType == AssemblyContentType.WindowsRuntime;
	}

	internal TypeCode GetTypeCode(Type type)
	{
		if (type.IsEnum)
		{
			type = GetUnderlyingType(type);
			return Type.GetTypeCode(type);
		}
		if (!IsSystemModule())
		{
			return TypeCode.Object;
		}
		Token token = new Token(type.MetadataToken);
		if (m_typeCodeMapping == null)
		{
			m_typeCodeMapping = CreateTypeCodeMapping();
		}
		for (int i = 0; i < m_typeCodeMapping.Length; i++)
		{
			if (token == m_typeCodeMapping[i])
			{
				return (TypeCode)i;
			}
		}
		return TypeCode.Object;
	}

	private Token[] CreateTypeCodeMapping()
	{
		return new Token[19]
		{
			default(Token),
			LookupTypeToken("System.Object"),
			LookupTypeToken("System.DBNull"),
			LookupTypeToken("System.Boolean"),
			LookupTypeToken("System.Char"),
			LookupTypeToken("System.SByte"),
			LookupTypeToken("System.Byte"),
			LookupTypeToken("System.Int16"),
			LookupTypeToken("System.UInt16"),
			LookupTypeToken("System.Int32"),
			LookupTypeToken("System.UInt32"),
			LookupTypeToken("System.Int64"),
			LookupTypeToken("System.UInt64"),
			LookupTypeToken("System.Single"),
			LookupTypeToken("System.Double"),
			LookupTypeToken("System.Decimal"),
			LookupTypeToken("System.DateTime"),
			default(Token),
			LookupTypeToken("System.String")
		};
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
		disposed = true;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (m_cachedThreadAffinityImporter != null)
			{
				int num = Marshal.ReleaseComObject(m_cachedThreadAffinityImporter);
				m_cachedThreadAffinityImporter = null;
			}
			if (m_metadata != null)
			{
				m_metadata.Dispose();
			}
		}
	}

	public int RowCount(MetadataTable metadataTableIndex)
	{
		IMetadataTables metadataTables = (IMetadataTables)RawImport;
		metadataTables.GetTableInfo(metadataTableIndex, out var _, out var countRows, out _, out _, out var _);
		return countRows;
	}

	public override void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
	{
		IMetadataImport2 metadataImport = (IMetadataImport2)RawImport;
		metadataImport.GetPEKind(out peKind, out machine);
	}

	public string GetRuntimeVersion()
	{
		IMetadataImport2 metadataImport = (IMetadataImport2)RawImport;
		metadataImport.GetVersionString(null, 0, out var pchName);
		StringBuilder builder = StringBuilderPool.Get(pchName);
		metadataImport.GetVersionString(builder, builder.Capacity, out pchName);
		string result = builder.ToString();
		StringBuilderPool.Release(ref builder);
		return result;
	}
}
