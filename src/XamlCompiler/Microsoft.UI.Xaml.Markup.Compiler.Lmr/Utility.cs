using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Adds;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal static class Utility
{
	public static bool Compare(string string1, string string2, bool ignoreCase)
	{
		return string.Equals(string1, string2, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}

	public static bool IsBindingFlagsMatching(MethodBase method, bool isInherited, BindingFlags bindingFlags)
	{
		return IsBindingFlagsMatching(method, method.IsStatic, method.IsPublic, isInherited, bindingFlags);
	}

	public static bool IsBindingFlagsMatching(FieldInfo fieldInfo, bool isInherited, BindingFlags bindingFlags)
	{
		return IsBindingFlagsMatching(fieldInfo, fieldInfo.IsStatic, fieldInfo.IsPublic, isInherited, bindingFlags);
	}

	public static bool IsBindingFlagsMatching(MemberInfo memberInfo, bool isStatic, bool isPublic, bool isInherited, BindingFlags bindingFlags)
	{
		if ((bindingFlags & BindingFlags.DeclaredOnly) != 0 && isInherited)
		{
			return false;
		}
		if (isPublic)
		{
			if ((bindingFlags & BindingFlags.Public) == 0)
			{
				return false;
			}
		}
		else if ((bindingFlags & BindingFlags.NonPublic) == 0)
		{
			return false;
		}
		if (memberInfo.MemberType != MemberTypes.TypeInfo && memberInfo.MemberType != MemberTypes.NestedType)
		{
			if (isStatic)
			{
				if ((bindingFlags & BindingFlags.FlattenHierarchy) == 0 && isInherited)
				{
					return false;
				}
				if ((bindingFlags & BindingFlags.Static) == 0)
				{
					return false;
				}
			}
			else if ((bindingFlags & BindingFlags.Instance) == 0)
			{
				return false;
			}
		}
		return true;
	}

	internal static string GetNamespaceHelper(string fullName)
	{
		if (fullName.Contains("."))
		{
			int length = fullName.LastIndexOf('.');
			return fullName.Substring(0, length);
		}
		return null;
	}

	internal static string GetTypeNameFromFullNameHelper(string fullName, bool isNested)
	{
		if (isNested)
		{
			int num = fullName.LastIndexOf('+');
			return fullName.Substring(num + 1);
		}
		int num2 = fullName.LastIndexOf('.');
		return fullName.Substring(num2 + 1);
	}

	internal static void VerifyNotByRef(MetadataOnlyCommonType type)
	{
		if (type.IsByRef)
		{
			string arg = type.Name + "&";
			throw new TypeLoadException(string.Format(CultureInfo.InvariantCulture, Resources.CannotFindTypeInModule, arg, type.Resolver.ToString()));
		}
	}

	internal static bool IsValidPath(string modulePath)
	{
		if (string.IsNullOrEmpty(modulePath))
		{
			return false;
		}
		char[] invalidPathChars = Path.GetInvalidPathChars();
		foreach (char c in invalidPathChars)
		{
			foreach (char c2 in modulePath)
			{
				if (c == c2)
				{
					return false;
				}
			}
		}
		try
		{
			if (!Path.IsPathRooted(modulePath))
			{
				return false;
			}
		}
		catch (Exception)
		{
			throw;
		}
		return true;
	}
}
