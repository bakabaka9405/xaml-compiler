using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows.Markup;

namespace System.Xaml.Schema;

internal abstract class Reflector
{
	protected NullableReference<ICustomAttributeProvider> _attributeProvider;

	protected IList<CustomAttributeData> _attributeData;

	internal ICustomAttributeProvider CustomAttributeProvider
	{
		get
		{
			return _attributeProvider.Value;
		}
		set
		{
			_attributeProvider.Value = value;
		}
	}

	internal bool CustomAttributeProviderIsSet => _attributeProvider.IsSet;

	internal bool CustomAttributeProviderIsSetVolatile => _attributeProvider.IsSetVolatile;

	protected abstract MemberInfo Member { get; }

	internal void SetCustomAttributeProviderVolatile(ICustomAttributeProvider value)
	{
		_attributeProvider.SetVolatile(value);
	}

	public bool IsAttributePresent(Type attributeType)
	{
		if (CustomAttributeProvider != null)
		{
			return CustomAttributeProvider.IsDefined(attributeType, inherit: false);
		}
		try
		{
			CustomAttributeData attribute = GetAttribute(attributeType);
			return attribute != null;
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return IsAttributePresent(attributeType);
		}
	}

	public string GetAttributeString(Type attributeType, out bool checkedInherited)
	{
		if (CustomAttributeProvider != null)
		{
			checkedInherited = true;
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: true);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			if (attributeType == typeof(ContentPropertyAttribute))
			{
				return ((ContentPropertyAttribute)customAttributes[0]).Name;
			}
			if (attributeType == typeof(RuntimeNamePropertyAttribute))
			{
				return ((RuntimeNamePropertyAttribute)customAttributes[0]).Name;
			}
			if (attributeType == typeof(DictionaryKeyPropertyAttribute))
			{
				return ((DictionaryKeyPropertyAttribute)customAttributes[0]).Name;
			}
			if (attributeType == typeof(XamlSetMarkupExtensionAttribute))
			{
				return ((XamlSetMarkupExtensionAttribute)customAttributes[0]).XamlSetMarkupExtensionHandler;
			}
			if (attributeType == typeof(XamlSetTypeConverterAttribute))
			{
				return ((XamlSetTypeConverterAttribute)customAttributes[0]).XamlSetTypeConverterHandler;
			}
			if (attributeType == typeof(UidPropertyAttribute))
			{
				return ((UidPropertyAttribute)customAttributes[0]).Name;
			}
			if (attributeType == typeof(XmlLangPropertyAttribute))
			{
				return ((XmlLangPropertyAttribute)customAttributes[0]).Name;
			}
			if (attributeType == typeof(ConstructorArgumentAttribute))
			{
				return ((ConstructorArgumentAttribute)customAttributes[0]).ArgumentName;
			}
			return null;
		}
		try
		{
			checkedInherited = false;
			CustomAttributeData attribute = GetAttribute(attributeType);
			if (attribute == null)
			{
				return null;
			}
			return Extract<string>(attribute) ?? string.Empty;
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return GetAttributeString(attributeType, out checkedInherited);
		}
	}

	public IReadOnlyDictionary<char, char> GetBracketCharacterAttributes(Type attributeType)
	{
		if (CustomAttributeProvider != null)
		{
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			if (attributeType == typeof(MarkupExtensionBracketCharactersAttribute))
			{
				Dictionary<char, char> dictionary = new Dictionary<char, char>();
				object[] array = customAttributes;
				foreach (object obj in array)
				{
					MarkupExtensionBracketCharactersAttribute markupExtensionBracketCharactersAttribute = (MarkupExtensionBracketCharactersAttribute)obj;
					dictionary.Add(markupExtensionBracketCharactersAttribute.OpeningBracket, markupExtensionBracketCharactersAttribute.ClosingBracket);
				}
				return new ReadOnlyDictionary<char, char>(dictionary);
			}
			return null;
		}
		if (attributeType == typeof(MarkupExtensionBracketCharactersAttribute))
		{
			return TokenizeBracketCharacters(attributeType);
		}
		return null;
	}

	public T? GetAttributeValue<T>(Type attributeType) where T : struct
	{
		if (CustomAttributeProvider != null)
		{
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			if (attributeType == typeof(DesignerSerializationVisibilityAttribute))
			{
				DesignerSerializationVisibility visibility = ((DesignerSerializationVisibilityAttribute)customAttributes[0]).Visibility;
				return (T)(object)visibility;
			}
			if (attributeType == typeof(UsableDuringInitializationAttribute))
			{
				bool usable = ((UsableDuringInitializationAttribute)customAttributes[0]).Usable;
				return (T)(object)usable;
			}
			return null;
		}
		try
		{
			CustomAttributeData attribute = GetAttribute(attributeType);
			if (attribute == null)
			{
				return null;
			}
			return Extract<T>(attribute);
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return GetAttributeValue<T>(attributeType);
		}
	}

	public Type GetAttributeType(Type attributeType)
	{
		if (CustomAttributeProvider != null)
		{
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			if (attributeType == typeof(TypeConverterAttribute))
			{
				string converterTypeName = ((TypeConverterAttribute)customAttributes[0]).ConverterTypeName;
				return XamlNamespace.GetTypeFromFullTypeName(converterTypeName);
			}
			if (attributeType == typeof(MarkupExtensionReturnTypeAttribute))
			{
				return ((MarkupExtensionReturnTypeAttribute)customAttributes[0]).ReturnType;
			}
			if (attributeType == typeof(ValueSerializerAttribute))
			{
				return ((ValueSerializerAttribute)customAttributes[0]).ValueSerializerType;
			}
			return null;
		}
		try
		{
			CustomAttributeData attribute = GetAttribute(attributeType);
			if (attribute == null)
			{
				return null;
			}
			return ExtractType(attribute);
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return GetAttributeType(attributeType);
		}
	}

	public Type[] GetAttributeTypes(Type attributeType, int count)
	{
		if (CustomAttributeProvider != null)
		{
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			XamlDeferLoadAttribute xamlDeferLoadAttribute = (XamlDeferLoadAttribute)customAttributes[0];
			Type typeFromFullTypeName = XamlNamespace.GetTypeFromFullTypeName(xamlDeferLoadAttribute.LoaderTypeName);
			Type typeFromFullTypeName2 = XamlNamespace.GetTypeFromFullTypeName(xamlDeferLoadAttribute.ContentTypeName);
			return new Type[2] { typeFromFullTypeName, typeFromFullTypeName2 };
		}
		try
		{
			CustomAttributeData attribute = GetAttribute(attributeType);
			if (attribute == null)
			{
				return null;
			}
			return ExtractTypes(attribute, count);
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return GetAttributeTypes(attributeType, count);
		}
	}

	public List<T> GetAllAttributeContents<T>(Type attributeType)
	{
		if (CustomAttributeProvider != null)
		{
			object[] customAttributes = CustomAttributeProvider.GetCustomAttributes(attributeType, inherit: false);
			if (customAttributes.Length == 0)
			{
				return null;
			}
			List<T> list = new List<T>();
			if (attributeType == typeof(ContentWrapperAttribute))
			{
				object[] array = customAttributes;
				for (int i = 0; i < array.Length; i++)
				{
					ContentWrapperAttribute contentWrapperAttribute = (ContentWrapperAttribute)array[i];
					list.Add((T)(object)contentWrapperAttribute.ContentWrapper);
				}
				return list;
			}
			if (attributeType == typeof(DependsOnAttribute))
			{
				object[] array2 = customAttributes;
				for (int j = 0; j < array2.Length; j++)
				{
					DependsOnAttribute dependsOnAttribute = (DependsOnAttribute)array2[j];
					list.Add((T)(object)dependsOnAttribute.Name);
				}
				return list;
			}
			return null;
		}
		try
		{
			List<CustomAttributeData> list2 = new List<CustomAttributeData>();
			GetAttributes(attributeType, list2);
			if (list2.Count == 0)
			{
				return null;
			}
			List<T> list3 = new List<T>();
			foreach (CustomAttributeData item in list2)
			{
				T val = Extract<T>(item);
				list3.Add((T)(object)val);
			}
			return list3;
		}
		catch (CustomAttributeFormatException)
		{
			CustomAttributeProvider = Member;
			return GetAllAttributeContents<T>(attributeType);
		}
	}

	protected static bool? GetFlag(int bitMask, int bitToCheck)
	{
		int validMask = GetValidMask(bitToCheck);
		if ((bitMask & validMask) != 0)
		{
			return (bitMask & bitToCheck) != 0;
		}
		return null;
	}

	protected static int GetValidMask(int flagMask)
	{
		return flagMask << 16;
	}

	protected static void SetFlag(ref int bitMask, int bitToSet, bool value)
	{
		int validMask = GetValidMask(bitToSet);
		int mask = validMask + (value ? bitToSet : 0);
		SetBit(ref bitMask, mask);
	}

	protected static void SetBit(ref int flags, int mask)
	{
		int num;
		int value;
		do
		{
			num = flags;
			value = num | mask;
		}
		while (num != Interlocked.CompareExchange(ref flags, value, num));
	}

	private static bool TypesAreEqual(Type userType, Type builtInType)
	{
		if (userType.Assembly.ReflectionOnly)
		{
			return LooseTypeExtensions.AssemblyQualifiedNameEquals(userType, builtInType);
		}
		return userType == builtInType;
	}

	private ReadOnlyDictionary<char, char> TokenizeBracketCharacters(Type attributeType)
	{
		if (attributeType == typeof(MarkupExtensionBracketCharactersAttribute))
		{
			IList<CustomAttributeData> list = new List<CustomAttributeData>();
			GetAttributes(attributeType, list);
			Dictionary<char, char> dictionary = new Dictionary<char, char>();
			foreach (CustomAttributeData item in list)
			{
				char key = (char)item.ConstructorArguments[0].Value;
				char value = (char)item.ConstructorArguments[1].Value;
				dictionary.Add(key, value);
			}
			return new ReadOnlyDictionary<char, char>(dictionary);
		}
		return null;
	}

	private Type ExtractType(CustomAttributeData cad)
	{
		Type type = null;
		if (cad.ConstructorArguments.Count == 1)
		{
			type = ExtractType(cad.ConstructorArguments[0]);
		}
		if (type == null)
		{
			ThrowInvalidMetadata(cad, 1, typeof(Type));
		}
		return type;
	}

	private Type[] ExtractTypes(CustomAttributeData cad, int count)
	{
		if (cad.ConstructorArguments.Count != count)
		{
			ThrowInvalidMetadata(cad, count, typeof(Type));
		}
		Type[] array = new Type[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = ExtractType(cad.ConstructorArguments[i]);
			if (array[i] == null)
			{
				ThrowInvalidMetadata(cad, count, typeof(Type));
			}
		}
		return array;
	}

	private Type ExtractType(CustomAttributeTypedArgument arg)
	{
		if (arg.ArgumentType == typeof(Type))
		{
			return (Type)arg.Value;
		}
		if (arg.ArgumentType == typeof(string))
		{
			string fullName = (string)arg.Value;
			return XamlNamespace.GetTypeFromFullTypeName(fullName);
		}
		return null;
	}

	private T Extract<T>(CustomAttributeData cad)
	{
		if (cad.ConstructorArguments.Count == 0)
		{
			return default(T);
		}
		if (cad.ConstructorArguments.Count > 1 || !TypesAreEqual(cad.ConstructorArguments[0].ArgumentType, typeof(T)))
		{
			ThrowInvalidMetadata(cad, 1, typeof(T));
		}
		return (T)cad.ConstructorArguments[0].Value;
	}

	protected void EnsureAttributeData()
	{
		if (_attributeData == null)
		{
			_attributeData = CustomAttributeData.GetCustomAttributes(Member);
		}
	}

	private CustomAttributeData GetAttribute(Type attributeType)
	{
		EnsureAttributeData();
		for (int i = 0; i < _attributeData.Count; i++)
		{
			if (TypesAreEqual(_attributeData[i].Constructor.DeclaringType, attributeType))
			{
				return _attributeData[i];
			}
		}
		return null;
	}

	private void GetAttributes(Type attributeType, IList<CustomAttributeData> cads)
	{
		EnsureAttributeData();
		for (int i = 0; i < _attributeData.Count; i++)
		{
			if (TypesAreEqual(_attributeData[i].Constructor.DeclaringType, attributeType))
			{
				cads.Add(_attributeData[i]);
			}
		}
	}

	protected void ThrowInvalidMetadata(CustomAttributeData cad, int expectedCount, Type expectedType)
	{
		throw new XamlSchemaException(SR.Get("UnexpectedConstructorArg", cad.Constructor.DeclaringType, Member, expectedCount, expectedType));
	}
}
