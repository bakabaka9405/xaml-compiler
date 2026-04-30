using System;
using System.Collections.Generic;
using System.Linq;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class CppWinRT_CodeGenerator<T> : NativeCodeGenerator<T>
{
	public override string ToStringWithCulture(ICodeGenOutput codegenOutput)
	{
		return codegenOutput.CppWinRTName();
	}

	public override string ToStringWithCulture(XamlType type)
	{
		return type.CppWinRTName();
	}

	public static string Projection(string typeName)
	{
		string text = NativeCodeGenerator<T>.Globalize(typeName);
		if (!text.StartsWith("::winrt::"))
		{
			text = "::winrt" + text;
			text = text.Replace("<::", "<::winrt::");
			return text.Replace("::winrt::winrt::", "::winrt::");
		}
		throw new ArgumentException("Name should not already contain ::winrt prefix");
	}

	protected string GetBindingTrackingClassName(BindUniverse bindUniverse, XamlClassCodeInfo codeInfo)
	{
		return NativeCodeGenerator<T>.Colonize(bindUniverse.NeedsCppBindingTrackingClass ? bindUniverse.BindingsTrackingClassName : Projection(base.ProjectInfo.RootNamespace + "::implementation::XamlBindingTrackingBase"));
	}

	public IEnumerable<string> GetCacheDeclarations(BindUniverse bindUniverse)
	{
		foreach (BindPathStep step in bindUniverse.BindPathSteps.Values.Where((BindPathStep bindPathStep) => bindPathStep.IsIncludedInUpdate && bindPathStep.NeedsUpdateChildListeners))
		{
			if (step.ImplementsINPC && step.RequiresChildNotification)
			{
				if (step is RootStep)
				{
					yield return "::winrt::weak_ref<" + Projection("Microsoft.UI.Xaml.Data") + "::INotifyPropertyChanged> cachePC_" + step.CodeName + ";";
				}
				else
				{
					yield return Projection("Microsoft.UI.Xaml.Data") + "::INotifyPropertyChanged cachePC_" + step.CodeName + "{nullptr};";
				}
			}
			if (step.ImplementsINDEI)
			{
				yield return Projection("Microsoft.UI.Xaml.Data") + "::INotifyDataErrorInfo cacheEC_" + step.CodeName + "{nullptr};";
			}
			if (step.ImplementsIObservableVector && step.RequiresChildNotification)
			{
				yield return step.ValueType.CppWinRTName() + " cacheVC_" + step.CodeName + "{nullptr};";
			}
			if (step.ImplementsIObservableMap && step.RequiresChildNotification)
			{
				yield return step.ValueType.CppWinRTName() + " cacheMC_" + step.CodeName + "{nullptr};";
			}
			else if (step.ImplementsINCC)
			{
				yield return Projection("Microsoft.UI.Xaml.Interop") + "::INotifyCollectionChanged cacheCC_" + step.CodeName + "{nullptr};";
			}
			foreach (DependencyPropertyStep item in step.TrackingSteps.OfType<DependencyPropertyStep>())
			{
				if (step is RootStep)
				{
					yield return "::winrt::weak_ref<" + Projection("Microsoft.UI.Xaml") + "::DependencyObject> cacheDPC_" + item.CodeName + ";";
				}
				else
				{
					yield return Projection("Microsoft.UI.Xaml") + "::DependencyObject cacheDPC_" + item.CodeName + "{nullptr};";
				}
			}
		}
	}

	public IEnumerable<string> GetTokenDeclarations(BindUniverse bindUniverse)
	{
		foreach (BindPathStep step in bindUniverse.BindPathSteps.Values.Where((BindPathStep bindPathStep) => bindPathStep.IsIncludedInUpdate && bindPathStep.NeedsUpdateChildListeners))
		{
			if (step.ImplementsINPC)
			{
				yield return "::winrt::event_token tokenPC_" + step.CodeName + " {};";
			}
			if (step.ImplementsINDEI)
			{
				yield return "::winrt::event_token tokenEC_" + step.CodeName + " {};";
			}
			if (step.ImplementsIObservableVector)
			{
				yield return "::winrt::event_token tokenVC_" + step.CodeName + " {};";
			}
			if (step.ImplementsIObservableMap)
			{
				yield return "::winrt::event_token tokenMC_" + step.CodeName + " {};";
			}
			else if (step.ImplementsINCC)
			{
				yield return "::winrt::event_token tokenCC_" + step.CodeName + " {};";
			}
		}
		foreach (BindPathStep item in bindUniverse.BindPathSteps.Values.Where((BindPathStep bindPathStep) => bindPathStep.IsIncludedInUpdate && bindPathStep.NeedsUpdateChildListeners))
		{
			foreach (DependencyPropertyStep item2 in item.TrackingSteps.OfType<DependencyPropertyStep>())
			{
				yield return "__int64 tokenDPC_" + item2.CodeName + "{0};";
			}
		}
	}
}
