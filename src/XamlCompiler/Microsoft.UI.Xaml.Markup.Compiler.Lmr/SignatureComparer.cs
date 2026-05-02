using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.UI.Xaml.Markup.Compiler.Lmr;

internal static class SignatureComparer
{
	private const BindingFlags MembersDeclaredOnTypeOnly = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	public static IEnumerable<MethodBase> FilterMethods(MethodFilter filter, MethodInfo[] allMethods)
	{
		List<MethodBase> list = new List<MethodBase>();
		CallingConventions reflectionCallingConvention = SignatureUtil.GetReflectionCallingConvention(filter.CallingConvention);
		foreach (MethodInfo methodInfo in allMethods)
		{
			if (methodInfo.Name.Equals(filter.Name, StringComparison.Ordinal) && SignatureUtil.IsCallingConventionMatch(methodInfo, reflectionCallingConvention) && SignatureUtil.IsGenericParametersCountMatch(methodInfo, filter.GenericParameterCount) && methodInfo.GetParameters().Length == filter.ParameterCount)
			{
				list.Add(methodInfo);
			}
		}
		return list;
	}

	public static IEnumerable<MethodBase> FilterConstructors(MethodFilter filter, ConstructorInfo[] allConstructors)
	{
		List<MethodBase> list = new List<MethodBase>();
		CallingConventions reflectionCallingConvention = SignatureUtil.GetReflectionCallingConvention(filter.CallingConvention);
		foreach (ConstructorInfo constructorInfo in allConstructors)
		{
			if (constructorInfo.Name.Equals(filter.Name, StringComparison.Ordinal) && SignatureUtil.IsCallingConventionMatch(constructorInfo, reflectionCallingConvention) && constructorInfo.GetParameters().Length == filter.ParameterCount)
			{
				list.Add(constructorInfo);
			}
		}
		return list;
	}

	internal static bool IsParametersTypeMatch(MethodBase templateMethod, TypeSignatureDescriptor[] parameters)
	{
		ParameterInfo[] parameters2 = templateMethod.GetParameters();
		int num = parameters2.Length;
		for (int i = 0; i < num; i++)
		{
			if (!parameters[i].Type.Equals(parameters2[i].ParameterType))
			{
				return false;
			}
		}
		return true;
	}

	public static MethodBase FindMatchingMethod(string methodName, Type typeToInspect, MethodSignatureDescriptor expectedSignature, GenericContext context)
	{
		bool flag = methodName.Equals(".ctor", StringComparison.Ordinal) || methodName.Equals(".cctor", StringComparison.Ordinal);
		int genericParameterCount = expectedSignature.GenericParameterCount;
		IEnumerable<MethodBase> enumerable = null;
		MethodFilter filter = new MethodFilter(methodName, genericParameterCount, expectedSignature.Parameters.Length, expectedSignature.CallingConvention);
		enumerable = ((!flag) ? FilterMethods(filter, typeToInspect.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) : FilterConstructors(filter, typeToInspect.GetConstructors(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)));
		MethodBase result = null;
		bool flag2 = false;
		foreach (MethodBase item in enumerable)
		{
			MethodBase methodBase = item;
			bool flag3 = false;
			if (genericParameterCount > 0 && context.MethodArgs.Length != 0)
			{
				methodBase = (item as MethodInfo).MakeGenericMethod(context.MethodArgs);
				flag3 = true;
			}
			MethodBase methodBase2 = null;
			methodBase2 = (typeToInspect.IsGenericType ? GetTemplateMethod(typeToInspect, methodBase.MetadataToken) : ((!flag3) ? methodBase : item));
			if ((flag || expectedSignature.ReturnParameter.Type.Equals((methodBase2 as MethodInfo).ReturnType)) && IsParametersTypeMatch(methodBase2, expectedSignature.Parameters))
			{
				if (flag2)
				{
					throw new AmbiguousMatchException();
				}
				result = methodBase;
				flag2 = true;
			}
		}
		return result;
	}

	private static MethodBase GetTemplateMethod(Type typeToInspect, int methodToken)
	{
		return typeToInspect.GetGenericTypeDefinition().Module.ResolveMethod(methodToken);
	}
}
