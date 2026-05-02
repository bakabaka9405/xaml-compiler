using System;
using System.Linq;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class CSharp_CodeGenerator<T> : ManagedCodeGenerator<T>
{
	public string GeneratedCodeAttribute => "[global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Microsoft.UI.Xaml.Markup.Compiler\",\" " + ManagedCodeGenerator<T>.XamlCompilerVersion + "\")]";

	public string DebuggerNonUserCodeAttribute => "[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]";

	public string OverloadAttribute => "[global::Windows.Foundation.Metadata.DefaultOverload]";

	public string NotCLSCompliantAttribute
	{
		get
		{
			if (!base.ProjectInfo.IsCLSCompliant)
			{
				return "";
			}
			return "[global::System.CLSCompliant(false)] ";
		}
	}

	public override string ToStringWithCulture(ICodeGenOutput codegenOutput)
	{
		return codegenOutput.CSharpName();
	}

	public override string ToStringWithCulture(XamlType type)
	{
		return type.CSharpName();
	}

	public string Globalize(string fullType)
	{
		if (!fullType.StartsWith("global::"))
		{
			return "global::" + fullType;
		}
		return fullType;
	}

	public string PrependNamespace(string objectType)
	{
		if (base.ProjectInfo.UsingCSWinRT)
		{
			return "System.ComponentModel." + objectType;
		}
		return "Microsoft.UI.Xaml.Data." + objectType;
	}

	public string ObjectCast(string destinationType, string sourceName)
	{
		if (base.ProjectInfo.UsingCSWinRT)
		{
			return "global::WinRT.CastExtensions.As<" + Globalize(destinationType) + ">(" + sourceName + ")";
		}
		return "(" + destinationType + ")" + sourceName;
	}

	public string INPCInterfaceName(BindPathStep step)
	{
		XamlType valueType = step.ValueType;
		if (step is FunctionStep)
		{
			valueType = step.Children.Single((BindPathStep bp) => bp.ImplementsINPC).ValueType;
		}
		if (!valueType.ImplementsINotifyPropertyChanged())
		{
			throw new InvalidOperationException(valueType.Name + " doesn't implement INotifyPropertyChanged");
		}
		if (valueType.ImplementsXamlINotifyPropertyChanged())
		{
			return Globalize("Microsoft.UI.Xaml.Data.INotifyPropertyChanged");
		}
		return Globalize("System.ComponentModel.INotifyPropertyChanged");
	}

	public string PropertyChangedEventArgName(BindPathStep step)
	{
		if (!step.ValueType.ImplementsINotifyPropertyChanged())
		{
			throw new InvalidOperationException(step.ValueType.Name + " doesn't implement INotifyPropertyChanged");
		}
		if (step.ValueType.ImplementsXamlINotifyPropertyChanged())
		{
			return Globalize("Microsoft.UI.Xaml.Data.PropertyChangedEventArgs");
		}
		return Globalize("System.ComponentModel.PropertyChangedEventArgs");
	}

	public string INDEIInterfaceName(BindPathStep step)
	{
		XamlType valueType = step.ValueType;
		if (step is FunctionStep)
		{
			valueType = step.Children.Single((BindPathStep bp) => bp.ImplementsINDEI).ValueType;
		}
		if (!valueType.ImplementsINotifyDataErrorInfo())
		{
			throw new InvalidOperationException(valueType.Name + " doesn't implement INotifyDataErrorInfo");
		}
		if (valueType.ImplementsXamlINotifyDataErrorInfo())
		{
			return Globalize("Microsoft.UI.Xaml.Data.INotifyDataErrorInfo");
		}
		return Globalize("System.ComponentModel.INotifyDataErrorInfo");
	}

	public string DataErrorsEventArgName(BindPathStep step)
	{
		if (!step.ValueType.ImplementsINotifyDataErrorInfo())
		{
			throw new InvalidOperationException(step.ValueType.Name + " doesn't implement INotifyDataErrorInfo");
		}
		if (step.ValueType.ImplementsXamlINotifyDataErrorInfo())
		{
			return Globalize("Microsoft.UI.Xaml.Data.DataErrorsChangedEventArgs");
		}
		return Globalize("System.ComponentModel.DataErrorsChangedEventArgs");
	}

	public string INCCInterfaceName(BindPathStep step)
	{
		XamlType valueType = step.ValueType;
		if (step is FunctionStep)
		{
			valueType = step.Children.Single((BindPathStep bp) => bp.ImplementsINPC).ValueType;
		}
		if (!valueType.ImplementsINotifyCollectionChanged())
		{
			throw new InvalidOperationException(valueType.Name + " doesn't implement INotifyCollectionChanged");
		}
		if (valueType.ImplementsXamlINotifyCollectionChanged())
		{
			return Globalize("Microsoft.UI.Xaml.Interop.INotifyCollectionChanged");
		}
		return Globalize("System.Collections.Specialized.INotifyCollectionChanged");
	}

	public string NotifyCollectionChangedEventArgName(BindPathStep step)
	{
		if (!step.ValueType.ImplementsINotifyCollectionChanged())
		{
			throw new InvalidOperationException(step.ValueType.Name + " doesn't implement INotifyCollectionChanged");
		}
		if (step.ValueType.ImplementsXamlINotifyCollectionChanged())
		{
			return Globalize("Microsoft.UI.Xaml.Interop.NotifyCollectionChangedEventArgs");
		}
		return Globalize("System.Collections.Specialized.NotifyCollectionChangedEventArgs");
	}
}
