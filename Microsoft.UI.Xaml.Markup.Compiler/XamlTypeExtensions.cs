using System;
using System.Reflection;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler;

public static class XamlTypeExtensions
{
	internal static bool IsFloat(this XamlType source)
	{
		return source.UnderlyingType.FullName == typeof(float).FullName;
	}

	internal static bool IsString(this XamlType source)
	{
		return source.UnderlyingType.FullName == typeof(string).FullName;
	}

	internal static bool IsIntegerIndexable(this XamlType source)
	{
		if (source.IsArray || source.IsCollection)
		{
			return source.ItemType != null;
		}
		return false;
	}

	internal static bool IsStringIndexable(this XamlType source)
	{
		if (source.IsDictionary)
		{
			return source.ItemType != null;
		}
		return false;
	}

	public static bool ImplementsINotifyPropertyChanged(this XamlType type)
	{
		if (type is IXamlTypeMeta xamlTypeMeta)
		{
			return xamlTypeMeta?.ImplementsINotifyPropertyChanged ?? false;
		}
		throw new ArgumentException("ImplementsINotifyPropertyChanged: XamlType does not have metadata", "type");
	}

	public static bool ImplementsINotifyCollectionChanged(this XamlType type)
	{
		if (type is IXamlTypeMeta xamlTypeMeta)
		{
			return xamlTypeMeta?.ImplementsINotifyCollectionChanged ?? false;
		}
		throw new ArgumentException("ImplementsINotifyCollectionChanged: XamlType does not have metadata", "type");
	}

	public static bool ImplementsIObservableVector(this XamlType type)
	{
		if (type is IXamlTypeMeta xamlTypeMeta)
		{
			return xamlTypeMeta?.ImplementsIObservableVector ?? false;
		}
		throw new ArgumentException("ImplementsIObservableVector: XamlType does not have metadata", "type");
	}

	public static bool ImplementsIObservableMap(this XamlType type)
	{
		if (type is IXamlTypeMeta xamlTypeMeta)
		{
			return xamlTypeMeta?.ImplementsIObservableMap ?? false;
		}
		throw new ArgumentException("ImplementsIObservableMap: XamlType does not have metadata", "type");
	}

	public static bool ImplementsINotifyDataErrorInfo(this XamlType type)
	{
		if (type is IXamlTypeMeta xamlTypeMeta)
		{
			return xamlTypeMeta?.ImplementsINotifyDataErrorInfo ?? false;
		}
		throw new ArgumentException("ImplementsINotifyDataErrorInfo: XamlType does not have metadata", "type");
	}

	private static bool InheritsFromNamedType(this Type type, string inheritedTypeName)
	{
		Type type2 = type;
		while (type2 != null && type2 != type2.BaseType)
		{
			if (type2.FullName.Equals(inheritedTypeName))
			{
				return true;
			}
			type2 = type2.BaseType;
		}
		return false;
	}

	public static bool IsDependencyProperty(this Type declaringType, string propertyName)
	{
		string name = propertyName + "Property";
		BindingFlags bindingAttr = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
		if (declaringType.InheritsFromNamedType("Microsoft.UI.Xaml.DependencyObject"))
		{
			PropertyInfo property = declaringType.GetProperty(name, bindingAttr);
			if (property != null)
			{
				return property.PropertyType.InheritsFromNamedType("Microsoft.UI.Xaml.DependencyProperty");
			}
			FieldInfo field = declaringType.GetField(name, bindingAttr);
			if (field != null)
			{
				return field.FieldType.InheritsFromNamedType("Microsoft.UI.Xaml.DependencyProperty");
			}
		}
		return false;
	}
}
