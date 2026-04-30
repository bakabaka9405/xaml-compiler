using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading;
using System.Windows.Markup;
using System.Xaml.MS.Impl;
using System.Xaml.Schema;
using MS.Internal.Xaml.Parser;

namespace System.Xaml;

public class XamlType : IEquatable<XamlType>
{
	internal static class EmptyList<T>
	{
		public static readonly ReadOnlyCollection<T> Value = new ReadOnlyCollection<T>(new T[0]);
	}

	private string _name;

	private XamlSchemaContext _schemaContext;

	private IList<XamlType> _typeArguments;

	private TypeReflector _reflector;

	[SecurityCritical]
	private NullableReference<Type> _underlyingType;

	private ReadOnlyCollection<string> _namespaces;

	private ThreeValuedBool _isNameValid;

	public XamlType BaseType
	{
		get
		{
			EnsureReflector();
			if (!_reflector.BaseTypeIsSet)
			{
				_reflector.BaseType = LookupBaseType();
			}
			return _reflector.BaseType;
		}
	}

	public XamlTypeInvoker Invoker
	{
		get
		{
			EnsureReflector();
			if (_reflector.Invoker == null)
			{
				_reflector.Invoker = LookupInvoker() ?? XamlTypeInvoker.UnknownInvoker;
			}
			return _reflector.Invoker;
		}
	}

	public bool IsNameValid
	{
		get
		{
			if (_isNameValid == ThreeValuedBool.NotSet)
			{
				_isNameValid = ((!XamlName.IsValidXamlName(_name)) ? ThreeValuedBool.False : ThreeValuedBool.True);
			}
			return _isNameValid == ThreeValuedBool.True;
		}
	}

	public bool IsUnknown
	{
		get
		{
			EnsureReflector();
			return _reflector.IsUnknown;
		}
	}

	public string Name => _name;

	public string PreferredXamlNamespace
	{
		get
		{
			IList<string> xamlNamespaces = GetXamlNamespaces();
			if (xamlNamespaces.Count > 0)
			{
				return xamlNamespaces[0];
			}
			return null;
		}
	}

	public IList<XamlType> TypeArguments => _typeArguments;

	public Type UnderlyingType
	{
		[SecuritySafeCritical]
		get
		{
			if (!_underlyingType.IsSet)
			{
				_underlyingType.SetIfNull(LookupUnderlyingType());
			}
			return _underlyingType.Value;
		}
	}

	internal NullableReference<Type> UnderlyingTypeInternal
	{
		[SecuritySafeCritical]
		get
		{
			return _underlyingType;
		}
	}

	public bool ConstructionRequiresArguments => GetFlag(BoolTypeBits.ConstructionRequiresArguments);

	public bool IsArray => GetCollectionKind() == XamlCollectionKind.Array;

	public bool IsCollection => GetCollectionKind() == XamlCollectionKind.Collection;

	public bool IsConstructible => GetFlag(BoolTypeBits.Constructible);

	public bool IsDictionary => GetCollectionKind() == XamlCollectionKind.Dictionary;

	public bool IsGeneric => TypeArguments != null;

	public bool IsMarkupExtension => GetFlag(BoolTypeBits.MarkupExtension);

	public bool IsNameScope => GetFlag(BoolTypeBits.NameScope);

	public bool IsNullable => GetFlag(BoolTypeBits.Nullable);

	public bool IsPublic => GetFlag(BoolTypeBits.Public);

	public bool IsUsableDuringInitialization => GetFlag(BoolTypeBits.UsableDuringInitialization);

	public bool IsWhitespaceSignificantCollection => GetFlag(BoolTypeBits.WhitespaceSignificantCollection);

	public bool IsXData => GetFlag(BoolTypeBits.XmlData);

	public bool TrimSurroundingWhitespace => GetFlag(BoolTypeBits.TrimSurroundingWhitespace);

	public bool IsAmbient => GetFlag(BoolTypeBits.Ambient);

	public XamlType KeyType
	{
		get
		{
			if (!IsDictionary)
			{
				return null;
			}
			if (_reflector.KeyType == null)
			{
				_reflector.KeyType = LookupKeyType() ?? XamlLanguage.Object;
			}
			return _reflector.KeyType;
		}
	}

	public XamlType ItemType
	{
		get
		{
			if (GetCollectionKind() == XamlCollectionKind.None)
			{
				return null;
			}
			if (_reflector.ItemType == null)
			{
				_reflector.ItemType = LookupItemType() ?? XamlLanguage.Object;
			}
			return _reflector.ItemType;
		}
	}

	public IList<XamlType> AllowedContentTypes
	{
		get
		{
			XamlCollectionKind collectionKind = GetCollectionKind();
			if (collectionKind != XamlCollectionKind.Collection && collectionKind != XamlCollectionKind.Dictionary)
			{
				return null;
			}
			if (_reflector.AllowedContentTypes == null)
			{
				_reflector.AllowedContentTypes = LookupAllowedContentTypes() ?? EmptyList<XamlType>.Value;
			}
			return _reflector.AllowedContentTypes;
		}
	}

	public IList<XamlType> ContentWrappers
	{
		get
		{
			if (!IsCollection)
			{
				return null;
			}
			if (_reflector.ContentWrappers == null)
			{
				_reflector.ContentWrappers = LookupContentWrappers() ?? EmptyList<XamlType>.Value;
			}
			return _reflector.ContentWrappers;
		}
	}

	public XamlValueConverter<TypeConverter> TypeConverter
	{
		get
		{
			EnsureReflector();
			if (!_reflector.TypeConverterIsSet)
			{
				_reflector.TypeConverter = LookupTypeConverter();
			}
			return _reflector.TypeConverter;
		}
	}

	public XamlValueConverter<ValueSerializer> ValueSerializer
	{
		get
		{
			EnsureReflector();
			if (!_reflector.ValueSerializerIsSet)
			{
				_reflector.ValueSerializer = LookupValueSerializer();
			}
			return _reflector.ValueSerializer;
		}
	}

