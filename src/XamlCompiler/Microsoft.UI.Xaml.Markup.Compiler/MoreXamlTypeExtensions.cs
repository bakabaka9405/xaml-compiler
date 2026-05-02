using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.Core;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal static class MoreXamlTypeExtensions
{
	private static IDictionary<string, Dictionary<string, bool>> implicitCastCache = new InstanceCache<string, Dictionary<string, bool>>();

	private static IReadOnlyDictionary<string, HashSet<string>> primitiveTypesCasting = new Dictionary<string, HashSet<string>>
	{
		{
			typeof(sbyte).FullName,
			new HashSet<string>
			{
				typeof(short).FullName,
				typeof(int).FullName,
				typeof(long).FullName,
				typeof(float).FullName,
				typeof(double).FullName
			}
		},
		{
			typeof(byte).FullName,
			new HashSet<string>
			{
				typeof(short).FullName,
				typeof(ushort).FullName,
				typeof(int).FullName,
				typeof(uint).FullName,
				typeof(long).FullName,
				typeof(ulong).FullName,
				typeof(float).FullName,
				typeof(double).FullName
			}
		},
		{
			typeof(short).FullName,
			new HashSet<string>
			{
				typeof(int).FullName,
				typeof(long).FullName,
				typeof(float).FullName,
				typeof(double).FullName
			}
		},
		{
			typeof(ushort).FullName,
			new HashSet<string>
			{
				typeof(int).FullName,
				typeof(uint).FullName,
				typeof(long).FullName,
				typeof(ulong).FullName,
				typeof(float).FullName,
				typeof(double).FullName
			}
		},
		{
			typeof(int).FullName,
			new HashSet<string>
			{
				typeof(long).FullName,
				typeof(float).FullName,
				typeof(double).FullName
			}
		},
		{
			typeof(uint).FullName,
			new HashSet<string>
			{
				typeof(long).FullName,
				typeof(ulong).FullName,
				typeof(double).FullName
			}
		},
		{
			typeof(float).FullName,
			new HashSet<string> { typeof(double).FullName }
		},
		{
			typeof(bool).FullName,
			new HashSet<string> { "Windows.Foundation.IReference`1<Boolean>" }
		}
	};

	private static IDictionary<string, Dictionary<string, LanguageSpecificString>> inlineConversionsCache = new InstanceCache<string, Dictionary<string, LanguageSpecificString>>();

	private static IReadOnlyDictionary<string, Dictionary<string, LanguageSpecificString>> inlineConversionsSupported = new Dictionary<string, Dictionary<string, LanguageSpecificString>>
	{
		{
			typeof(bool).FullName,
			new Dictionary<string, LanguageSpecificString>
			{
				{
					"Microsoft.UI.Xaml.Visibility",
					new LanguageSpecificString(() => "{0} ? ::" + KnownTypes.VisibilityColonized + "::Visible : ::" + KnownTypes.VisibilityColonized + "::Collapsed", () => "{0} ? ::winrt::" + KnownTypes.VisibilityColonized + "::Visible : ::winrt::" + KnownTypes.VisibilityColonized + "::Collapsed", () => "{0} ? global::Microsoft.UI.Xaml.Visibility.Visible : global::Microsoft.UI.Xaml.Visibility.Collapsed", () => "If({0}, Global.Microsoft.UI.Xaml.Visibility.Visible, Global.Microsoft.UI.Xaml.Visibility.Collapsed)")
				},
				{
					"Windows.Foundation.IReference`1<Boolean>",
					new LanguageSpecificString(() => "{0}")
				}
			}
		},
		{
			"System.Nullable`1<Boolean>",
			new Dictionary<string, LanguageSpecificString> { 
			{
				"Microsoft.UI.Xaml.Visibility",
				new LanguageSpecificString(() => "{0} && {0}->Value ? ::" + KnownTypes.VisibilityColonized + "::Visible : ::" + KnownTypes.VisibilityColonized + "::Collapsed", delegate
				{
					throw new NotImplementedException("Unexpected System.Nullable<Boolean> to Visibility");
				}, () => "({0} ?? false) ? global::Microsoft.UI.Xaml.Visibility.Visible : global::Microsoft.UI.Xaml.Visibility.Collapsed", () => "If(If({0}, False), Global.Microsoft.UI.Xaml.Visibility.Visible, Global.Microsoft.UI.Xaml.Visibility.Collapsed)")
			} }
		},
		{
			"Windows.Foundation.IReference`1<Boolean>",
			new Dictionary<string, LanguageSpecificString>
			{
				{
					typeof(bool).FullName,
					new LanguageSpecificString(() => "{0} ? {0}->Value : false", () => "{0} ? {0}.Value() : false", () => "{0} ?? false", () => "If({0}, False)")
				},
				{
					"Microsoft.UI.Xaml.Visibility",
					new LanguageSpecificString(() => "{0} && {0}->Value ? ::" + KnownTypes.VisibilityColonized + "::Visible : ::" + KnownTypes.VisibilityColonized + "::Collapsed", () => "{0} && {0}.Value() ? ::winrt::" + KnownTypes.VisibilityColonized + "::Visible : ::winrt::" + KnownTypes.VisibilityColonized + "::Collapsed", () => "({0} ?? false) ? global::Microsoft.UI.Xaml.Visibility.Visible : global::Microsoft.UI.Xaml.Visibility.Collapsed", () => "If(If({0}, False), Global.Microsoft.UI.Xaml.Visibility.Visible, Global.Microsoft.UI.Xaml.Visibility.Collapsed)")
				}
			}
		},
		{
			"Microsoft.UI.Xaml.Visibility",
			new Dictionary<string, LanguageSpecificString>
			{
				{
					typeof(bool).FullName,
					new LanguageSpecificString(() => "{0} == ::" + KnownTypes.VisibilityColonized + "::Visible", () => "{0} == ::winrt::" + KnownTypes.VisibilityColonized + "::Visible", () => "{0} == global::Microsoft.UI.Xaml.Visibility.Visible", () => "{0} = Global.Microsoft.UI.Xaml.Visibility.Visible")
				},
				{
					"System.Nullable`1<Boolean>",
					new LanguageSpecificString(() => "{0} == ::" + KnownTypes.VisibilityColonized + "::Visible", () => "{0} == ::winrt::" + KnownTypes.VisibilityColonized + "::Visible", () => "{0} == global::Microsoft.UI.Xaml.Visibility.Visible", () => "{0} = Global.Microsoft.UI.Xaml.Visibility.Visible")
				},
				{
					"Windows.Foundation.IReference`1<Boolean>",
					new LanguageSpecificString(() => "{0} == ::" + KnownTypes.VisibilityColonized + "::Visible", () => "{0} == ::winrt::" + KnownTypes.VisibilityColonized + "::Visible", () => "{0} == global::Microsoft.UI.Xaml.Visibility.Visible", () => "{0} = Global.Microsoft.UI.Xaml.Visibility.Visible")
				}
			}
		}
	};

	internal static IDictionary<string, Dictionary<string, LanguageSpecificString>> InlineConversionsCache
	{
		get
		{
			if (inlineConversionsCache.Count == 0)
			{
				foreach (KeyValuePair<string, Dictionary<string, LanguageSpecificString>> item in inlineConversionsSupported)
				{
					inlineConversionsCache.Add(item.Key, item.Value);
				}
			}
			return inlineConversionsCache;
		}
	}

	internal static bool ImplementsXamlINotifyPropertyChanged(this XamlType type)
	{
		if (type.ImplementsINotifyPropertyChanged())
		{
			DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
			return directUIXamlType.HasInterface("Microsoft.UI.Xaml.Data.INotifyPropertyChanged");
		}
		return false;
	}

	internal static bool ImplementsXamlINotifyDataErrorInfo(this XamlType type)
	{
		if (type.ImplementsINotifyDataErrorInfo())
		{
			DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
			return directUIXamlType.HasInterface("Microsoft.UI.Xaml.Data.INotifyDataErrorInfo");
		}
		return false;
	}

	internal static bool ImplementsXamlINotifyCollectionChanged(this XamlType type)
	{
		if (type.ImplementsINotifyCollectionChanged())
		{
			DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
			return directUIXamlType.HasInterface("Microsoft.UI.Xaml.Interop.INotifyCollectionChanged");
		}
		return false;
	}

	internal static bool ImplementsIInputValidationControl(this XamlType type)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		return directUIXamlType.HasInterface("Microsoft.UI.Xaml.Controls.IInputValidationControl");
	}

	internal static bool IsDerivedFromValidationCommand(this XamlType type)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		if (!(directUIXamlType != null))
		{
			return false;
		}
		return directUIXamlType.IsDerivedFromValidationCommand;
	}

	internal static bool IsDeprecated(this XamlType type)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		if (!(directUIXamlType != null))
		{
			return false;
		}
		return directUIXamlType.IsDeprecated;
	}

	internal static bool IsDerivedFromFrameworkTemplate(this XamlType type)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		if (!(directUIXamlType != null))
		{
			return false;
		}
		return directUIXamlType.IsDerivedFromFrameworkTemplate;
	}

	internal static bool IsDerivedFromDataTemplate(this XamlType type)
	{
		DirectUISchemaContext directUISchemaContext = type.SchemaContext as DirectUISchemaContext;
		return directUISchemaContext.DirectUISystem.DataTemplate.IsAssignableFrom(type.UnderlyingType);
	}

	internal static bool IsDerivedFromControlTemplate(this XamlType type)
	{
		DirectUISchemaContext directUISchemaContext = type.SchemaContext as DirectUISchemaContext;
		return directUISchemaContext.DirectUISystem.ControlTemplate.IsAssignableFrom(type.UnderlyingType);
	}

	internal static bool IsDerivedFromResourceDictionary(this XamlType type)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		if (!(directUIXamlType != null))
		{
			return false;
		}
		return directUIXamlType.IsDerivedFromResourceDictionary;
	}

	internal static bool IsDerivedFromUIElement(this XamlType type)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		if (!(directUIXamlType != null))
		{
			return false;
		}
		return directUIXamlType.IsDerivedFromUIElement;
	}

	internal static bool IsDerivedFromWindow(this XamlType type)
	{
		DirectUISchemaContext directUISchemaContext = type.SchemaContext as DirectUISchemaContext;
		return directUISchemaContext.DirectUISystem.Window.IsAssignableFrom(type.UnderlyingType);
	}

	internal static bool IsDerivedFromFlyoutBase(this XamlType type)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		if (!(directUIXamlType != null))
		{
			return false;
		}
		return directUIXamlType.IsDerivedFromFlyoutBase;
	}

	internal static bool IsDerivedFromMarkupExtension(this XamlType type)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		if (!(directUIXamlType != null))
		{
			return false;
		}
		return directUIXamlType.IsDerivedFromMarkupExtension;
	}

	internal static bool IsDerivedFromTextBox(this XamlType type)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		if (!(directUIXamlType != null))
		{
			return false;
		}
		return directUIXamlType.IsDerivedFromTextBox;
	}

	internal static bool IsDelegate(this XamlType type)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		if (!(directUIXamlType != null))
		{
			return false;
		}
		return directUIXamlType.IsDelegate;
	}

	internal static bool IsObject(this XamlType type)
	{
		return type.UnderlyingType.FullName == typeof(object).ToString();
	}

	internal static bool IsVoid(this XamlType type)
	{
		return type.UnderlyingType.FullName == typeof(void).ToString();
	}

	internal static bool IsBoolOrNullableBool(this XamlType xamlType)
	{
		Type underlyingType = xamlType.UnderlyingType;
		string text = (underlyingType.IsGenericType ? XamlSchemaCodeInfo.GetFullGenericNestedName(underlyingType, setAirity: true) : underlyingType.FullName);
		if (!(text == typeof(bool).FullName) && !(text == XamlSchemaCodeInfo.GetFullGenericNestedName(typeof(bool?), setAirity: true)))
		{
			return text == "Windows.Foundation.IReference`1<Boolean>";
		}
		return true;
	}

	private static DirectUIXamlType GetDirectUIXamlType(XamlType type)
	{
		return type as DirectUIXamlType;
	}

	private static bool CacheImplicitCast(Type source, Type target, bool castable)
	{
		string key = (source.IsGenericType ? XamlSchemaCodeInfo.GetFullGenericNestedName(source, setAirity: true) : source.FullName);
		string key2 = (target.IsGenericType ? XamlSchemaCodeInfo.GetFullGenericNestedName(target, setAirity: true) : target.FullName);
		if (!implicitCastCache.ContainsKey(key))
		{
			implicitCastCache.Add(key, new Dictionary<string, bool>());
		}
		if (!implicitCastCache[key].ContainsKey(key2))
		{
			implicitCastCache[key].Add(key2, castable);
		}
		return castable;
	}

	private static bool IsImplicitlyCastableTo(Type source, Type target)
	{
		string key = (source.IsGenericType ? XamlSchemaCodeInfo.GetFullGenericNestedName(source, setAirity: true) : source.FullName);
		string text = (target.IsGenericType ? XamlSchemaCodeInfo.GetFullGenericNestedName(target, setAirity: true) : target.FullName);
		if (implicitCastCache.ContainsKey(key) && implicitCastCache[key].ContainsKey(text))
		{
			return implicitCastCache[key][text];
		}
		if (primitiveTypesCasting.ContainsKey(key) && primitiveTypesCasting[key].Contains(text))
		{
			return CacheImplicitCast(source, target, castable: true);
		}
		MethodInfo[] methods = source.GetMethods(BindingFlags.Static | BindingFlags.Public);
		MethodInfo[] array = methods;
		foreach (MethodInfo methodInfo in array)
		{
			if (methodInfo.ReturnType == target && methodInfo.Name.Equals("op_Implicit"))
			{
				return CacheImplicitCast(source, target, castable: true);
			}
		}
		methods = target.GetMethods(BindingFlags.Static | BindingFlags.Public);
		MethodInfo[] array2 = methods;
		foreach (MethodInfo methodInfo2 in array2)
		{
			if (methodInfo2.ReturnType == target && methodInfo2.Name.Equals("op_Implicit"))
			{
				ParameterInfo[] parameters = methodInfo2.GetParameters();
				if (parameters[0].ParameterType == source)
				{
					return CacheImplicitCast(source, target, castable: true);
				}
			}
		}
		return CacheImplicitCast(source, target, castable: false);
	}

	internal static bool CanAssignDirectlyTo(this XamlType source, XamlType target)
	{
		return source.CanAssignDirectlyTo(target.UnderlyingType);
	}

	internal static bool CanAssignDirectlyTo(this XamlType source, Type targetType)
	{
		return source.UnderlyingType.CanAssignDirectlyTo(targetType);
	}

	internal static bool CanAssignDirectlyTo(this Type sourceType, Type targetType)
	{
		if (!sourceType.CanAssignDirectlyWithNoImplicitCast(targetType))
		{
			return IsImplicitlyCastableTo(sourceType, targetType);
		}
		return true;
	}

	internal static bool CanAssignDirectlyWithNoImplicitCast(this XamlType source, Type targetType)
	{
		return source.UnderlyingType.CanAssignDirectlyWithNoImplicitCast(targetType);
	}

	internal static bool CanAssignDirectlyWithNoImplicitCast(this Type sourceType, Type targetType)
	{
		if (!(sourceType.FullName == targetType.FullName))
		{
			return targetType.IsAssignableFrom(sourceType);
		}
		return true;
	}

	internal static bool CanBoxTo(this XamlType source, XamlType target)
	{
		Type underlyingType = source.UnderlyingType;
		string key = (underlyingType.IsGenericType ? XamlSchemaCodeInfo.GetFullGenericNestedName(underlyingType, setAirity: true) : underlyingType.FullName);
		Type underlyingType2 = target.UnderlyingType;
		string key2 = (underlyingType2.IsGenericType ? XamlSchemaCodeInfo.GetFullGenericNestedName(underlyingType2, setAirity: true) : underlyingType2.FullName);
		LanguageSpecificString languageSpecificString = null;
		Dictionary<string, LanguageSpecificString> dictionary = null;
		if (target.IsBoxedType())
		{
			Type type = underlyingType2.GetGenericArguments()[0];
			XamlType xamlType = target.SchemaContext.GetXamlType(type);
			LanguageSpecificString languageSpecificString2 = null;
			if (!underlyingType.CanAssignDirectlyTo(type) && source.CanInlineConvert(xamlType))
			{
				languageSpecificString2 = source.GetInlineConversionFormats(xamlType);
			}
			if (!xamlType.CanAssignDirectlyTo(target))
			{
				string arg = xamlType.CppCXName();
				string arg2 = xamlType.CSharpName();
				string arg3 = xamlType.VBName();
				if (languageSpecificString2 == null)
				{
					string cppWinRTCast;
					if (underlyingType.FullName.Equals(type.FullName))
					{
						cppWinRTCast = "{0}";
					}
					else
					{
						cppWinRTCast = source.CppWinRTCast(xamlType, "{0}");
					}
					languageSpecificString2 = new LanguageSpecificString(() => "{0}", () => cppWinRTCast, () => "{0}", () => "{0}");
				}
				string cppBoxedValue = $"ref new ::Platform::Box<{arg}>({languageSpecificString2.CppCXName()})";
				string cSharpBoxedValue = $"new global::System.Nullable<{arg2}>({languageSpecificString2.CSharpName()})";
				string vbBoxedValue = $"New Global.System.Nullable (Of {arg3})({languageSpecificString2.VBName()})";
				string cppWinRTBoxedValue = $"winrt::box_value({languageSpecificString2.CppWinRTName()}).as<{target.CppWinRTName()}>()";
				languageSpecificString = new LanguageSpecificString(() => cppBoxedValue, () => cppWinRTBoxedValue, () => cSharpBoxedValue, () => vbBoxedValue);
			}
		}
		if (InlineConversionsCache.ContainsKey(key))
		{
			dictionary = InlineConversionsCache[key];
		}
		if (dictionary == null)
		{
			InlineConversionsCache.Add(key, new Dictionary<string, LanguageSpecificString>());
			dictionary = InlineConversionsCache[key];
		}
		if (!dictionary.ContainsKey(key2) && languageSpecificString != null)
		{
			dictionary.Add(key2, languageSpecificString);
		}
		return languageSpecificString != null;
	}

	internal static bool CanInlineConvert(this XamlType source, XamlType target)
	{
		Type underlyingType = source.UnderlyingType;
		string text = (underlyingType.IsGenericType ? XamlSchemaCodeInfo.GetFullGenericNestedName(underlyingType, setAirity: true) : underlyingType.FullName);
		Type underlyingType2 = target.UnderlyingType;
		string key = (underlyingType2.IsGenericType ? XamlSchemaCodeInfo.GetFullGenericNestedName(underlyingType2, setAirity: true) : underlyingType2.FullName);
		Dictionary<string, LanguageSpecificString> dictionary = null;
		if (InlineConversionsCache.ContainsKey(text))
		{
			dictionary = InlineConversionsCache[text];
			if (dictionary.ContainsKey(key))
			{
				return dictionary[key] != null;
			}
		}
		bool flag = false;
		if (underlyingType2.IsSubclassOf(underlyingType) || underlyingType.IsAssignableFrom(underlyingType2))
		{
			flag = true;
		}
		if (!flag && primitiveTypesCasting.ContainsKey(key))
		{
			HashSet<string> hashSet = primitiveTypesCasting[key];
			if (hashSet.Contains(text))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			MethodInfo[] methods = underlyingType.GetMethods(BindingFlags.Static | BindingFlags.Public);
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				if (methodInfo.ReturnType == underlyingType2 && methodInfo.Name.Equals("op_Explicit"))
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			MethodInfo[] methods2 = underlyingType2.GetMethods(BindingFlags.Static | BindingFlags.Public);
			MethodInfo[] array2 = methods2;
			foreach (MethodInfo methodInfo2 in array2)
			{
				if (methodInfo2.ReturnType == underlyingType2 && methodInfo2.Name.Equals("op_Explicit"))
				{
					ParameterInfo[] parameters = methodInfo2.GetParameters();
					if (parameters[0].ParameterType == underlyingType)
					{
						flag = true;
						break;
					}
				}
			}
		}
		LanguageSpecificString languageSpecificString = null;
		if (flag)
		{
			string targetCppName = target.CppCXName();
			string text2 = target.CppWinRTName();
			string targetCSharpName = target.CSharpName();
			string targeVbName = target.VBName();
			string sourceVbCastName = source.GetVBCastName(target.UnderlyingType);
			string cppWinRTCast = source.CppWinRTCast(target, "{0}");
			languageSpecificString = new LanguageSpecificString(() => $"safe_cast<{targetCppName}>({{0}})", () => cppWinRTCast, () => $"({targetCSharpName}){{0}}", () => string.Format("{1}({{0}}, {0})", targeVbName, sourceVbCastName));
		}
		if (source.CanBoxTo(target) || InlineConversionsCache.ContainsKey(text))
		{
			dictionary = InlineConversionsCache[text];
		}
		if (dictionary == null)
		{
			InlineConversionsCache.Add(text, new Dictionary<string, LanguageSpecificString>());
			dictionary = InlineConversionsCache[text];
		}
		if (!dictionary.ContainsKey(key))
		{
			dictionary.Add(key, languageSpecificString);
		}
		if (languageSpecificString == null && dictionary.ContainsKey(key))
		{
			languageSpecificString = dictionary[key];
		}
		return languageSpecificString != null;
	}

	public static LanguageSpecificString GetInlineConversionExpression(this XamlType source, XamlType target, LanguageSpecificString memberExpression)
	{
		LanguageSpecificString expressions = source.GetInlineConversionFormats(target);
		return new LanguageSpecificString(() => (!(expressions != null)) ? ("safe_cast<" + target.CppCXName() + ">(" + memberExpression.CppCXName() + ")") : string.Format(expressions.CppCXName(), memberExpression.CppCXName()), () => (!(expressions != null)) ? source.CppWinRTCast(target, memberExpression.CppWinRTName()) : string.Format(expressions.CppWinRTName(), memberExpression.CppWinRTName()), () => (!(expressions != null)) ? ("(" + target.CSharpName() + ")" + memberExpression.CSharpName()) : string.Format(expressions.CSharpName(), memberExpression.CSharpName()), () => (!(expressions != null)) ? (source.GetVBCastName(target.UnderlyingType) + "(" + memberExpression.VBName() + ", " + target.VBName() + ")") : string.Format(expressions.VBName(), memberExpression.VBName()));
	}

	internal static LanguageSpecificString GetInlineConversionFormats(this XamlType source, XamlType target)
	{
		Type underlyingType = source.UnderlyingType;
		string key = (underlyingType.IsGenericType ? XamlSchemaCodeInfo.GetFullGenericNestedName(underlyingType, setAirity: true) : underlyingType.FullName);
		Type underlyingType2 = target.UnderlyingType;
		string key2 = (underlyingType2.IsGenericType ? XamlSchemaCodeInfo.GetFullGenericNestedName(underlyingType2, setAirity: true) : underlyingType2.FullName);
		if (InlineConversionsCache.ContainsKey(key))
		{
			Dictionary<string, LanguageSpecificString> dictionary = InlineConversionsCache[key];
			if (dictionary.ContainsKey(key2))
			{
				return dictionary[key2];
			}
		}
		return null;
	}

	internal static bool IsEnum(this XamlType source)
	{
		return source.UnderlyingType.IsEnum;
	}

	internal static IEnumerable<string> GetEnumNames(this XamlType source)
	{
		if (source.IsEnum())
		{
			string[] names = Enum.GetNames(source.UnderlyingType);
			for (int i = 0; i < names.Length; i++)
			{
				yield return names[i];
			}
		}
	}

	public static bool IsNullExtension(this XamlType instance)
	{
		if (instance.SchemaContext is DirectUISchemaContext directUISchemaContext && instance.CanAssignTo(directUISchemaContext.DirectUIXamlLanguage.NullExtension))
		{
			return true;
		}
		return false;
	}

	public static string GetVBCastName(this XamlType source, Type targetType)
	{
		if (!source.CanAssignDirectlyWithNoImplicitCast(targetType))
		{
			return "CType";
		}
		return "DirectCast";
	}

	public static bool IsContractVersionAttribute(this Type type)
	{
		return type.FullName.Equals("Windows.Foundation.Metadata.ContractVersionAttribute");
	}

	internal static bool HasCreateFromStringMethod(this XamlType type)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		if (!(directUIXamlType != null))
		{
			return false;
		}
		return directUIXamlType.CreateFromStringMethod.Exists;
	}

	internal static bool HasMember(this XamlType type, string name)
	{
		DirectUIXamlType directUIXamlType = GetDirectUIXamlType(type);
		if (!(directUIXamlType != null))
		{
			return false;
		}
		return directUIXamlType.HasMember(name);
	}

	internal static CreateFromStringMethod GetCreateFromStringMethod(this XamlType type)
	{
		return GetDirectUIXamlType(type)?.CreateFromStringMethod;
	}

	internal static XamlCompileError EnsureCreateFromStringResolved(this DirectUISchemaContext schemaContext, string declaringTypeName, CreateFromStringMethod createMethod, XamlDomNode locationForErrors)
	{
		if (!createMethod.Resolved)
		{
			string unresolvedName = createMethod.UnresolvedName;
			if (!unresolvedName.Contains("."))
			{
				return new XamlValidationCreateFromStringError(declaringTypeName, unresolvedName, XamlCompilerResources.CreateFromString_MethodOnTypeNotFound, locationForErrors);
			}
			string methodName = unresolvedName.Substring(unresolvedName.LastIndexOf('.') + 1);
			string fullName = unresolvedName.Substring(0, unresolvedName.LastIndexOf('.'));
			if (string.IsNullOrEmpty(methodName))
			{
				return new XamlValidationCreateFromStringError(declaringTypeName, unresolvedName, XamlCompilerResources.CreateFromString_MethodOnTypeNotFound, locationForErrors);
			}
			XamlType xamlType = schemaContext.GetXamlType(fullName);
			if (xamlType == null)
			{
				return new XamlValidationCreateFromStringError(declaringTypeName, unresolvedName, XamlCompilerResources.CreateFromString_TypeNotFound, locationForErrors);
			}
			MethodInfo[] methods = xamlType.UnderlyingType.GetMethods(BindingFlags.Static | BindingFlags.Public);
			bool flag = false;
			bool flag2 = false;
			foreach (MethodInfo item in methods.Where((MethodInfo m) => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)))
			{
				flag = true;
				ParameterInfo[] parameters = item.GetParameters();
				if (parameters.Length == 1 && parameters[0].ParameterType.FullName.Equals(typeof(string).FullName, StringComparison.OrdinalIgnoreCase) && !parameters[0].IsOut)
				{
					flag2 = true;
					createMethod.SetResolved(xamlType, methodName, item);
					break;
				}
			}
			if (!flag)
			{
				return new XamlValidationCreateFromStringError(declaringTypeName, unresolvedName, XamlCompilerResources.CreateFromString_MethodOnTypeNotFound, locationForErrors);
			}
			if (flag && !flag2)
			{
				return new XamlValidationCreateFromStringError(declaringTypeName, unresolvedName, XamlCompilerResources.CreateFromString_InvalidMethodSignature, locationForErrors);
			}
		}
		return null;
	}

	public static string NameWithApiInformation(this XamlMember member)
	{
		DirectUIXamlMember directUIXamlMember = member as DirectUIXamlMember;
		if (directUIXamlMember?.ApiInformation != null)
		{
			return $"{member.ToString()}?{directUIXamlMember.ApiInformation.UniqueName}";
		}
		return member.ToString();
	}

	public static LanguageSpecificString GetStringToThing(this XamlType type, string valueName, bool isLiteral = false)
	{
		return type.GetStringToThing(new LanguageSpecificString(() => valueName), isLiteral);
	}

	public static LanguageSpecificString GetStringToThing(this XamlType type, LanguageSpecificString valueName, bool isLiteral = false)
	{
		if (type.IsBoxedType())
		{
			XamlType xamlType = type.SchemaContext.GetXamlType(type.GetBoxedType());
			LanguageSpecificString stringToThing = xamlType.GetStringToThing(valueName, isLiteral);
			if (xamlType.CanAssignDirectlyTo(type))
			{
				return stringToThing;
			}
			if (xamlType.CanInlineConvert(type))
			{
				return xamlType.GetInlineConversionExpression(type, stringToThing);
			}
			throw new NotImplementedException("Couldn't convert boxed type " + xamlType.UnderlyingType.FullName + " to " + type.UnderlyingType.FullName);
		}
		string cppWinRTValue = (isLiteral ? ("L" + valueName.CppWinRTName()) : valueName.CppWinRTName());
		if (!type.HasCreateFromStringMethod())
		{
			return new LanguageSpecificString(() => "(" + type.CppCXName() + ") ::" + KnownTypes.XamlBindingHelperColonized + "::ConvertValue(" + type.CppCXName(IncludeHatIfApplicable: false) + "::typeid, " + valueName.CppCXName() + ")", () => (!type.NeedsBoxUnbox()) ? type.SchemaContext.GetXamlType(typeof(object)).CppWinRTCast(type, "::winrt::" + KnownTypes.XamlBindingHelperColonized + "::ConvertValue(::winrt::xaml_typename<" + type.CppWinRTName() + ">(), ::winrt::box_value(::winrt::hstring(" + cppWinRTValue + ")))") : ("::winrt::unbox_value<" + type.CppWinRTName() + ">(::winrt::" + KnownTypes.XamlBindingHelperColonized + "::ConvertValue(::winrt::xaml_typename<" + type.CppWinRTName() + ">(), ::winrt::box_value(::winrt::hstring(" + cppWinRTValue + "))))"), () => "(" + type.CSharpName() + ") global::Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(" + type.CSharpName() + "), " + valueName.CSharpName() + ")", () => "DirectCast(Global.Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(GetType(" + type.VBName() + "), " + valueName.VBName() + "), " + type.VBName() + ")");
		}
		CreateFromStringMethod createFromString = type.GetCreateFromStringMethod();
		if (createFromString.MethodInfo.ReturnType.CanAssignDirectlyTo(type.UnderlyingType))
		{
			return new LanguageSpecificString(() => $"{createFromString.ResolvedName.CppCXName()}({valueName.CppCXName()})", () => $"{createFromString.ResolvedName.CppWinRTName()}({valueName.CppWinRTName()})", () => $"{createFromString.ResolvedName.CSharpName()}({valueName.CSharpName()})", () => $"{createFromString.ResolvedName.VBName()}({valueName.VBName()})");
		}
		return new LanguageSpecificString(() => $"({type.CppCXName()}){createFromString.ResolvedName.CppCXName()}({valueName.CppCXName()})", () => "::winrt::unbox_value<" + type.CppWinRTName() + ">(" + createFromString.ResolvedName.CppWinRTName() + "(" + cppWinRTValue + "))", () => $"({type.CSharpName()}){createFromString.ResolvedName.CSharpName()}({valueName.CSharpName()})", () => string.Format("DirectCast({1}({2}), {0})", type.VBName(), createFromString.ResolvedName.VBName(), valueName.VBName()));
	}

	public static LanguageSpecificString ToStringWithNullCheckExpression(this XamlType type, LanguageSpecificString expression)
	{
		if (type.UnderlyingType.IsValueType)
		{
			return new LanguageSpecificString(() => expression.CppCXName() + ".ToString()", () => "::winrt::to_hstring(" + expression.CppWinRTName() + ")", () => expression.CSharpName() + ".ToString()", () => expression.VBName() + ".ToString()");
		}
		return new LanguageSpecificString(() => expression.CppCXName() + " != nullptr ? " + expression.CppCXName() + "->ToString() : nullptr", () => "::winrt::to_hstring(" + expression.CppWinRTName() + ")", () => expression.CSharpName() + " != null ? " + expression.CSharpName() + ".ToString() : null", () => "If(" + expression.VBName() + " IsNot Nothing, " + expression.VBName() + ".ToString(), Nothing)");
	}

	public static bool NeedsBoxUnbox(this XamlType instance)
	{
		if (!instance.UnderlyingType.IsValueType)
		{
			return instance.IsString();
		}
		return true;
	}

	public static string CppWinRTCast(this XamlType source, XamlType target, string expression)
	{
		if (source.UnderlyingType.IsPrimitive && target.UnderlyingType.IsPrimitive)
		{
			return "static_cast<" + target.CppWinRTName() + ">(" + expression + ")";
		}
		if (target.IsString() && !source.IsString())
		{
			return "::winrt::to_hstring(" + expression + ")";
		}
		if (!source.UnderlyingType.IsPrimitive && !target.UnderlyingType.IsPrimitive)
		{
			return expression + ".as<" + target.CppWinRTName() + ">()";
		}
		return "unknown_cast<" + target.CppWinRTName() + ">(" + expression + ")";
	}

	public static string CppWinRTLocalElseRef(this XamlType type)
	{
		if (DomHelper.IsLocalType(type))
		{
			return type.CppWinRTName().ToLocalCppWinRTTypeName();
		}
		return type.CppWinRTName();
	}

	public static bool HasFullXamlMetadataProviderAttribute(this Type type)
	{
		return Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ReflectionHelper.GetCustomAttributeData(type, inherit: false, "Microsoft.UI.Xaml.Markup.FullXamlMetadataProviderAttribute").Any();
	}

	public static bool IsBoxedType(this XamlType type)
	{
		return type.UnderlyingType.IsBoxedType();
	}

	public static bool IsBoxedType(this Type type)
	{
		string fullName = type.FullName;
		if (!fullName.StartsWith("System.Nullable"))
		{
			return fullName.StartsWith("Windows.Foundation.IReference");
		}
		return true;
	}

	public static Type GetBoxedType(this XamlType type)
	{
		return type.UnderlyingType.GetBoxedType();
	}

	public static Type GetBoxedType(this Type type)
	{
		return type.GetGenericArguments()[0];
	}

	public static string TryGetInputPropertyName(this XamlType type)
	{
		string result = null;
		CustomAttributeData customAttributeData = Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ReflectionHelper.GetCustomAttributeData(type.UnderlyingType, inherit: false, "Microsoft.UI.Xaml.Controls.InputPropertyAttribute").SingleOrDefault();
		if (customAttributeData != null)
		{
			result = customAttributeData.NamedArguments[0].TypedValue.Value.ToString();
		}
		return result;
	}
}
