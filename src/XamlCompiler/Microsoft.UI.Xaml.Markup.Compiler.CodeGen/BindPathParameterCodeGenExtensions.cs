using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.Core;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal static class BindPathParameterCodeGenExtensions
{
	public static Dictionary<Parameter, IBindPathParameterCodeGen> generatorCache = new InstanceCache<Parameter, IBindPathParameterCodeGen>();

	public static IBindPathParameterCodeGen CodeGen(this Parameter instance)
	{
		IBindPathParameterCodeGen value = null;
		if (!generatorCache.TryGetValue(instance, out value))
		{
			if (instance is FunctionNumberParam instance2)
			{
				value = new FunctionNumberParamCodeGenerator
				{
					Instance = instance2
				};
			}
			else if (instance is FunctionNullValueParam instance3)
			{
				value = new FunctionNullValueParamCodeGenerator
				{
					Instance = instance3
				};
			}
			else if (instance is FunctionStringParam instance4)
			{
				value = new FunctionStringParamCodeGenerator
				{
					Instance = instance4
				};
			}
			else if (instance is FunctionBoolParam instance5)
			{
				value = new FunctionBoolParamCodeGenerator
				{
					Instance = instance5
				};
			}
			else
			{
				if (!(instance is FunctionPathParam instance6))
				{
					throw new NotImplementedException("Code generator not found!");
				}
				value = new FunctionPathParamCodeGenerator
				{
					Instance = instance6
				};
			}
			generatorCache.Add(instance, value);
		}
		return value;
	}
}
