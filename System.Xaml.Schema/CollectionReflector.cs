using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace System.Xaml.Schema;

internal static class CollectionReflector
{
	private static Type[] s_typeOfObjectArray;

	private static Type[] s_typeOfTwoObjectArray;

	private static MethodInfo s_getEnumeratorMethod;

	private static MethodInfo s_listAddMethod;

	private static MethodInfo s_dictionaryAddMethod;

	private static Type[] TypeOfObjectArray
	{
		get
		{
			if (s_typeOfObjectArray == null)
			{
				s_typeOfObjectArray = new Type[1] { typeof(object) };
			}
			return s_typeOfObjectArray;
		}
	}

	private static Type[] TypeOfTwoObjectArray
	{
		get
		{
			if (s_typeOfTwoObjectArray == null)
			{
				s_typeOfTwoObjectArray = new Type[2]
				{
					typeof(object),
					typeof(object)
				};
			}
			return s_typeOfTwoObjectArray;
		}
	}

	private static MethodInfo IEnumerableGetEnumeratorMethod
	{
		get
		{
			if (s_getEnumeratorMethod == null)
			{
				s_getEnumeratorMethod = typeof(IEnumerable).GetMethod("GetEnumerator");
			}
			return s_getEnumeratorMethod;
		}
	}

	private static MethodInfo IListAddMethod
	{
		get
		{
			if (s_listAddMethod == null)
			{
				s_listAddMethod = typeof(IList).GetMethod("Add");
			}
			return s_listAddMethod;
		}
	}

	private static MethodInfo IDictionaryAddMethod
	{
		get
		{
			if (s_dictionaryAddMethod == null)
			{
				s_dictionaryAddMethod = typeof(IDictionary).GetMethod("Add");
			}
			return s_dictionaryAddMethod;
		}
	}

	internal static XamlCollectionKind LookupCollectionKind(Type type, out MethodInfo addMethod)
	{
		addMethod = null;
		if (type.IsArray)
		{
			return XamlCollectionKind.Array;
		}
		if (!typeof(IEnumerable).IsAssignableFrom(type) && LookupEnumeratorMethod(type) == null)
		{
			return XamlCollectionKind.None;
		}
		if (typeof(IDictionary).IsAssignableFrom(type))
		{
			return XamlCollectionKind.Dictionary;
		}
		if (TryGetIDictionaryAdder(type, out addMethod))
		{
			return XamlCollectionKind.Dictionary;
		}
		if (typeof(IList).IsAssignableFrom(type))
		{
			return XamlCollectionKind.Collection;
		}
		if (TryGetICollectionAdder(type, out addMethod))
		{
			return XamlCollectionKind.Collection;
		}
		if (TryGetDictionaryAdder(type, mayBeIDictionary: false, out addMethod))
		{
			return XamlCollectionKind.Dictionary;
		}
		if (TryGetCollectionAdder(type, mayBeICollection: false, out addMethod))
		{
			return XamlCollectionKind.Collection;
		}
		return XamlCollectionKind.None;
	}

	internal static MethodInfo LookupAddMethod(Type type, XamlCollectionKind collectionKind)
	{
		MethodInfo addMethod = null;
		switch (collectionKind)
		{
		case XamlCollectionKind.Collection:
			if (TryGetCollectionAdder(type, mayBeICollection: true, out addMethod) && addMethod == null)
			{
				throw new XamlSchemaException(SR.Get("AmbiguousCollectionItemType", type));
			}
			break;
		case XamlCollectionKind.Dictionary:
			if (TryGetDictionaryAdder(type, mayBeIDictionary: true, out addMethod) && addMethod == null)
			{
				throw new XamlSchemaException(SR.Get("AmbiguousDictionaryItemType", type));
			}
			break;
		}
		return addMethod;
	}

	private static bool TryGetICollectionAdder(Type type, out MethodInfo addMethod)
	{
		bool hasMultiple = false;
		Type genericInterface = GetGenericInterface(type, typeof(ICollection<>), out hasMultiple);
		if (genericInterface != null)
		{
			addMethod = genericInterface.GetMethod("Add");
			return true;
		}
		addMethod = null;
		return hasMultiple;
	}

	private static bool TryGetCollectionAdder(Type type, bool mayBeICollection, out MethodInfo addMethod)
	{
		bool flag = false;
		if (mayBeICollection && TryGetICollectionAdder(type, out addMethod))
		{
			if (addMethod != null)
			{
				return true;
			}
			flag = true;
		}
		bool hasMoreThanOne = false;
		addMethod = GetAddMethod(type, 1, out hasMoreThanOne);
		if (addMethod == null && typeof(IList).IsAssignableFrom(type))
		{
			addMethod = IListAddMethod;
		}
		if (addMethod != null)
		{
			return true;
		}
		if (hasMoreThanOne || flag)
		{
			addMethod = GetMethod(type, "Add", TypeOfObjectArray);
			return true;
		}
		return false;
	}

