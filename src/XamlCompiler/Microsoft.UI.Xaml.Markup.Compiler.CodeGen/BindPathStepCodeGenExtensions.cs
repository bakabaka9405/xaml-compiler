using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.Core;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal static class BindPathStepCodeGenExtensions
{
	public static Dictionary<BindPathStep, IBindPathStepCodeGen> generatorCache = new InstanceCache<BindPathStep, IBindPathStepCodeGen>();

	public static IBindPathStepCodeGen CodeGen(this BindPathStep instance)
	{
		IBindPathStepCodeGen value = null;
		if (!generatorCache.TryGetValue(instance, out value))
		{
			if (instance is RootStep instance2)
			{
				value = new RootStepCodeGenerator
				{
					Instance = instance2
				};
			}
			else if (instance is StaticRootStep instance3)
			{
				value = new StaticRootStepCodeGenerator
				{
					Instance = instance3
				};
			}
			else if (instance is RootNamedElementStep instance4)
			{
				value = new RootNamedElementStepCodeGenerator
				{
					Instance = instance4
				};
			}
			else if (instance is FieldStep instance5)
			{
				value = new FieldStepCodeGenerator
				{
					Instance = instance5
				};
			}
			else if (instance is CastStep instance6)
			{
				value = new CastStepCodeGenerator
				{
					Instance = instance6
				};
			}
			else if (instance is ArrayIndexStep instance7)
			{
				value = new ArrayIndexStepCodeGenerator
				{
					Instance = instance7
				};
			}
			else if (instance is MapIndexStep instance8)
			{
				value = new MapIndexStepCodeGenerator
				{
					Instance = instance8
				};
			}
			else if (instance is AttachedPropertyStep instance9)
			{
				value = new AttachedPropertyStepCodeGenerator
				{
					Instance = instance9
				};
			}
			else if (instance is DependencyPropertyStep instance10)
			{
				value = new DependencyPropertyStepCodeGenerator<DependencyPropertyStep>
				{
					Instance = instance10
				};
			}
			else if (instance is PropertyStep instance11)
			{
				value = new PropertyStepCodeGenerator<PropertyStep>
				{
					Instance = instance11
				};
			}
			else if (instance is MethodStep instance12)
			{
				value = new MethodStepCodeGenerator
				{
					Instance = instance12
				};
			}
			else
			{
				if (!(instance is FunctionStep instance13))
				{
					throw new NotImplementedException("Code generator not found!");
				}
				value = new FunctionStepCodeGenerator
				{
					Instance = instance13
				};
			}
			generatorCache.Add(instance, value);
		}
		return value;
	}
}