	public XamlMember ContentProperty
	{
		get
		{
			EnsureReflector();
			if (!_reflector.ContentPropertyIsSet)
			{
				_reflector.ContentProperty = LookupContentProperty();
			}
			return _reflector.ContentProperty;
		}
	}

	public XamlValueConverter<XamlDeferringLoader> DeferringLoader
	{
		get
		{
			EnsureReflector();
			if (!_reflector.DeferringLoaderIsSet)
			{
				_reflector.DeferringLoader = LookupDeferringLoader();
			}
			return _reflector.DeferringLoader;
		}
	}

	public XamlType MarkupExtensionReturnType
	{
		get
		{
			if (!IsMarkupExtension)
			{
				return null;
			}
			if (_reflector.MarkupExtensionReturnType == null)
			{
				_reflector.MarkupExtensionReturnType = LookupMarkupExtensionReturnType() ?? XamlLanguage.Object;
			}
			return _reflector.MarkupExtensionReturnType;
		}
	}

	public XamlSchemaContext SchemaContext => _schemaContext;

	internal bool IsUsableAsReadOnly
	{
		get
		{
			XamlCollectionKind collectionKind = GetCollectionKind();
			if (collectionKind != XamlCollectionKind.Collection && collectionKind != XamlCollectionKind.Dictionary)
			{
				return IsXData;
			}
			return true;
		}
	}

	internal MethodInfo IsReadOnlyMethod
	{
		get
		{
			if (ItemType == null || UnderlyingType == null)
			{
				return null;
			}
			if (!_reflector.IsReadOnlyMethodIsSet)
			{
				if (UnderlyingType != null && ItemType.UnderlyingType != null)
				{
					_reflector.IsReadOnlyMethod = CollectionReflector.GetIsReadOnlyMethod(UnderlyingType, ItemType.UnderlyingType);
				}
				else
				{
					_reflector.IsReadOnlyMethod = null;
				}
			}
			return _reflector.IsReadOnlyMethod;
		}
	}

	internal EventHandler<XamlSetMarkupExtensionEventArgs> SetMarkupExtensionHandler
	{
		get
		{
			if (!_reflector.XamlSetMarkupExtensionHandlerIsSet)
			{
				_reflector.XamlSetMarkupExtensionHandler = LookupSetMarkupExtensionHandler();
			}
			return _reflector.XamlSetMarkupExtensionHandler;
		}
	}

	internal EventHandler<XamlSetTypeConverterEventArgs> SetTypeConverterHandler
	{
		get
		{
			EnsureReflector();
			if (!_reflector.XamlSetTypeConverterHandlerIsSet)
			{
				_reflector.XamlSetTypeConverterHandler = LookupSetTypeConverterHandler();
			}
			return _reflector.XamlSetTypeConverterHandler;
		}
	}

	internal MethodInfo AddMethod
	{
		get
		{
			if (UnderlyingType == null)
			{
				return null;
			}
			EnsureReflector();
			if (!_reflector.AddMethodIsSet)
			{
				XamlCollectionKind collectionKind = GetCollectionKind();
				_reflector.AddMethod = CollectionReflector.LookupAddMethod(UnderlyingType, collectionKind);
			}
			return _reflector.AddMethod;
		}
	}

	internal MethodInfo GetEnumeratorMethod
	{
		get
		{
			if (GetCollectionKind() == XamlCollectionKind.None || UnderlyingType == null)
			{
				return null;
			}
			if (!_reflector.GetEnumeratorMethodIsSet)
			{
				_reflector.GetEnumeratorMethod = CollectionReflector.GetEnumeratorMethod(UnderlyingType);
			}
			return _reflector.GetEnumeratorMethod;
		}
	}

	private bool AreAttributesAvailable
	{
		get
		{
			EnsureReflector();
			if (!_reflector.CustomAttributeProviderIsSet)
			{
				_reflector.CustomAttributeProvider = LookupCustomAttributeProvider();
			}
			if (_reflector.CustomAttributeProvider == null)
			{
				return UnderlyingTypeInternal.Value != null;
			}
			return true;
		}
	}

	private BindingFlags ConstructorBindingFlags
	{
		get
		{
			BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
			if (!IsPublic)
			{
				bindingFlags |= BindingFlags.NonPublic;
			}
			return bindingFlags;
		}
	}

