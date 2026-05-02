using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal abstract class BindAssignment : BindAssignmentBase, IBindAssignment
{
	private XamlDomMember fallbackMember;

	public BindPathStep BindBackStep { get; private set; }

	public BindStatus BindStatus { get; set; }

	public string BindBackPath { get; }

	public string TargetNullValue { get; }

	public string Converter { get; }

	public string ConverterParameter { get; }

	public string ConverterLanguage { get; }

	public UpdateSourceTrigger UpdateSourceTrigger { get; }

	public string CodeName => string.Format(CultureInfo.InvariantCulture, "obj{0}_{1}", base.ConnectionIdElement.ConnectionId, base.MemberName);

	public int ComputedPhase
	{
		get
		{
			if (base.ConnectionIdElement?.PhaseAssignment == null)
			{
				return 0;
			}
			return base.ConnectionIdElement.PhaseAssignment.Phase;
		}
	}

	public bool IsTrackingSource => BindStatus.HasFlag(BindStatus.TracksSource);

	public bool IsTrackingTarget => BindStatus.HasFlag(BindStatus.TracksTarget);

	public LanguageSpecificString FallbackValueExpression
	{
		get
		{
			if (fallbackMember.Item is XamlDomObject xamlDomObject)
			{
				if (xamlDomObject.Type.IsNullExtension())
				{
					return LanguageSpecificString.Null;
				}
			}
			else
			{
				string fallbackMemberValue = DomHelper.GetStringValueOfProperty(fallbackMember);
				if (fallbackMemberValue != null)
				{
					if (MemberType.IsString())
					{
						return new LanguageSpecificString(() => fallbackMemberValue.Quotenate(), () => "L" + fallbackMemberValue.Quotenate(), () => fallbackMemberValue.Quotenate(), () => fallbackMemberValue.Quotenate());
					}
					return MemberType.GetStringToThing(fallbackMemberValue.Quotenate(), isLiteral: true);
				}
			}
			return null;
		}
	}

	internal LanguageSpecificString TargetNullValueExpression
	{
		get
		{
			if (MemberType.IsNullable)
			{
				if (TargetNullValue == null)
				{
					return new LanguageSpecificString(() => LanguageSpecificString.Null.CppCXName(), () => "std::nullopt", () => LanguageSpecificString.Null.CSharpName(), () => LanguageSpecificString.Null.VBName());
				}
				return new LanguageSpecificString(() => TargetNullValue.Quotenate(), () => "L" + TargetNullValue.Quotenate(), () => TargetNullValue.Quotenate(), () => TargetNullValue.Quotenate());
			}
			return new LanguageSpecificString(() => "");
		}
	}

	public XamlType ValueType => base.PathStep.ValueType;

	public bool NeedsBox
	{
		get
		{
			if (ValueType.NeedsBoxUnbox())
			{
				return MemberType.IsObject();
			}
			return false;
		}
	}

	public bool NeedsLostFocusForTwoWay
	{
		get
		{
			switch (UpdateSourceTrigger)
			{
			case UpdateSourceTrigger.LostFocus:
				return true;
			case UpdateSourceTrigger.PropertyChanged:
				return false;
			case UpdateSourceTrigger.Default:
				if (base.MemberName == "Text")
				{
					return base.ConnectionIdElement.Type.IsDerivedFromTextBox();
				}
				return false;
			default:
				throw new ArgumentException($"Unexpected UpdateSourceTrigger '{UpdateSourceTrigger}'");
			}
		}
	}

	public XamlType MemberTargetType => bindMember.Member.TargetType;

	public string DisableFlagName => "is" + base.ConnectionIdElement.ObjectCodeName + base.MemberName + "Disabled";

	public LanguageSpecificString ReverseAssignmentExpression
	{
		get
		{
			LanguageSpecificString memberGetExpression = base.ConnectionIdElement.GetMemberGetExpression(this);
			return GetAssignmentExpression(MemberType, ValueType, memberGetExpression, convertBack: true);
		}
	}

	public static BindAssignment Create(XamlDomMember bindMember, BindUniverse bindUniverse, ConnectionIdElement connectionIdElement)
	{
		if (DomHelper.IsLoadMember(bindMember))
		{
			return new BoundLoadAssignment(bindMember, bindUniverse, connectionIdElement);
		}
		return new BoundPropertyAssignment(bindMember, bindUniverse, connectionIdElement);
	}

	protected BindAssignment(XamlDomMember bindMember, BindUniverse bindUniverse, ConnectionIdElement connectionIdElement)
		: base(bindMember, bindUniverse, connectionIdElement)
	{
		BindBackPath = DomHelper.GetStringValueOfProperty(bindItem.GetMemberNode("BindBack"));
		BindStatus = BindStatus.HasBinding;
		string text = DomHelper.GetStringValueOfProperty(bindItem.GetMemberNode("Mode"));
		if (text == null && connectionIdElement.DefaultBindMode != null)
		{
			text = connectionIdElement.DefaultBindMode;
		}
		if (text != null)
		{
			if (!text.Equals("OneTime", StringComparison.InvariantCultureIgnoreCase))
			{
				BindStatus |= BindStatus.TracksSource;
			}
			if (text.Equals("TwoWay", StringComparison.InvariantCultureIgnoreCase))
			{
				BindStatus |= BindStatus.TracksTarget;
			}
		}
		fallbackMember = bindItem.GetMemberNode("FallbackValue");
		if (fallbackMember != null)
		{
			BindStatus |= BindStatus.HasFallbackValue;
		}
		TargetNullValue = DomHelper.GetStringValueOfProperty(bindItem.GetMemberNode("TargetNullValue"));
		if (TargetNullValue != null)
		{
			BindStatus |= BindStatus.HasTargetNullValue;
		}
		XamlDomMember memberNode = bindItem.GetMemberNode("Converter");
		if (memberNode != null)
		{
			Converter = DomHelper.GetStaticResource_ResourceKey(memberNode.Item as XamlDomObject);
			BindStatus |= BindStatus.HasConverter;
		}
		ConverterParameter = DomHelper.GetStringValueOfProperty(bindItem.GetMemberNode("ConverterParameter"));
		ConverterLanguage = DomHelper.GetStringValueOfProperty(bindItem.GetMemberNode("ConverterLanguage"));
		memberNode = bindItem.GetMemberNode("UpdateSourceTrigger");
		if (memberNode != null)
		{
			Enum.TryParse<UpdateSourceTrigger>(DomHelper.GetStringValueOfProperty(memberNode), out var result);
			UpdateSourceTrigger = result;
		}
	}

	public LanguageSpecificString GetConverterExpression(LanguageSpecificString objectExpression, bool convertBack)
	{
		XamlType type = (convertBack ? base.PathStep.ValueType : MemberType);
		string text = (convertBack ? "ConvertBack" : "Convert");
		string cxConverter = string.Format("safe_cast<{0}>(this->LookupConverter(\"{1}\")->{6}({5}, {2}::typeid, {3}, {4}))", type.CppCXName(), Converter, type.CppCXName(IncludeHatIfApplicable: false), (ConverterParameter == null) ? "nullptr" : ConverterParameter.Quotenate(), (ConverterLanguage == null) ? "nullptr" : ConverterLanguage.Quotenate(), objectExpression.CppCXName(), text);
		string cppConverter = string.Format("::winrt::unbox_value<{0}>(LookupConverter(L\"{1}\").{6}(::winrt::box_value({5}), ::winrt::xaml_typename<{2}>(), {3}, {4}))", type.CppWinRTName(), Converter, type.CppWinRTName(), (ConverterParameter == null) ? "nullptr" : ("::winrt::box_value(::winrt::hstring(L\"" + ConverterParameter + "\"))"), (ConverterLanguage == null) ? "::winrt::hstring{}" : ("L\"" + ConverterLanguage + "\""), objectExpression.CppWinRTName(), text);
		string csConverter = string.Format("({0})this.LookupConverter(\"{1}\").{5}({4}, typeof({0}), {2}, {3})", type.CSharpName(), Converter, (ConverterParameter == null) ? "null" : ConverterParameter.Quotenate(), (ConverterLanguage == null) ? "null" : ConverterLanguage.Quotenate(), objectExpression.CSharpName(), text);
		string vbConverter = string.Format("DirectCast(Me.LookupConverter(\"{1}\").{5}({4}, GetType({0}), {2}, {3}),{0})", type.VBName(), Converter, (ConverterParameter == null) ? "Nothing" : ConverterParameter.Quotenate(), (ConverterLanguage == null) ? "Nothing" : ConverterLanguage.Quotenate(), objectExpression.VBName(), text);
		return new LanguageSpecificString(() => cxConverter, () => cppConverter, () => csConverter, () => vbConverter);
	}

	public LanguageSpecificString DirectAssignmentExpression(string expression)
	{
		return GetAssignmentExpression(ValueType, MemberType, new LanguageSpecificString(() => expression), convertBack: false);
	}

	private LanguageSpecificString GetAssignmentExpression(XamlType source, XamlType target, LanguageSpecificString expression, bool convertBack)
	{
		if (BindStatus.HasFlag(BindStatus.HasConverter))
		{
			return GetConverterExpression(expression, convertBack);
		}
		if (source.CanAssignDirectlyTo(target))
		{
			return expression;
		}
		if (source.CanInlineConvert(target))
		{
			return source.GetInlineConversionExpression(target, expression);
		}
		if (target.IsString())
		{
			return source.ToStringWithNullCheckExpression(expression);
		}
		if (source.IsString())
		{
			return target.GetStringToThing(expression);
		}
		throw new ArgumentException($"Don't know how to generate code for bind assignment at line {base.LineNumberInfo.StartLineNumber}");
	}

	public IEnumerable<XamlCompileErrorBase> ParsePath()
	{
		List<XamlCompileErrorBase> list = new List<XamlCompileErrorBase>();
		string expressionBeingParsed = "";
		try
		{
			List<string> list2 = new List<string>();
			expressionBeingParsed = BindAssignmentBase.GetBindingPath(bindItem);
			base.PathStep = ParseBindPath(list2);
			if (!string.IsNullOrEmpty(BindBackPath))
			{
				expressionBeingParsed = BindBackPath;
				BindBackStep = ParseBindPath(BindBackPath, list2);
			}
			foreach (string item in list2)
			{
				list.Add(new XamlXBindParseWarning(bindItem, item));
			}
		}
		catch (CompiledBindingParseException ex)
		{
			list.Add(new XamlXBindParseError(bindItem, ex));
			return list;
		}
		catch (ParseException ex2)
		{
			list.Add(new XamlXBindParseError(this, bindItem.StartLinePosition, expressionBeingParsed, ex2.Message));
			return list;
		}
		ApplySpecialCaseCasting();
		base.PathStep.AddBindAssignment(this);
		if (ValidatePathAssignment(list) && ValidateMode(list) && ValidateTypeCasting(list) && ValidateBindBackAssignment(list) && ValidateConverter(list) && ValidateApiInformation(list) && ValidateUpdateSourceTrigger(list))
		{
			ValidateFallbackValue(list);
		}
		return list;
	}

	private bool ValidateConverter(IList<XamlCompileErrorBase> issues)
	{
		if (Converter == null)
		{
			bool flag = ValueType.IsString() || MemberType.IsString();
			bool flag2 = ValueType.CanAssignDirectlyTo(MemberType) || ValueType.CanBoxTo(MemberType);
			bool flag3 = MemberType.CanAssignDirectlyTo(ValueType) || MemberType.CanInlineConvert(ValueType);
			if (!flag && !IsTrackingTarget)
			{
				flag = flag || flag2;
			}
			if (!flag && IsTrackingTarget)
			{
				flag = flag || (flag2 && flag3);
			}
			if (!flag)
			{
				issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_NeedsConverter, ValueType, MemberType)));
				return false;
			}
			if (ConverterParameter != null)
			{
				issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_OrphanConverterParam, "ConverterParameter")));
				return false;
			}
			if (ConverterLanguage != null)
			{
				issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_OrphanConverterParam, "ConverterLanguage")));
				return false;
			}
		}
		if (Converter != null && base.PathStep is FunctionStep)
		{
			issues.Add(new BindAssignmentValidationError(bindItem, XamlCompilerResources.BindAssignment_ConverterWithFunctionBindingNotSupported));
			return false;
		}
		return true;
	}

	private bool ValidatePathAssignment(IList<XamlCompileErrorBase> issues)
	{
		foreach (BindPathStep item in base.PathStep.ParentsAndSelf)
		{
			if (item is MethodStep)
			{
				issues.Add(new BindAssignmentValidationError(bindItem, XamlCompilerResources.BindPathParser_CantBindToMethods));
				return false;
			}
		}
		if (base.PathStep is FunctionStep && !base.PathStep.ValueType.CanAssignDirectlyTo(MemberType) && (!MemberType.IsString() || base.PathStep.ValueType.IsVoid()))
		{
			issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_FunctionReturnTypeInvalid, base.PathStep.ValueType.UnderlyingType.FullName, MemberType)));
			return false;
		}
		return true;
	}

	private bool ValidateBindBackAssignment(IList<XamlCompileErrorBase> issues)
	{
		if (base.PathStep is FunctionStep && BindStatus.HasFlag(BindStatus.TracksTarget))
		{
			if (BindBackStep == null)
			{
				issues.Add(new BindAssignmentValidationError(bindItem, XamlCompilerResources.BindAssignment_BindBack_NotFound));
				return false;
			}
			if (!(BindBackStep is MethodStep))
			{
				issues.Add(new BindAssignmentValidationError(bindItem, XamlCompilerResources.BindAssignment_BindBack_NotMethod));
				return false;
			}
			MethodStep methodStep = BindBackStep as MethodStep;
			if (methodStep.Parameters.Count != 1 || !MemberType.CanAssignDirectlyTo(methodStep.Parameters[0].ParameterType))
			{
				issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_BindBack_InvalidMethod, MemberType)));
				return false;
			}
		}
		else if (BindBackStep != null)
		{
			issues.Add(new BindAssignmentValidationError(bindItem, XamlCompilerResources.BindAssignment_BindBack_Unexpected));
			return false;
		}
		return true;
	}

	private void ApplySpecialCaseCasting()
	{
		if (Converter == null && MemberType.UnderlyingType.FullName == "Microsoft.UI.Xaml.Visibility" && ValueType.IsBoolOrNullableBool())
		{
			BindPathStep step = new CastStep(MemberType, base.PathStep, base.ApiInformation);
			base.PathStep = base.BindUniverse.EnsureUniquePathStep(step);
		}
	}

	private bool ValidateTypeCasting(IList<XamlCompileErrorBase> issues)
	{
		if (base.PathStep is CastStep && IsTrackingTarget)
		{
			issues.Add(new BindAssignmentValidationError(bindItem, XamlCompilerResources.BindPathParser_CantTwoWayCastStep));
			return false;
		}
		return true;
	}

	private bool ValidateApiInformation(IList<XamlCompileErrorBase> issues)
	{
		if (base.ApiInformation == null)
		{
			for (BindPathStep bindPathStep = base.PathStep; bindPathStep != null; bindPathStep = bindPathStep.Parent)
			{
				if (bindPathStep.ValueTypeIsConditional)
				{
					issues.Add(new BindAssignmentValidationError(bindItem, XamlCompilerResources.BindAssignment_RequiresConditionalNamespace));
					return false;
				}
			}
		}
		return true;
	}

	private bool ValidateUpdateSourceTrigger(IList<XamlCompileErrorBase> issues)
	{
		XamlDomMember memberNode = bindItem.GetMemberNode("UpdateSourceTrigger");
		if (memberNode != null)
		{
			UpdateSourceTrigger result = UpdateSourceTrigger.Default;
			if (!Enum.TryParse<UpdateSourceTrigger>(DomHelper.GetStringValueOfProperty(memberNode), out result))
			{
				issues.Add(new BindAssignmentValidationError(bindItem, XamlCompilerResources.BindAssignment_UpdateSourceTrigger_UnrecognizedValue));
				return false;
			}
			if (result == UpdateSourceTrigger.Explicit)
			{
				issues.Add(new BindAssignmentValidationError(bindItem, XamlCompilerResources.BindAssignment_UpdateSourceTrigger_ExplicitUnsupported));
				return false;
			}
			if (!BindStatus.HasFlag(BindStatus.TracksTarget))
			{
				issues.Add(new BindAssignmentValidationError(bindItem, XamlCompilerResources.BindAssignment_UpdateSourceTrigger_UpdateSourceTriggerOnlyWithTwoWay));
				return false;
			}
			if (result == UpdateSourceTrigger.LostFocus)
			{
				XamlMember member = MemberDeclaringType.GetMember("LostFocus");
				if (member == null || !member.IsEvent)
				{
					issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_UpdateSourceTrigger_LostFocusEventRequired)));
					return false;
				}
			}
			if (result == UpdateSourceTrigger.PropertyChanged && !DomHelper.IsDependencyProperty(bindMember))
			{
				issues.Add(new BindAssignmentValidationError(bindItem, ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_UpdateSourceTrigger_PropertyChangedOnlyOnDP)));
				return false;
			}
		}
		return true;
	}

	private bool ValidateMode(IList<XamlCompileErrorBase> issues)
	{
		if (BindStatus.HasFlag(BindStatus.TracksSource) && !BindStatus.HasFlag(BindStatus.TracksTarget))
		{
			List<BindPathStep> list = new List<BindPathStep> { base.PathStep };
			list.AddRange(base.PathStep.Dependencies);
			IEnumerable<BindPathStep> source = list.SelectMany((BindPathStep s) => s.ParentsAndSelf);
			if (!source.Any((BindPathStep s) => s.RequiresChildNotification))
			{
				issues.Add(new BindAssignmentValidationWarning(bindItem, ErrorCode.WMC1506, XamlCompilerResources.BindAssignment_OneWay_NoWay));
			}
		}
		return true;
	}

	private bool ValidateFallbackValue(IList<XamlCompileErrorBase> issues)
	{
		if (fallbackMember != null && FallbackValueExpression == null)
		{
			issues.Add(new BindAssignmentValidationError(bindItem, XamlCompilerResources.BindAssignment_InvalidFallbackValue));
			return false;
		}
		return true;
	}
}
