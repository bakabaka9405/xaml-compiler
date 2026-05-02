using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler.DirectUI;

internal class ReflectionHelper
{
	private static ConcurrentDictionary<string, IList<CustomAttributeData>> _typeAttrs = new ConcurrentDictionary<string, IList<CustomAttributeData>>();

	internal static void Release()
	{
		_typeAttrs = new ConcurrentDictionary<string, IList<CustomAttributeData>>();
	}

	internal static IEnumerable<CustomAttributeData> GetCustomAttributeData(Type type, bool inherit, string attributeTypeFullName)
	{
		IEnumerable<CustomAttributeData> customAttributeData = GetCustomAttributeData(type, inherit);
		foreach (CustomAttributeData item in customAttributeData)
		{
			Type declaringType = item.Constructor.DeclaringType;
			if (declaringType.FullName == attributeTypeFullName)
			{
				yield return item;
			}
		}
	}

	internal static CustomAttributeData FindAttributeByTypeName(Type type, bool inherit, string attributeTypeFullName)
	{
		IEnumerable<CustomAttributeData> customAttributeData = GetCustomAttributeData(type, inherit);
		return FindAttributeByTypeName(customAttributeData, attributeTypeFullName);
	}

	internal static CustomAttributeData FindAttributeByTypeName(MethodInfo mi, string attributeTypeFullName)
	{
		IList<CustomAttributeData> customAttributeData = GetCustomAttributeData(mi);
		return FindAttributeByTypeName(customAttributeData, attributeTypeFullName);
	}

	internal static CustomAttributeData FindAttributeByTypeName(PropertyInfo pi, string attributeTypeFullName)
	{
		IList<CustomAttributeData> customAttributeData = GetCustomAttributeData(pi);
		return FindAttributeByTypeName(customAttributeData, attributeTypeFullName);
	}

	internal static CustomAttributeData FindAttributeByShortTypeName(MemberInfo memberInfo, string attributeTypeShortName)
	{
		IList<CustomAttributeData> customAttributeData = GetCustomAttributeData(memberInfo);
		return FindAttributeByShortTypeName(customAttributeData, attributeTypeShortName);
	}

	internal static IList<CustomAttributeData> GetCustomAttributeData(MethodInfo mi)
	{
		string key = mi.DeclaringType.AssemblyQualifiedName + "." + mi.Name;
		IList<CustomAttributeData> value = null;
		if (!_typeAttrs.TryGetValue(key, out value) && mi != null)
		{
			value = mi.GetCustomAttributesData();
			_typeAttrs[key] = value;
		}
		return value;
	}

	internal static IList<CustomAttributeData> GetCustomAttributeData(PropertyInfo pi)
	{
		string key = pi.DeclaringType.AssemblyQualifiedName + "." + pi.Name;
		IList<CustomAttributeData> value = null;
		if (!_typeAttrs.TryGetValue(key, out value) && pi != null)
		{
			value = pi.GetCustomAttributesData();
			_typeAttrs[key] = value;
		}
		return value;
	}

	internal static IList<CustomAttributeData> GetCustomAttributeData(MemberInfo memberInfo)
	{
		string key = memberInfo.DeclaringType.AssemblyQualifiedName + "." + memberInfo.Name;
		IList<CustomAttributeData> value = null;
		if (!_typeAttrs.TryGetValue(key, out value) && memberInfo != null)
		{
			value = memberInfo.GetCustomAttributesData();
			_typeAttrs[key] = value;
		}
		return value;
	}

	internal static IList<CustomAttributeData> GetCustomAttributeData(Type type)
	{
		if (!_typeAttrs.TryGetValue(type.AssemblyQualifiedName, out var value))
		{
			value = type.GetCustomAttributesData();
			_typeAttrs[type.AssemblyQualifiedName] = value;
		}
		return value;
	}

	internal static IEnumerable<CustomAttributeData> GetCustomAttributeData(Type type, bool inherit)
	{
		Type currentType = type;
		do
		{
			foreach (CustomAttributeData customAttributeDatum in GetCustomAttributeData(currentType))
			{
				yield return customAttributeDatum;
			}
			currentType = currentType.BaseType;
		}
		while (inherit && currentType != null);
	}

	internal static CustomAttributeData FindAttributeByTypeName(IEnumerable<CustomAttributeData> attrData, string attributeTypeFullName)
	{
		foreach (CustomAttributeData attrDatum in attrData)
		{
			Type attributeType = attrDatum.AttributeType;
			if (attributeType.FullName == attributeTypeFullName || attributeType.FullName.Replace("Windows.UI.Xaml", "Microsoft.UI.Xaml") == attributeTypeFullName)
			{
				return attrDatum;
			}
		}
		return null;
	}

	internal static CustomAttributeData FindAttributeByShortTypeName(IEnumerable<CustomAttributeData> attrData, string attributeTypeShortName)
	{
		foreach (CustomAttributeData attrDatum in attrData)
		{
			Type attributeType = attrDatum.AttributeType;
			if (attributeType.FullName.EndsWith(attributeTypeShortName))
			{
				return attrDatum;
			}
		}
		return null;
	}

	internal static object GetAttributeConstructorArgument(CustomAttributeData customAttr, int idx, string name)
	{
		if (idx >= 0 && customAttr.ConstructorArguments.Count > idx)
		{
			return customAttr.ConstructorArguments[idx].Value;
		}
		if (!string.IsNullOrEmpty(name))
		{
			foreach (CustomAttributeNamedArgument namedArgument in customAttr.NamedArguments)
			{
				if (namedArgument.MemberName == name)
				{
					return namedArgument.TypedValue.Value;
				}
			}
		}
		return null;
	}
}