	private static bool TryGetIDictionaryAdder(Type type, out MethodInfo addMethod)
	{
		bool hasMultiple = false;
		Type genericInterface = GetGenericInterface(type, typeof(IDictionary<, >), out hasMultiple);
		if (genericInterface != null)
		{
			addMethod = GetPublicMethod(genericInterface, "Add", 2);
			return true;
		}
		addMethod = null;
		return hasMultiple;
	}

	private static bool TryGetDictionaryAdder(Type type, bool mayBeIDictionary, out MethodInfo addMethod)
	{
		bool flag = false;
		if (mayBeIDictionary && TryGetIDictionaryAdder(type, out addMethod))
		{
			if (addMethod != null)
			{
				return true;
			}
			flag = true;
		}
		bool hasMoreThanOne = false;
		addMethod = GetAddMethod(type, 2, out hasMoreThanOne);
		if (addMethod == null && typeof(IDictionary).IsAssignableFrom(type))
		{
			addMethod = IDictionaryAddMethod;
		}
		if (addMethod != null)
		{
			return true;
		}
		if (hasMoreThanOne || flag)
		{
			addMethod = GetMethod(type, "Add", TypeOfTwoObjectArray);
			return true;
		}
		return false;
	}

	internal static MethodInfo GetAddMethod(Type type, Type contentType)
	{
		return GetMethod(type, "Add", new Type[1] { contentType });
	}

	internal static MethodInfo GetEnumeratorMethod(Type type)
	{
		if (typeof(IEnumerable).IsAssignableFrom(type))
		{
			return IEnumerableGetEnumeratorMethod;
		}
		return LookupEnumeratorMethod(type);
	}

	internal static MethodInfo GetIsReadOnlyMethod(Type collectionType, Type itemType)
	{
		Type type = typeof(ICollection<>).MakeGenericType(itemType);
		if (type.IsAssignableFrom(collectionType))
		{
			return type.GetProperty("IsReadOnly").GetGetMethod();
		}
		return null;
	}

	private static MethodInfo LookupEnumeratorMethod(Type type)
	{
		MethodInfo methodInfo = GetMethod(type, "GetEnumerator", Type.EmptyTypes);
		if (methodInfo != null && !typeof(IEnumerator).IsAssignableFrom(methodInfo.ReturnType))
		{
			methodInfo = null;
		}
		return methodInfo;
	}

	private static Type GetGenericInterface(Type type, Type interfaceType, out bool hasMultiple)
	{
		Type type2 = null;
		hasMultiple = false;
		if (type.IsGenericType && type.GetGenericTypeDefinition() == interfaceType)
		{
			return type;
		}
		Type[] interfaces = type.GetInterfaces();
		foreach (Type type3 in interfaces)
		{
			if (type3.IsGenericType && type3.GetGenericTypeDefinition() == interfaceType)
			{
				if (type2 != null)
				{
					hasMultiple = true;
					return null;
				}
				type2 = type3;
			}
		}
		return type2;
	}

	private static MethodInfo GetAddMethod(Type type, int paramCount, out bool hasMoreThanOne)
	{
		MethodInfo methodInfo = null;
		MemberInfo[] member = type.GetMember("Add", MemberTypes.Method, GetBindingFlags(type));
		if (member != null)
		{
			MemberInfo[] array = member;
			foreach (MemberInfo memberInfo in array)
			{
				MethodInfo methodInfo2 = (MethodInfo)memberInfo;
				if (!TypeReflector.IsPublicOrInternal(methodInfo2))
				{
					continue;
				}
				ParameterInfo[] parameters = methodInfo2.GetParameters();
				if (parameters != null && parameters.Length == paramCount)
				{
					if (methodInfo != null)
					{
						hasMoreThanOne = true;
						return null;
					}
					methodInfo = methodInfo2;
				}
			}
		}
		hasMoreThanOne = false;
		return methodInfo;
	}

	private static BindingFlags GetBindingFlags(Type type)
	{
		BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
		if (!type.IsVisible)
		{
			bindingFlags |= BindingFlags.NonPublic;
		}
		return bindingFlags;
	}

	private static MethodInfo GetMethod(Type type, string name, Type[] argTypes)
	{
		MethodInfo methodInfo = type.GetMethod(name, GetBindingFlags(type), null, argTypes, null);
		if (methodInfo != null && !TypeReflector.IsPublicOrInternal(methodInfo))
		{
			methodInfo = null;
		}
		return methodInfo;
	}

	private static MethodInfo GetPublicMethod(Type type, string name, int argCount)
	{
		MemberInfo[] member = type.GetMember(name, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public);
		foreach (MemberInfo memberInfo in member)
		{
			MethodInfo methodInfo = (MethodInfo)memberInfo;
			if (methodInfo.GetParameters().Length == argCount)
			{
				return methodInfo;
			}
		}
		return null;
	}
}
