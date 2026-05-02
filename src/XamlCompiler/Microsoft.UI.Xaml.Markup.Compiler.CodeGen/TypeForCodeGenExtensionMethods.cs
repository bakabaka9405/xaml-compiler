using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

public static class TypeForCodeGenExtensionMethods
{
	private static Dictionary<XamlType, TypeForCodeGen> cache = new Dictionary<XamlType, TypeForCodeGen>();

	private static TypeForCodeGen GetTypeForCodeGen(this XamlType type)
	{
		TypeForCodeGen typeForCodeGen;
		if (cache.ContainsKey(type))
		{
			typeForCodeGen = cache[type];
		}
		else
		{
			typeForCodeGen = new TypeForCodeGen(type);
			cache.Add(type, typeForCodeGen);
		}
		return typeForCodeGen;
	}

	public static string CSharpName(this XamlType type)
	{
		return type.GetTypeForCodeGen().FullName.CSharpName();
	}

	public static string CppCXName(this XamlType type, bool IncludeHatIfApplicable = true)
	{
		TypeForCodeGen typeForCodeGen = type.GetTypeForCodeGen();
		if (type.UnderlyingType.IsValueType || !IncludeHatIfApplicable)
		{
			return typeForCodeGen.FullName.CppCXName();
		}
		return typeForCodeGen.FullName.CppCXName() + "^";
	}

	public static string CppWinRTName(this XamlType type)
	{
		return type.GetTypeForCodeGen().FullName.CppWinRTName();
	}

	public static string MemberFriendlyName(this XamlType type)
	{
		return type.GetTypeForCodeGen().MemberFriendlyName;
	}

	public static string VBName(this XamlType type)
	{
		return type.GetTypeForCodeGen().FullName.VBName();
	}

	public static IEnumerable<Parameter> TryGetInvokeParameters(this Type multicastDelegate)
	{
		return multicastDelegate.GetInvokeParameters(throwOnError: false);
	}

	public static IEnumerable<Parameter> GetInvokeParameters(this Type multicastDelegate, bool throwOnError = true)
	{
		if (multicastDelegate.BaseType.FullName != "System.MulticastDelegate")
		{
			if (throwOnError)
			{
				throw new ArgumentException("Type '" + multicastDelegate.BaseType.FullName + "' is not a multi cast delegate.");
			}
			return null;
		}
		MethodInfo method = multicastDelegate.GetMethod("Invoke");
		if (method == null)
		{
			if (throwOnError)
			{
				throw new ArgumentException("Type '" + multicastDelegate.BaseType.FullName + "' does not have an Invoke method.");
			}
			return null;
		}
		return from p in method.GetParameters()
			select new Parameter(p);
	}

	public static string ForCall(this IEnumerable<Parameter> parameters)
	{
		return parameters.Select((Parameter p) => p.Name).ToCommaSeparatedValues();
	}

	public static LanguageSpecificString Declaration(this IEnumerable<Parameter> parameters)
	{
		IEnumerable<string> cppDeclarations = parameters.Select((Parameter p) => XamlSchemaCodeInfo.GetFullGenericNestedName(p.ParameterType, "CppWinRT", globalized: true) + " const& " + p.Name);
		IEnumerable<string> csDeclarations = parameters.Select((Parameter p) => XamlSchemaCodeInfo.GetFullGenericNestedName(p.ParameterType, "C#", globalized: true) + " " + p.Name);
		IEnumerable<string> vbDeclarations = parameters.Select((Parameter p) => p.Name + " As " + XamlSchemaCodeInfo.GetFullGenericNestedName(p.ParameterType, "VB", globalized: true));
		IEnumerable<string> cxDeclarations = parameters.Select((Parameter p) => string.Format("{0}{1} {2}", XamlSchemaCodeInfo.GetFullGenericNestedName(p.ParameterType, "C++", globalized: true), p.ParameterType.IsValueType ? "" : " ^ ", p.Name));
		return new LanguageSpecificString(() => cxDeclarations.ToCommaSeparatedValues(), () => cppDeclarations.ToCommaSeparatedValues(), () => csDeclarations.ToCommaSeparatedValues(), () => vbDeclarations.ToCommaSeparatedValues());
	}
}
