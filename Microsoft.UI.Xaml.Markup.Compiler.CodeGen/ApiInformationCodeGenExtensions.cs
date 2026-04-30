using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.Core;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal static class ApiInformationCodeGenExtensions
{
	private static Dictionary<ApiInformation, IApiInformationCodeGen> apiInformationCache = new InstanceCache<ApiInformation, IApiInformationCodeGen>();

	private static Dictionary<ApiInformationMethod, IApiInformationCodeGen> apiInformationMethodCache = new InstanceCache<ApiInformationMethod, IApiInformationCodeGen>();

	private static Dictionary<ApiInformationParameter, IApiInformationCodeGen> apiInformationParameterCache = new InstanceCache<ApiInformationParameter, IApiInformationCodeGen>();

	public static IApiInformationCodeGen CodeGen(this ApiInformation instance)
	{
		IApiInformationCodeGen value = null;
		if (!apiInformationCache.TryGetValue(instance, out value))
		{
			value = new ApiInformationCodeGenerator
			{
				Instance = instance
			};
			apiInformationCache.Add(instance, value);
		}
		return value;
	}

	public static IApiInformationCodeGen CodeGen(this ApiInformationMethod instance)
	{
		IApiInformationCodeGen value = null;
		if (!apiInformationMethodCache.TryGetValue(instance, out value))
		{
			value = new ApiInformationMethodCodeGenerator
			{
				Instance = instance
			};
			apiInformationMethodCache.Add(instance, value);
		}
		return value;
	}

	public static IApiInformationCodeGen CodeGen(this ApiInformationParameter instance)
	{
		IApiInformationCodeGen value = null;
		if (!apiInformationParameterCache.TryGetValue(instance, out value))
		{
			value = new ApiInformationParameterCodeGenerator
			{
				Instance = instance
			};
			apiInformationParameterCache.Add(instance, value);
		}
		return value;
	}
}
