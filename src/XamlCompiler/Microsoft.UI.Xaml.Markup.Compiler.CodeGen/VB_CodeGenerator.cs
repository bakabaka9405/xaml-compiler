using System;
using System.Text;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal class VB_CodeGenerator<T> : ManagedCodeGenerator<T>
{
	public string GeneratedCodeAttribute => "<Global.System.CodeDom.Compiler.GeneratedCodeAttribute(\"Microsoft.UI.Xaml.Markup.Compiler\", \" " + ManagedCodeGenerator<T>.XamlCompilerVersion + "\")>  _";

	public string DebuggerNonUserCodeAttribute => "<Global.System.Diagnostics.DebuggerNonUserCodeAttribute()>  _";

	public string NotCLSCompliantAttribute
	{
		get
		{
			if (!base.ProjectInfo.IsCLSCompliant)
			{
				return "";
			}
			return "<Global.System.CLSCompliant(False)> ";
		}
	}

	public override string ToStringWithCulture(ICodeGenOutput codegenOutput)
	{
		return codegenOutput.VBName();
	}

	public override string ToStringWithCulture(XamlType type)
	{
		return type.VBName();
	}

	public override string ToStringWithCulture(bool value)
	{
		if (!value)
		{
			return "False";
		}
		return "True";
	}

	public string Globalize(string fullType)
	{
		return "Global." + fullType;
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

	public string INPCInterfaceName(BindPathStep step)
	{
		if (!step.ValueType.ImplementsINotifyPropertyChanged())
		{
			throw new InvalidOperationException(step.ValueType.Name + " doesn't implement INotifyPropertyChanged");
		}
		if (step.ValueType.ImplementsXamlINotifyPropertyChanged())
		{
			return Globalize("Microsoft.UI.Xaml.Data.INotifyPropertyChanged");
		}
		return Globalize("System.ComponentModel.INotifyPropertyChanged");
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

	public string INDEIInterfaceName(BindPathStep step)
	{
		if (!step.ValueType.ImplementsINotifyDataErrorInfo())
		{
			throw new InvalidOperationException(step.ValueType.Name + " doesn't implement INotifyDataErrorInfo");
		}
		if (step.ValueType.ImplementsXamlINotifyDataErrorInfo())
		{
			return Globalize("Microsoft.UI.Xaml.Data.INotifyDataErrorInfo");
		}
		return Globalize("System.ComponentModel.INotifyDataErrorInfo");
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

	public string INCCInterfaceName(BindPathStep step)
	{
		if (!step.ValueType.ImplementsINotifyCollectionChanged())
		{
			throw new InvalidOperationException(step.ValueType.Name + " doesn't implement INotifyCollectionChanged");
		}
		if (step.ValueType.ImplementsXamlINotifyCollectionChanged())
		{
			return Globalize("Microsoft.UI.Xaml.Interop.INotifyCollectionChanged");
		}
		return Globalize("System.Collections.Specialized.INotifyCollectionChanged");
	}

	protected override string GetPhaseCondition(BindPathStep bindStep)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("(phase And (NOT_PHASED");
		if (bindStep.IsTrackingSource)
		{
			stringBuilder.Append(" Or DATA_CHANGED");
		}
		foreach (int distinctPhase in bindStep.DistinctPhases)
		{
			stringBuilder.AppendFormat(" Or (1 << {0})", distinctPhase);
		}
		stringBuilder.Append(")) <> 0");
		return stringBuilder.ToString();
	}

	public override string GetDirectPhaseCondition(int phase, bool isTracking)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("(phase And ((1 << {0}) Or NOT_PHASED ", phase);
		if (isTracking)
		{
			stringBuilder.Append("Or DATA_CHANGED");
		}
		stringBuilder.Append(")) <> 0");
		return stringBuilder.ToString();
	}
}