	protected XamlType(string typeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
	{
		if (typeName == null)
		{
			throw new ArgumentNullException("typeName");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		_name = typeName;
		_schemaContext = schemaContext;
		_typeArguments = GetTypeArguments(typeArguments);
	}

	public XamlType(string unknownTypeNamespace, string unknownTypeName, IList<XamlType> typeArguments, XamlSchemaContext schemaContext)
	{
		if (unknownTypeNamespace == null)
		{
			throw new ArgumentNullException("unknownTypeNamespace");
		}
		if (unknownTypeName == null)
		{
			throw new ArgumentNullException("unknownTypeName");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		_name = unknownTypeName;
		_namespaces = new ReadOnlyCollection<string>(new string[1] { unknownTypeNamespace });
		_schemaContext = schemaContext;
		_typeArguments = GetTypeArguments(typeArguments);
		_reflector = TypeReflector.UnknownReflector;
	}

	public XamlType(Type underlyingType, XamlSchemaContext schemaContext)
		: this(underlyingType, schemaContext, null)
	{
	}

	public XamlType(Type underlyingType, XamlSchemaContext schemaContext, XamlTypeInvoker invoker)
		: this(null, underlyingType, schemaContext, invoker, null)
	{
	}

	[SecuritySafeCritical]
	internal XamlType(string alias, Type underlyingType, XamlSchemaContext schemaContext, XamlTypeInvoker invoker, TypeReflector reflector)
	{
		if (underlyingType == null)
		{
			throw new ArgumentNullException("underlyingType");
		}
		if (schemaContext == null)
		{
			throw new ArgumentNullException("schemaContext");
		}
		_reflector = reflector ?? new TypeReflector(underlyingType);
		_name = alias ?? GetTypeName(underlyingType);
		_schemaContext = schemaContext;
		_typeArguments = GetTypeArguments(underlyingType, schemaContext);
		_underlyingType.Value = underlyingType;
		_reflector.Invoker = invoker;
	}

	public XamlMember GetMember(string name)
	{
		EnsureReflector();
		if (!_reflector.Members.TryGetValue(name, out var member) && !_reflector.Members.IsComplete)
		{
			member = LookupMember(name, skipReadOnlyCheck: false);
			return _reflector.Members.TryAdd(name, member);
		}
		return member;
	}

	public ICollection<XamlMember> GetAllMembers()
	{
		EnsureReflector();
		if (!_reflector.Members.IsComplete)
		{
			IEnumerable<XamlMember> enumerable = LookupAllMembers();
			if (enumerable != null)
			{
				foreach (XamlMember item in enumerable)
				{
					_reflector.Members.TryAdd(item.Name, item);
				}
				_reflector.Members.IsComplete = true;
			}
		}
		return _reflector.Members.Values;
	}

	public XamlMember GetAliasedProperty(XamlDirective directive)
	{
		EnsureReflector();
		if (!_reflector.TryGetAliasedProperty(directive, out var member))
		{
			member = LookupAliasedProperty(directive);
			_reflector.TryAddAliasedProperty(directive, member);
		}
		return member;
	}

	public XamlMember GetAttachableMember(string name)
	{
		EnsureReflector();
		if (!_reflector.AttachableMembers.TryGetValue(name, out var member) && !_reflector.AttachableMembers.IsComplete)
		{
			member = LookupAttachableMember(name);
			return _reflector.AttachableMembers.TryAdd(name, member);
		}
		return member;
	}

	public ICollection<XamlMember> GetAllAttachableMembers()
	{
		EnsureReflector();
		if (!_reflector.AttachableMembers.IsComplete)
		{
			IEnumerable<XamlMember> enumerable = LookupAllAttachableMembers();
			if (enumerable != null)
			{
				foreach (XamlMember item in enumerable)
				{
					_reflector.AttachableMembers.TryAdd(item.Name, item);
				}
			}
			_reflector.AttachableMembers.IsComplete = true;
		}
		return _reflector.AttachableMembers.Values;
	}

	public virtual bool CanAssignTo(XamlType xamlType)
	{
		if ((object)xamlType == null)
		{
			return false;
		}
		Type underlyingType = xamlType.UnderlyingType;
		XamlType xamlType2 = this;
		do
		{
			Type underlyingType2 = xamlType2.UnderlyingType;
			if (underlyingType != null && underlyingType2 != null)
			{
				if (underlyingType2.Assembly.ReflectionOnly && underlyingType.Assembly == typeof(XamlType).Assembly)
				{
					return LooseTypeExtensions.IsAssemblyQualifiedNameAssignableFrom(underlyingType, underlyingType2);
				}
				return underlyingType.IsAssignableFrom(underlyingType2);
			}
			if (xamlType2 == xamlType)
			{
				return true;
			}
			xamlType2 = xamlType2.BaseType;
		}
		while (xamlType2 != null);
		return false;
	}

	public IList<XamlType> GetPositionalParameters(int parameterCount)
	{
		EnsureReflector();
		if (!_reflector.TryGetPositionalParameters(parameterCount, out var result))
		{
			result = LookupPositionalParameters(parameterCount);
			return _reflector.TryAddPositionalParameters(parameterCount, result);
		}
		return result;
	}

	public virtual IList<string> GetXamlNamespaces()
	{
		if (_namespaces == null)
		{
			_namespaces = _schemaContext.GetXamlNamespaces(this);
			if (_namespaces == null)
			{
				_namespaces = new ReadOnlyCollection<string>(new string[1] { string.Empty });
			}
		}
		return _namespaces;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendTypeName(stringBuilder, forceNsInitialization: false);
		return stringBuilder.ToString();
	}

	internal string GetQualifiedName()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendTypeName(stringBuilder, forceNsInitialization: true);
		return stringBuilder.ToString();
	}

	internal bool IsVisibleTo(Assembly accessingAssembly)
	{
		if (IsPublic)
		{
			return true;
		}
		Type underlyingType = UnderlyingType;
		if (accessingAssembly != null && underlyingType != null)
		{
			return TypeReflector.IsVisibleTo(underlyingType, accessingAssembly, SchemaContext);
		}
		return false;
	}

	internal ICollection<XamlMember> GetAllExcludedReadOnlyMembers()
	{
		EnsureReflector();
		if (_reflector.ExcludedReadOnlyMembers == null)
		{
			_reflector.ExcludedReadOnlyMembers = LookupAllExcludedReadOnlyMembers() ?? EmptyList<XamlMember>.Value;
		}
		return _reflector.ExcludedReadOnlyMembers;
	}

	internal IEnumerable<ConstructorInfo> GetConstructors()
	{
		if (UnderlyingType == null)
		{
			return EmptyList<ConstructorInfo>.Value;
		}
		if (IsPublic)
		{
			return UnderlyingType.GetConstructors();
		}
		return GetPublicAndInternalConstructors();
	}

	internal ConstructorInfo GetConstructor(Type[] paramTypes)
	{
		if (UnderlyingType == null)
		{
			return null;
		}
		IEnumerable<ConstructorInfo> constructors = GetConstructors();
		ConstructorInfo[] array = constructors as ConstructorInfo[];
		if (array == null)
		{
			array = new List<ConstructorInfo>(constructors).ToArray();
		}
		Binder defaultBinder = Type.DefaultBinder;
		BindingFlags constructorBindingFlags = ConstructorBindingFlags;
		MethodBase[] match = array;
		MethodBase methodBase = defaultBinder.SelectMethod(constructorBindingFlags, match, paramTypes, null);
		return (ConstructorInfo)methodBase;
	}

	protected virtual XamlMember LookupAliasedProperty(XamlDirective directive)
	{
		if (AreAttributesAvailable)
		{
			Type type = null;
			bool skipReadOnlyCheck = false;
			if (directive == XamlLanguage.Key)
			{
				type = typeof(DictionaryKeyPropertyAttribute);
				skipReadOnlyCheck = true;
			}
			else if (directive == XamlLanguage.Name)
			{
				type = typeof(RuntimeNamePropertyAttribute);
			}
			else if (directive == XamlLanguage.Uid)
			{
				type = typeof(UidPropertyAttribute);
			}
			else if (directive == XamlLanguage.Lang)
			{
				type = typeof(XmlLangPropertyAttribute);
			}
			if (type != null && TryGetAttributeString(type, out var result))
			{
				if (string.IsNullOrEmpty(result))
				{
					return null;
				}
				return GetPropertyOrUnknown(result, skipReadOnlyCheck);
			}
		}
		if (BaseType != null)
		{
			return BaseType.GetAliasedProperty(directive);
		}
		return null;
	}

	protected virtual IList<XamlType> LookupAllowedContentTypes()
	{
		IList<XamlType> list = ContentWrappers ?? EmptyList<XamlType>.Value;
		List<XamlType> list2 = new List<XamlType>(list.Count + 1);
		list2.Add(ItemType);
		foreach (XamlType item in list)
		{
			if (item.ContentProperty != null && !item.ContentProperty.IsUnknown)
			{
				XamlType type = item.ContentProperty.Type;
				if (!list2.Contains(type))
				{
					list2.Add(type);
				}
			}
		}
		return list2.AsReadOnly();
	}

	protected virtual XamlType LookupBaseType()
	{
		Type underlyingType = UnderlyingType;
		if (underlyingType == null)
		{
			return XamlLanguage.Object;
		}
		if (underlyingType.BaseType != null)
		{
			return SchemaContext.GetXamlType(underlyingType.BaseType);
		}
		return null;
	}

	protected virtual XamlCollectionKind LookupCollectionKind()
	{
		if (UnderlyingType == null)
		{
			if (!(BaseType != null))
			{
				return XamlCollectionKind.None;
			}
			return BaseType.GetCollectionKind();
		}
		MethodInfo addMethod = null;
		XamlCollectionKind result = CollectionReflector.LookupCollectionKind(UnderlyingType, out addMethod);
		if (addMethod != null)
		{
			_reflector.AddMethod = addMethod;
		}
		return result;
	}

	protected virtual bool LookupConstructionRequiresArguments()
	{
		Type underlyingType = UnderlyingType;
		if (underlyingType == null)
		{
			return GetDefaultFlag(BoolTypeBits.ConstructionRequiresArguments);
		}
		if (underlyingType.IsValueType)
		{
			return false;
		}
		ConstructorInfo constructor = underlyingType.GetConstructor(ConstructorBindingFlags, null, Type.EmptyTypes, null);
		if (!(constructor == null))
		{
			return !TypeReflector.IsPublicOrInternal(constructor);
		}
		return true;
	}

	protected virtual XamlMember LookupContentProperty()
	{
		if (TryGetAttributeString(typeof(ContentPropertyAttribute), out var result))
		{
			if (string.IsNullOrEmpty(result))
			{
				return null;
			}
			return GetPropertyOrUnknown(result, skipReadOnlyCheck: false);
		}
		if (BaseType != null)
		{
			return BaseType.ContentProperty;
		}
		return null;
	}

	protected virtual IList<XamlType> LookupContentWrappers()
	{
		List<XamlType> list = null;
		if (AreAttributesAvailable)
		{
			List<Type> allAttributeContents = _reflector.GetAllAttributeContents<Type>(typeof(ContentWrapperAttribute));
			if (allAttributeContents != null)
			{
				list = new List<XamlType>(allAttributeContents.Count);
				foreach (Type item in allAttributeContents)
				{
					list.Add(SchemaContext.GetXamlType(item));
				}
			}
		}
		if (BaseType != null)
		{
			IList<XamlType> contentWrappers = BaseType.ContentWrappers;
			if (list == null)
			{
				return contentWrappers;
			}
			if (contentWrappers != null)
			{
				list.AddRange(contentWrappers);
			}
		}
		return GetReadOnly(list);
	}

	protected virtual ICustomAttributeProvider LookupCustomAttributeProvider()
	{
		return null;
	}

	protected virtual XamlValueConverter<XamlDeferringLoader> LookupDeferringLoader()
	{
		if (AreAttributesAvailable)
		{
			Type[] attributeTypes = _reflector.GetAttributeTypes(typeof(XamlDeferLoadAttribute), 2);
			if (attributeTypes != null)
			{
				return SchemaContext.GetValueConverter<XamlDeferringLoader>(attributeTypes[0], null);
			}
		}
		if (BaseType != null)
		{
			return BaseType.DeferringLoader;
		}
		return null;
	}

	protected virtual bool LookupIsConstructible()
	{
		Type underlyingType = UnderlyingType;
		if (underlyingType == null)
		{
			return GetDefaultFlag(BoolTypeBits.Constructible);
		}
		if (underlyingType.IsAbstract || underlyingType.IsInterface || underlyingType.IsNested || underlyingType.IsGenericParameter || underlyingType.IsGenericTypeDefinition)
		{
			return false;
		}
		if (underlyingType.IsValueType)
		{
			return true;
		}
		if (!ConstructionRequiresArguments)
		{
			return true;
		}
		using (IEnumerator<ConstructorInfo> enumerator = GetConstructors().GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				ConstructorInfo current = enumerator.Current;
				return true;
			}
		}
		return false;
	}

	protected virtual XamlTypeInvoker LookupInvoker()
	{
		if (!(UnderlyingType != null))
		{
			return null;
		}
		return new XamlTypeInvoker(this);
	}

	protected virtual bool LookupIsMarkupExtension()
	{
		return CanAssignTo(XamlLanguage.MarkupExtension);
	}

	protected virtual bool LookupIsNameScope()
	{
		return CanAssignTo(XamlLanguage.INameScope);
	}

	protected virtual bool LookupIsNullable()
	{
		if (UnderlyingType != null)
		{
			if (UnderlyingType.IsValueType)
			{
				return IsNullableGeneric();
			}
			return true;
		}
		return GetDefaultFlag(BoolTypeBits.Nullable);
	}

	protected virtual bool LookupIsUnknown()
	{
		if (_reflector != null)
		{
			return _reflector.IsUnknown;
		}
		return UnderlyingType == null;
	}

	protected virtual bool LookupIsWhitespaceSignificantCollection()
	{
		if (AreAttributesAvailable && _reflector.IsAttributePresent(typeof(WhitespaceSignificantCollectionAttribute)))
		{
			return true;
		}
		if (BaseType != null)
		{
			return BaseType.IsWhitespaceSignificantCollection;
		}
		if (IsUnknown)
		{
			return _reflector.GetFlag(BoolTypeBits.WhitespaceSignificantCollection).Value;
		}
		return GetDefaultFlag(BoolTypeBits.WhitespaceSignificantCollection);
	}

	protected virtual XamlType LookupKeyType()
	{
		MethodInfo addMethod = AddMethod;
		if (addMethod != null)
		{
			ParameterInfo[] parameters = addMethod.GetParameters();
			if (parameters.Length == 2)
			{
				return SchemaContext.GetXamlType(parameters[0].ParameterType);
			}
		}
		else if (UnderlyingType == null && BaseType != null)
		{
			return BaseType.KeyType;
		}
		return null;
	}

	protected virtual XamlType LookupItemType()
	{
		Type type = null;
		MethodInfo addMethod = AddMethod;
		if (addMethod != null)
		{
			ParameterInfo[] parameters = addMethod.GetParameters();
			if (parameters.Length == 2)
			{
				type = parameters[1].ParameterType;
			}
			else if (parameters.Length == 1)
			{
				type = parameters[0].ParameterType;
			}
		}
		else if (UnderlyingType != null)
		{
			if (UnderlyingType.IsArray)
			{
				type = UnderlyingType.GetElementType();
			}
		}
		else if (BaseType != null)
		{
			return BaseType.ItemType;
		}
		if (!(type != null))
		{
			return null;
		}
		return SchemaContext.GetXamlType(type);
	}

	protected virtual XamlType LookupMarkupExtensionReturnType()
	{
		if (AreAttributesAvailable)
		{
			Type attributeType = _reflector.GetAttributeType(typeof(MarkupExtensionReturnTypeAttribute));
			if (attributeType != null)
			{
				return SchemaContext.GetXamlType(attributeType);
			}
		}
		if (BaseType != null)
		{
			return BaseType.MarkupExtensionReturnType;
		}
		return null;
	}

	protected virtual IEnumerable<XamlMember> LookupAllAttachableMembers()
	{
		if (UnderlyingType == null)
		{
			if (!(BaseType != null))
			{
				return null;
			}
			return BaseType.GetAllAttachableMembers();
		}
		EnsureReflector();
		return _reflector.LookupAllAttachableMembers(SchemaContext);
	}

	protected virtual IEnumerable<XamlMember> LookupAllMembers()
	{
		if (UnderlyingType == null)
		{
			if (!(BaseType != null))
			{
				return null;
			}
			return BaseType.GetAllMembers();
		}
		EnsureReflector();
		_reflector.LookupAllMembers(out var newProperties, out var newEvents, out var knownMembers);
		if (newProperties != null)
		{
			foreach (PropertyInfo item2 in newProperties)
			{
				XamlMember property = SchemaContext.GetProperty(item2);
				if (!property.IsReadOnly || property.Type.IsUsableAsReadOnly)
				{
					knownMembers.Add(property);
				}
			}
		}
		if (newEvents != null)
		{
			foreach (EventInfo item3 in newEvents)
			{
				XamlMember item = SchemaContext.GetEvent(item3);
				knownMembers.Add(item);
			}
		}
		return knownMembers;
	}

	protected virtual XamlMember LookupMember(string name, bool skipReadOnlyCheck)
	{
		if (UnderlyingType == null)
		{
			if (BaseType != null)
			{
				if (!skipReadOnlyCheck)
				{
					return BaseType.GetMember(name);
				}
				return BaseType.LookupMember(name, skipReadOnlyCheck: true);
			}
			return null;
		}
		EnsureReflector();
		PropertyInfo propertyInfo = _reflector.LookupProperty(name);
		if (propertyInfo != null)
		{
			XamlMember property = SchemaContext.GetProperty(propertyInfo);
			if (!skipReadOnlyCheck && property.IsReadOnly && !property.Type.IsUsableAsReadOnly)
			{
				return null;
			}
			return property;
		}
		EventInfo eventInfo = _reflector.LookupEvent(name);
		if (eventInfo != null)
		{
			return SchemaContext.GetEvent(eventInfo);
		}
		return null;
	}

	protected virtual XamlMember LookupAttachableMember(string name)
	{
		if (UnderlyingType == null)
		{
			if (!(BaseType != null))
			{
				return null;
			}
			return BaseType.GetAttachableMember(name);
		}
		EnsureReflector();
		if (_reflector.LookupAttachableProperty(name, out var getter, out var setter))
		{
			XamlMember attachableProperty = SchemaContext.GetAttachableProperty(name, getter, setter);
			if (attachableProperty.IsReadOnly && !attachableProperty.Type.IsUsableAsReadOnly)
			{
				return null;
			}
			return attachableProperty;
		}
		setter = _reflector.LookupAttachableEvent(name);
		if (setter != null)
		{
			return SchemaContext.GetAttachableEvent(name, setter);
		}
		return null;
	}

	protected virtual IList<XamlType> LookupPositionalParameters(int parameterCount)
	{
		if (UnderlyingType == null)
		{
			return null;
		}
		EnsureReflector();
		if (_reflector.ReflectedPositionalParameters == null)
		{
			_reflector.ReflectedPositionalParameters = LookupAllPositionalParameters();
		}
		_reflector.ReflectedPositionalParameters.TryGetValue(parameterCount, out var value);
		return value;
	}

	protected virtual Type LookupUnderlyingType()
	{
		return UnderlyingTypeInternal.Value;
	}

	protected virtual bool LookupIsPublic()
	{
		Type underlyingType = UnderlyingType;
		if (underlyingType == null)
		{
			return GetDefaultFlag(BoolTypeBits.Public);
		}
		return underlyingType.IsVisible;
	}

	protected virtual bool LookupIsXData()
	{
		return CanAssignTo(XamlLanguage.IXmlSerializable);
	}

	protected virtual bool LookupIsAmbient()
	{
		if (AreAttributesAvailable && _reflector.IsAttributePresent(typeof(AmbientAttribute)))
		{
			return true;
		}
		if (BaseType != null)
		{
			return BaseType.IsAmbient;
		}
		if (IsUnknown)
		{
			return _reflector.GetFlag(BoolTypeBits.Ambient).Value;
		}
		return GetDefaultFlag(BoolTypeBits.Ambient);
	}

	protected virtual XamlValueConverter<TypeConverter> LookupTypeConverter()
	{
		if (AreAttributesAvailable)
		{
			Type attributeType = _reflector.GetAttributeType(typeof(TypeConverterAttribute));
			if (attributeType != null)
			{
				return SchemaContext.GetValueConverter<TypeConverter>(attributeType, null);
			}
		}
		if (BaseType != null)
		{
			XamlValueConverter<TypeConverter> typeConverter = BaseType.TypeConverter;
			if (typeConverter != null && typeConverter.TargetType != XamlLanguage.Object)
			{
				return typeConverter;
			}
		}
		Type underlyingType = UnderlyingType;
		if (underlyingType != null)
		{
			if (underlyingType.IsEnum)
			{
				return SchemaContext.GetValueConverter<TypeConverter>(typeof(EnumConverter), this);
			}
			XamlValueConverter<TypeConverter> typeConverter2 = BuiltInValueConverter.GetTypeConverter(underlyingType);
			if (typeConverter2 != null)
			{
				return typeConverter2;
			}
			if (IsNullableGeneric())
			{
				Type[] genericArguments = underlyingType.GetGenericArguments();
				XamlType xamlType = SchemaContext.GetXamlType(genericArguments[0]);
				return xamlType.TypeConverter;
			}
		}
		return null;
	}

	protected virtual XamlValueConverter<ValueSerializer> LookupValueSerializer()
	{
		if (AreAttributesAvailable)
		{
			Type attributeType = _reflector.GetAttributeType(typeof(ValueSerializerAttribute));
			if (attributeType != null)
			{
				return SchemaContext.GetValueConverter<ValueSerializer>(attributeType, null);
			}
		}
		if (BaseType != null)
		{
			XamlValueConverter<ValueSerializer> valueSerializer = BaseType.ValueSerializer;
			if (valueSerializer != null)
			{
				return valueSerializer;
			}
		}
		Type underlyingType = UnderlyingType;
		if (underlyingType != null)
		{
			XamlValueConverter<ValueSerializer> valueSerializer2 = BuiltInValueConverter.GetValueSerializer(underlyingType);
			if (valueSerializer2 != null)
			{
				return valueSerializer2;
			}
			if (IsNullableGeneric())
			{
				Type[] genericArguments = underlyingType.GetGenericArguments();
				XamlType xamlType = SchemaContext.GetXamlType(genericArguments[0]);
				return xamlType.ValueSerializer;
			}
		}
		return null;
	}

	protected virtual bool LookupTrimSurroundingWhitespace()
	{
		if (AreAttributesAvailable && _reflector.IsAttributePresent(typeof(TrimSurroundingWhitespaceAttribute)))
		{
			return true;
		}
		if (BaseType != null)
		{
			return BaseType.TrimSurroundingWhitespace;
		}
		return GetDefaultFlag(BoolTypeBits.TrimSurroundingWhitespace);
	}

	protected virtual bool LookupUsableDuringInitialization()
	{
		if (AreAttributesAvailable)
		{
			bool? attributeValue = _reflector.GetAttributeValue<bool>(typeof(UsableDuringInitializationAttribute));
			if (attributeValue.HasValue)
			{
				return attributeValue.Value;
			}
		}
		if (BaseType != null)
		{
			return BaseType.IsUsableDuringInitialization;
		}
		return GetDefaultFlag(BoolTypeBits.UsableDuringInitialization);
	}

	protected virtual EventHandler<XamlSetMarkupExtensionEventArgs> LookupSetMarkupExtensionHandler()
	{
		if (UnderlyingType != null && TryGetAttributeString(typeof(XamlSetMarkupExtensionAttribute), out var result))
		{
			if (string.IsNullOrEmpty(result))
			{
				return null;
			}
			return (EventHandler<XamlSetMarkupExtensionEventArgs>)SafeReflectionInvoker.CreateDelegate(typeof(EventHandler<XamlSetMarkupExtensionEventArgs>), UnderlyingType, result);
		}
		if (BaseType != null)
		{
			return BaseType.SetMarkupExtensionHandler;
		}
		return null;
	}

	protected virtual EventHandler<XamlSetTypeConverterEventArgs> LookupSetTypeConverterHandler()
	{
		if (UnderlyingType != null && TryGetAttributeString(typeof(XamlSetTypeConverterAttribute), out var result))
		{
			if (string.IsNullOrEmpty(result))
			{
				return null;
			}
			return (EventHandler<XamlSetTypeConverterEventArgs>)SafeReflectionInvoker.CreateDelegate(typeof(EventHandler<XamlSetTypeConverterEventArgs>), UnderlyingType, result);
		}
		if (BaseType != null)
		{
			return BaseType.SetTypeConverterHandler;
		}
		return null;
	}

	private void AppendTypeName(StringBuilder sb, bool forceNsInitialization)
	{
		string value = null;
		if (forceNsInitialization)
		{
			value = PreferredXamlNamespace;
		}
		else if (_namespaces != null && _namespaces.Count > 0)
		{
			value = _namespaces[0];
		}
		if (!string.IsNullOrEmpty(value))
		{
			sb.Append("{");
			sb.Append(PreferredXamlNamespace);
			sb.Append("}");
		}
		else if (UnderlyingTypeInternal.Value != null)
		{
			sb.Append(UnderlyingTypeInternal.Value.Namespace);
			sb.Append(".");
		}
		sb.Append(Name);
		if (!IsGeneric)
		{
			return;
		}
		sb.Append("(");
		for (int i = 0; i < TypeArguments.Count; i++)
		{
			TypeArguments[i].AppendTypeName(sb, forceNsInitialization);
			if (i < TypeArguments.Count - 1)
			{
				sb.Append(", ");
			}
		}
		sb.Append(")");
	}

	private void CreateReflector()
	{
		Interlocked.CompareExchange(value: (!LookupIsUnknown()) ? new TypeReflector(UnderlyingType) : TypeReflector.UnknownReflector, location1: ref _reflector, comparand: null);
	}

	private void EnsureReflector()
	{
		if (_reflector == null)
		{
			CreateReflector();
		}
	}

	private XamlCollectionKind GetCollectionKind()
	{
		EnsureReflector();
		if (!_reflector.CollectionKindIsSet)
		{
			_reflector.CollectionKind = LookupCollectionKind();
		}
		return _reflector.CollectionKind;
	}

	private bool GetFlag(BoolTypeBits flagBit)
	{
		EnsureReflector();
		bool? flag = _reflector.GetFlag(flagBit);
		if (!flag.HasValue)
		{
			flag = LookupBooleanValue(flagBit);
			_reflector.SetFlag(flagBit, flag.Value);
		}
		return flag.Value;
	}

	private XamlMember GetPropertyOrUnknown(string propertyName, bool skipReadOnlyCheck)
	{
		XamlMember xamlMember = (skipReadOnlyCheck ? LookupMember(propertyName, skipReadOnlyCheck: true) : GetMember(propertyName));
		if (xamlMember == null)
		{
			xamlMember = new XamlMember(propertyName, this, isAttachable: false);
		}
		return xamlMember;
	}

	private static bool GetDefaultFlag(BoolTypeBits flagBit)
	{
		return (BoolTypeBits.Default & flagBit) == flagBit;
	}

	private IEnumerable<ConstructorInfo> GetPublicAndInternalConstructors()
	{
		ConstructorInfo[] constructors = UnderlyingType.GetConstructors(ConstructorBindingFlags);
		foreach (ConstructorInfo constructorInfo in constructors)
		{
			if (TypeReflector.IsPublicOrInternal(constructorInfo))
			{
				yield return constructorInfo;
			}
		}
	}

	internal static ReadOnlyCollection<T> GetReadOnly<T>(IList<T> list)
	{
		if (list == null)
		{
			return null;
		}
		if (list.Count > 0)
		{
			return new ReadOnlyCollection<T>(list);
		}
		return EmptyList<T>.Value;
	}

	private static ReadOnlyCollection<XamlType> GetTypeArguments(IList<XamlType> typeArguments)
	{
		if (typeArguments == null || typeArguments.Count == 0)
		{
			return null;
		}
		foreach (XamlType typeArgument in typeArguments)
		{
			if (typeArgument == null)
			{
				throw new ArgumentException(SR.Get("CollectionCannotContainNulls", "typeArguments"));
			}
		}
		return new List<XamlType>(typeArguments).AsReadOnly();
	}

	private static ReadOnlyCollection<XamlType> GetTypeArguments(Type type, XamlSchemaContext schemaContext)
	{
		Type type2 = type;
		while (type2.IsArray)
		{
			type2 = type2.GetElementType();
		}
		if (!type2.IsGenericType)
		{
			return null;
		}
		Type[] genericArguments = type2.GetGenericArguments();
		XamlType[] array = new XamlType[genericArguments.Length];
		for (int i = 0; i < genericArguments.Length; i++)
		{
			array[i] = schemaContext.GetXamlType(genericArguments[i]);
		}
		return GetReadOnly(array);
	}

	private static string GetTypeName(Type type)
	{
		string text = type.Name;
		int num = text.IndexOf('`');
		if (num >= 0)
		{
			text = GenericTypeNameScanner.StripSubscript(text, out var subscript);
			text = text.Substring(0, num) + subscript;
		}
		if (type.IsNested)
		{
			text = GetTypeName(type.DeclaringType) + "+" + text;
		}
		return text;
	}

	private bool IsNullableGeneric()
	{
		if (UnderlyingType != null)
		{
			if (KS.Eq(UnderlyingType.Name, "Nullable`1") && UnderlyingType.Assembly == typeof(Nullable<>).Assembly)
			{
				return UnderlyingType.Namespace == typeof(Nullable<>).Namespace;
			}
			return false;
		}
		return false;
	}

	private ICollection<XamlMember> LookupAllExcludedReadOnlyMembers()
	{
		if (UnderlyingType == null)
		{
			return null;
		}
		ICollection<XamlMember> allMembers = GetAllMembers();
		IList<PropertyInfo> list = _reflector.LookupRemainingProperties();
		if (list == null)
		{
			return null;
		}
		List<XamlMember> list2 = new List<XamlMember>(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			XamlMember xamlMember = new XamlMember(list[i], SchemaContext);
			if (xamlMember.IsReadOnly && !xamlMember.Type.IsUsableAsReadOnly)
			{
				list2.Add(xamlMember);
			}
		}
		return new ReadOnlyCollection<XamlMember>(list2);
	}

	private Dictionary<int, IList<XamlType>> LookupAllPositionalParameters()
	{
		if (UnderlyingType == XamlLanguage.Type.UnderlyingType)
		{
			Dictionary<int, IList<XamlType>> dictionary = new Dictionary<int, IList<XamlType>>();
			XamlType xamlType = SchemaContext.GetXamlType(typeof(Type));
			XamlType[] list = new XamlType[1] { xamlType };
			dictionary.Add(1, GetReadOnly(list));
			return dictionary;
		}
		Dictionary<int, IList<XamlType>> dictionary2 = new Dictionary<int, IList<XamlType>>();
		foreach (ConstructorInfo constructor in GetConstructors())
		{
			ParameterInfo[] parameters = constructor.GetParameters();
			XamlType[] array = new XamlType[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameterInfo = parameters[i];
				Type parameterType = parameterInfo.ParameterType;
				XamlType xamlType2 = SchemaContext.GetXamlType(parameterType);
				array[i] = xamlType2;
			}
			if (dictionary2.ContainsKey(array.Length))
			{
				if (!SchemaContext.SupportMarkupExtensionsWithDuplicateArity)
				{
					throw new XamlSchemaException(SR.Get("MarkupExtensionWithDuplicateArity", UnderlyingType, array.Length));
				}
			}
			else
			{
				dictionary2.Add(array.Length, GetReadOnly(array));
			}
		}
		return dictionary2;
	}

	private bool LookupBooleanValue(BoolTypeBits typeBit)
	{
		bool flag;
		switch (typeBit)
		{
		case BoolTypeBits.Constructible:
			flag = LookupIsConstructible();
			break;
		case BoolTypeBits.ConstructionRequiresArguments:
			flag = LookupConstructionRequiresArguments();
			break;
		case BoolTypeBits.MarkupExtension:
			flag = LookupIsMarkupExtension();
			break;
		case BoolTypeBits.Nullable:
			flag = LookupIsNullable();
			break;
		case BoolTypeBits.NameScope:
			flag = LookupIsNameScope();
			break;
		case BoolTypeBits.Public:
			flag = LookupIsPublic();
			break;
		case BoolTypeBits.TrimSurroundingWhitespace:
			flag = LookupTrimSurroundingWhitespace();
			break;
		case BoolTypeBits.UsableDuringInitialization:
			flag = LookupUsableDuringInitialization();
			if (flag && IsMarkupExtension)
			{
				string message = SR.Get("UsableDuringInitializationOnME", this);
				throw new XamlSchemaException(message);
			}
			break;
		case BoolTypeBits.WhitespaceSignificantCollection:
			flag = LookupIsWhitespaceSignificantCollection();
			break;
		case BoolTypeBits.XmlData:
			flag = LookupIsXData();
			break;
		case BoolTypeBits.Ambient:
			flag = LookupIsAmbient();
			break;
		default:
			flag = GetDefaultFlag(typeBit);
			break;
		}
		return flag;
	}

	private bool TryGetAttributeString(Type attributeType, out string result)
	{
		if (!AreAttributesAvailable)
		{
			result = null;
			return false;
		}
		result = _reflector.GetAttributeString(attributeType, out var checkedInherited);
		if (checkedInherited || result != null)
		{
			return true;
		}
		XamlType baseType = BaseType;
		if (baseType != null)
		{
			return baseType.TryGetAttributeString(attributeType, out result);
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		XamlType xamlType = obj as XamlType;
		return this == xamlType;
	}

	public override int GetHashCode()
	{
		if (IsUnknown)
		{
			int num = _name.GetHashCode() ^ _namespaces[0].GetHashCode();
			if (_typeArguments != null && _typeArguments.Count > 0)
			{
				foreach (XamlType typeArgument in _typeArguments)
				{
					num ^= typeArgument.GetHashCode();
				}
			}
			return num;
		}
		if (UnderlyingType != null)
		{
			return UnderlyingType.GetHashCode() ^ 8;
		}
		return base.GetHashCode();
	}

	public bool Equals(XamlType other)
	{
		return this == other;
	}

	public static bool operator ==(XamlType xamlType1, XamlType xamlType2)
	{
		if ((object)xamlType1 == xamlType2)
		{
			return true;
		}
		if ((object)xamlType1 == null || (object)xamlType2 == null)
		{
			return false;
		}
		if (xamlType1.IsUnknown)
		{
			if (xamlType2.IsUnknown)
			{
				if (xamlType1._name == xamlType2._name && xamlType1._namespaces[0] == xamlType2._namespaces[0])
				{
					return typeArgumentsAreEqual(xamlType1, xamlType2);
				}
				return false;
			}
			return false;
		}
		if (xamlType2.IsUnknown)
		{
			return false;
		}
		return xamlType1.UnderlyingType == xamlType2.UnderlyingType;
	}

	public static bool operator !=(XamlType xamlType1, XamlType xamlType2)
	{
		return !(xamlType1 == xamlType2);
	}

	private static bool typeArgumentsAreEqual(XamlType xamlType1, XamlType xamlType2)
	{
		if (!xamlType1.IsGeneric)
		{
			return !xamlType2.IsGeneric;
		}
		if (!xamlType2.IsGeneric)
		{
			return false;
		}
		if (xamlType1._typeArguments.Count != xamlType2._typeArguments.Count)
		{
			return false;
		}
		for (int i = 0; i < xamlType1._typeArguments.Count; i++)
		{
			if (xamlType1._typeArguments[i] != xamlType2._typeArguments[i])
			{
				return false;
			}
		}
		return true;
	}
}
