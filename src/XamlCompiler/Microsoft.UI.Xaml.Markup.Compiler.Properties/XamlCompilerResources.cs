using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Microsoft.UI.Xaml.Markup.Compiler.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class XamlCompilerResources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				ResourceManager resourceManager = new ResourceManager("Microsoft.UI.Xaml.Markup.Compiler.Properties.XamlCompilerResources", typeof(XamlCompilerResources).Assembly);
				resourceMan = resourceManager;
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static string BindAssignment_BindBack_InvalidMethod => ResourceManager.GetString("BindAssignment_BindBack_InvalidMethod", resourceCulture);

	internal static string BindAssignment_BindBack_NotFound => ResourceManager.GetString("BindAssignment_BindBack_NotFound", resourceCulture);

	internal static string BindAssignment_BindBack_NotMethod => ResourceManager.GetString("BindAssignment_BindBack_NotMethod", resourceCulture);

	internal static string BindAssignment_BindBack_Unexpected => ResourceManager.GetString("BindAssignment_BindBack_Unexpected", resourceCulture);

	internal static string BindAssignment_CastCannotStartWithAttachedProperty => ResourceManager.GetString("BindAssignment_CastCannotStartWithAttachedProperty", resourceCulture);

	internal static string BindAssignment_ConverterWithFunctionBindingNotSupported => ResourceManager.GetString("BindAssignment_ConverterWithFunctionBindingNotSupported", resourceCulture);

	internal static string BindAssignment_FieldNameElementName_Conflict => ResourceManager.GetString("BindAssignment_FieldNameElementName_Conflict", resourceCulture);

	internal static string BindAssignment_FieldNameElementName_ConflictBadConfig => ResourceManager.GetString("BindAssignment_FieldNameElementName_ConflictBadConfig", resourceCulture);

	internal static string BindAssignment_FunctionReturnTypeInvalid => ResourceManager.GetString("BindAssignment_FunctionReturnTypeInvalid", resourceCulture);

	internal static string BindAssignment_InvalidCast => ResourceManager.GetString("BindAssignment_InvalidCast", resourceCulture);

	internal static string BindAssignment_InvalidFallbackValue => ResourceManager.GetString("BindAssignment_InvalidFallbackValue", resourceCulture);

	internal static string BindAssignment_InvalidPropertyPathSyntax => ResourceManager.GetString("BindAssignment_InvalidPropertyPathSyntax", resourceCulture);

	internal static string BindAssignment_NeedsConverter => ResourceManager.GetString("BindAssignment_NeedsConverter", resourceCulture);

	internal static string BindAssignment_OneWay_NoWay => ResourceManager.GetString("BindAssignment_OneWay_NoWay", resourceCulture);

	internal static string BindAssignment_OrphanConverterParam => ResourceManager.GetString("BindAssignment_OrphanConverterParam", resourceCulture);

	internal static string BindAssignment_RequiresConditionalNamespace => ResourceManager.GetString("BindAssignment_RequiresConditionalNamespace", resourceCulture);

	internal static string BindAssignment_UpdateSourceTrigger_ExplicitUnsupported => ResourceManager.GetString("BindAssignment_UpdateSourceTrigger_ExplicitUnsupported", resourceCulture);

	internal static string BindAssignment_UpdateSourceTrigger_LostFocusEventRequired => ResourceManager.GetString("BindAssignment_UpdateSourceTrigger_LostFocusEventRequired", resourceCulture);

	internal static string BindAssignment_UpdateSourceTrigger_PropertyChangedOnlyOnDP => ResourceManager.GetString("BindAssignment_UpdateSourceTrigger_PropertyChangedOnlyOnDP", resourceCulture);

	internal static string BindAssignment_UpdateSourceTrigger_UnrecognizedValue => ResourceManager.GetString("BindAssignment_UpdateSourceTrigger_UnrecognizedValue", resourceCulture);

	internal static string BindAssignment_UpdateSourceTrigger_UpdateSourceTriggerOnlyWithTwoWay => ResourceManager.GetString("BindAssignment_UpdateSourceTrigger_UpdateSourceTriggerOnlyWithTwoWay", resourceCulture);

	internal static string BindAssignment_XamlXBindAssignmentValidationError => ResourceManager.GetString("BindAssignment_XamlXBindAssignmentValidationError", resourceCulture);

	internal static string BindAssignment_XamlXBindParseError => ResourceManager.GetString("BindAssignment_XamlXBindParseError", resourceCulture);

	internal static string BindAssignment_XamlXBindUsedInStyleError => ResourceManager.GetString("BindAssignment_XamlXBindUsedInStyleError", resourceCulture);

	internal static string BindPathParser_CantBindToMethods => ResourceManager.GetString("BindPathParser_CantBindToMethods", resourceCulture);

	internal static string BindPathParser_CantTwoWayCastStep => ResourceManager.GetString("BindPathParser_CantTwoWayCastStep", resourceCulture);

	internal static string BindPathParser_PathSetTwice => ResourceManager.GetString("BindPathParser_PathSetTwice", resourceCulture);

	internal static string BindPathParser_UnexpectedReflectionType => ResourceManager.GetString("BindPathParser_UnexpectedReflectionType", resourceCulture);

	internal static string BoundEventAssignment_InvalidSignature => ResourceManager.GetString("BoundEventAssignment_InvalidSignature", resourceCulture);

	internal static string BoundEventAssignment_NonDelegateProperty => ResourceManager.GetString("BoundEventAssignment_NonDelegateProperty", resourceCulture);

	internal static string BoundEventAssignment_NonLeafMethod => ResourceManager.GetString("BoundEventAssignment_NonLeafMethod", resourceCulture);

	internal static string BoundEventAssignment_NoOverloads => ResourceManager.GetString("BoundEventAssignment_NoOverloads", resourceCulture);

	internal static string BoundEventAssignment_SignatureMismatch => ResourceManager.GetString("BoundEventAssignment_SignatureMismatch", resourceCulture);

	internal static string ConditionalNamespace_ConditionalInStandard => ResourceManager.GetString("ConditionalNamespace_ConditionalInStandard", resourceCulture);

	internal static string ConditionalNamespace_FailedToParse => ResourceManager.GetString("ConditionalNamespace_FailedToParse", resourceCulture);

	internal static string ConditionalNamespace_MultipleApiInformations => ResourceManager.GetString("ConditionalNamespace_MultipleApiInformations", resourceCulture);

	internal static string ConditionalNamespace_MultipleTargetPlatforms => ResourceManager.GetString("ConditionalNamespace_MultipleTargetPlatforms", resourceCulture);

	internal static string ConditionalNamespace_UnmatchedApiInformationParameters => ResourceManager.GetString("ConditionalNamespace_UnmatchedApiInformationParameters", resourceCulture);

	internal static string ConditionalNamespace_UnrecognizedApiInformation => ResourceManager.GetString("ConditionalNamespace_UnrecognizedApiInformation", resourceCulture);

	internal static string CppCodeGen_MissingMember => ResourceManager.GetString("CppCodeGen_MissingMember", resourceCulture);

	internal static string CreateFromString_InvalidMethodSignature => ResourceManager.GetString("CreateFromString_InvalidMethodSignature", resourceCulture);

	internal static string CreateFromString_MethodOnTypeNotFound => ResourceManager.GetString("CreateFromString_MethodOnTypeNotFound", resourceCulture);

	internal static string CreateFromString_TypeNotFound => ResourceManager.GetString("CreateFromString_TypeNotFound", resourceCulture);

	internal static string DuiSchema_AmbiguousCollectionAdd => ResourceManager.GetString("DuiSchema_AmbiguousCollectionAdd", resourceCulture);

	internal static string DuiSchema_ArgumentNotXamlDirective => ResourceManager.GetString("DuiSchema_ArgumentNotXamlDirective", resourceCulture);

	internal static string DuiSchema_BadBindablePropertyProvider => ResourceManager.GetString("DuiSchema_BadBindablePropertyProvider", resourceCulture);

	internal static string DuiSchema_BindableNotSupportedOnGeneric => ResourceManager.GetString("DuiSchema_BindableNotSupportedOnGeneric", resourceCulture);

	internal static string DuiSchema_CustomAttributesTypeLoadException => ResourceManager.GetString("DuiSchema_CustomAttributesTypeLoadException", resourceCulture);

	internal static string DuiSchema_GetAllXamlTypeNotImpl => ResourceManager.GetString("DuiSchema_GetAllXamlTypeNotImpl", resourceCulture);

	internal static string DuiSchema_TypeLoadException => ResourceManager.GetString("DuiSchema_TypeLoadException", resourceCulture);

	internal static string DuiSchema_TypeLoadExceptionMessage => ResourceManager.GetString("DuiSchema_TypeLoadExceptionMessage", resourceCulture);

	internal static string DuiSchema_WRTAssembliesMissing => ResourceManager.GetString("DuiSchema_WRTAssembliesMissing", resourceCulture);

	internal static string Harvester_ClassMustHaveANamespace => ResourceManager.GetString("Harvester_ClassMustHaveANamespace", resourceCulture);

	internal static string Harvester_ClassNameEmptyPathPart => ResourceManager.GetString("Harvester_ClassNameEmptyPathPart", resourceCulture);

	internal static string Harvester_ClassNameNoWhiteSpace => ResourceManager.GetString("Harvester_ClassNameNoWhiteSpace", resourceCulture);

	internal static string Harvester_ControlTemplateDoesNotDefineTargetType => ResourceManager.GetString("Harvester_ControlTemplateDoesNotDefineTargetType", resourceCulture);

	internal static string Harvester_DataTemplateDoesNotDefineDataType => ResourceManager.GetString("Harvester_DataTemplateDoesNotDefineDataType", resourceCulture);

	internal static string Harvester_ProjectFolderIsNotADirectory => ResourceManager.GetString("Harvester_ProjectFolderIsNotADirectory", resourceCulture);

	internal static string TypeInfoReflection_TypeViolatesNamingConvention => ResourceManager.GetString("TypeInfoReflection_TypeViolatesNamingConvention", resourceCulture);

	internal static string XamlCompiler_BadName => ResourceManager.GetString("XamlCompiler_BadName", resourceCulture);

	internal static string XamlCompiler_BadNameChar => ResourceManager.GetString("XamlCompiler_BadNameChar", resourceCulture);

	internal static string XamlCompiler_BadValueInSupressWarningsList => ResourceManager.GetString("XamlCompiler_BadValueInSupressWarningsList", resourceCulture);

	internal static string XamlCompiler_BaseFilenamesMustBeTheSame => ResourceManager.GetString("XamlCompiler_BaseFilenamesMustBeTheSame", resourceCulture);

	internal static string XamlCompiler_BindingAotCompatibility => ResourceManager.GetString("XamlCompiler_BindingAotCompatibility", resourceCulture);

	internal static string XamlCompiler_BindingSetValueFailed => ResourceManager.GetString("XamlCompiler_BindingSetValueFailed", resourceCulture);

	internal static string XamlCompiler_BindingUpdateFailed => ResourceManager.GetString("XamlCompiler_BindingUpdateFailed", resourceCulture);

	internal static string XamlCompiler_CannotHaveDeferLoadStrategy => ResourceManager.GetString("XamlCompiler_CannotHaveDeferLoadStrategy", resourceCulture);

	internal static string XamlCompiler_CannotNameElementTwice => ResourceManager.GetString("XamlCompiler_CannotNameElementTwice", resourceCulture);

	internal static string XamlCompiler_CantAccessNonPublicType => ResourceManager.GetString("XamlCompiler_CantAccessNonPublicType", resourceCulture);

	internal static string XamlCompiler_CantAddToCollectionObject => ResourceManager.GetString("XamlCompiler_CantAddToCollectionObject", resourceCulture);

	internal static string XamlCompiler_CantAddToCollectionProperty => ResourceManager.GetString("XamlCompiler_CantAddToCollectionProperty", resourceCulture);

	internal static string XamlCompiler_CantAddToDictionaryObject => ResourceManager.GetString("XamlCompiler_CantAddToDictionaryObject", resourceCulture);

	internal static string XamlCompiler_CantAddToDictionaryProperty => ResourceManager.GetString("XamlCompiler_CantAddToDictionaryProperty", resourceCulture);

	internal static string XamlCompiler_CantAssign => ResourceManager.GetString("XamlCompiler_CantAssign", resourceCulture);

	internal static string XamlCompiler_CantAssignTextToProperty => ResourceManager.GetString("XamlCompiler_CantAssignTextToProperty", resourceCulture);

	internal static string XamlCompiler_CantAssignToReadOnlyProperty => ResourceManager.GetString("XamlCompiler_CantAssignToReadOnlyProperty", resourceCulture);

	internal static string XamlCompiler_CantNameValueTypes => ResourceManager.GetString("XamlCompiler_CantNameValueTypes", resourceCulture);

	internal static string XamlCompiler_CantResolveAssembly => ResourceManager.GetString("XamlCompiler_CantResolveAssembly", resourceCulture);

	internal static string XamlCompiler_CantResolveDataType => ResourceManager.GetString("XamlCompiler_CantResolveDataType", resourceCulture);

	internal static string XamlCompiler_CantResolveWinUIMetadata => ResourceManager.GetString("XamlCompiler_CantResolveWinUIMetadata", resourceCulture);

	internal static string XamlCompiler_CodeGenStaticAssert_IncompleteType => ResourceManager.GetString("XamlCompiler_CodeGenStaticAssert_IncompleteType", resourceCulture);

	internal static string XamlCompiler_CodeGenStaticAssert_IncompleteType_NoPch => ResourceManager.GetString("XamlCompiler_CodeGenStaticAssert_IncompleteType_NoPch", resourceCulture);

	internal static string XamlCompiler_CodeGenString_Bad => ResourceManager.GetString("XamlCompiler_CodeGenString_Bad", resourceCulture);

	internal static string XamlCompiler_CodeGenString_NotSupported => ResourceManager.GetString("XamlCompiler_CodeGenString_NotSupported", resourceCulture);

	internal static string XamlCompiler_CodeGenString_Using => ResourceManager.GetString("XamlCompiler_CodeGenString_Using", resourceCulture);

	internal static string XamlCompiler_CodeLangNotSupported => ResourceManager.GetString("XamlCompiler_CodeLangNotSupported", resourceCulture);

	internal static string XamlCompiler_DeferLoadStrategyMissingXName => ResourceManager.GetString("XamlCompiler_DeferLoadStrategyMissingXName", resourceCulture);

	internal static string XamlCompiler_Deprecated => ResourceManager.GetString("XamlCompiler_Deprecated", resourceCulture);

	internal static string XamlCompiler_DictionaryItemsCannotBeText => ResourceManager.GetString("XamlCompiler_DictionaryItemsCannotBeText", resourceCulture);

	internal static string XamlCompiler_DictionaryItemsHasDuplicateKey => ResourceManager.GetString("XamlCompiler_DictionaryItemsHasDuplicateKey", resourceCulture);

	internal static string XamlCompiler_DictionaryItemsMustHaveKeys => ResourceManager.GetString("XamlCompiler_DictionaryItemsMustHaveKeys", resourceCulture);

	internal static string XamlCompiler_DuplicateTypeName => ResourceManager.GetString("XamlCompiler_DuplicateTypeName", resourceCulture);

	internal static string XamlCompiler_DuplicationAssignment => ResourceManager.GetString("XamlCompiler_DuplicationAssignment", resourceCulture);

	internal static string XamlCompiler_ElementNameAlreadyUsed => ResourceManager.GetString("XamlCompiler_ElementNameAlreadyUsed", resourceCulture);

	internal static string XamlCompiler_EventValuesMustBeText => ResourceManager.GetString("XamlCompiler_EventValuesMustBeText", resourceCulture);

	internal static string XamlCompiler_Experimental => ResourceManager.GetString("XamlCompiler_Experimental", resourceCulture);

	internal static string XamlCompiler_FeatureNotInMinVersion => ResourceManager.GetString("XamlCompiler_FeatureNotInMinVersion", resourceCulture);

	internal static string XamlCompiler_FeatureOnlyInTargetVersion => ResourceManager.GetString("XamlCompiler_FeatureOnlyInTargetVersion", resourceCulture);

	internal static string XamlCompiler_FileOpenFailure => ResourceManager.GetString("XamlCompiler_FileOpenFailure", resourceCulture);

	internal static string XamlCompiler_IdPropertiesMustBeText => ResourceManager.GetString("XamlCompiler_IdPropertiesMustBeText", resourceCulture);

	internal static string XamlCompiler_InternalErrorProcessingStyle => ResourceManager.GetString("XamlCompiler_InternalErrorProcessingStyle", resourceCulture);

	internal static string XamlCompiler_InvalidCPA => ResourceManager.GetString("XamlCompiler_InvalidCPA", resourceCulture);

	internal static string XamlCompiler_InvalidFieldModifier => ResourceManager.GetString("XamlCompiler_InvalidFieldModifier", resourceCulture);

	internal static string XamlCompiler_InvalidPropertyType => ResourceManager.GetString("XamlCompiler_InvalidPropertyType", resourceCulture);

	internal static string XamlCompiler_InvalidSignedChar => ResourceManager.GetString("XamlCompiler_InvalidSignedChar", resourceCulture);

	internal static string XamlCompiler_InvalidValueForPhase => ResourceManager.GetString("XamlCompiler_InvalidValueForPhase", resourceCulture);

	internal static string XamlCompiler_InvalidValueForSuppressXamlTrimWarnings => ResourceManager.GetString("XamlCompiler_InvalidValueForSuppressXamlTrimWarnings", resourceCulture);

	internal static string XamlCompiler_LanguageUnsupported => ResourceManager.GetString("XamlCompiler_LanguageUnsupported", resourceCulture);

	internal static string XamlCompiler_LoadConflict => ResourceManager.GetString("XamlCompiler_LoadConflict", resourceCulture);

	internal static string XamlCompiler_LoadMissingName => ResourceManager.GetString("XamlCompiler_LoadMissingName", resourceCulture);

	internal static string XamlCompiler_LoadNotSupported => ResourceManager.GetString("XamlCompiler_LoadNotSupported", resourceCulture);

	internal static string XamlCompiler_LocalAssemblyMissingWarning => ResourceManager.GetString("XamlCompiler_LocalAssemblyMissingWarning", resourceCulture);

	internal static string XamlCompiler_MemberContractDoesNotExist => ResourceManager.GetString("XamlCompiler_MemberContractDoesNotExist", resourceCulture);

	internal static string XamlCompiler_MissingCPA => ResourceManager.GetString("XamlCompiler_MissingCPA", resourceCulture);

	internal static string XamlCompiler_MoreThanOneApplicationXaml => ResourceManager.GetString("XamlCompiler_MoreThanOneApplicationXaml", resourceCulture);

	internal static string XamlCompiler_MustNotSetLocalAssembly => ResourceManager.GetString("XamlCompiler_MustNotSetLocalAssembly", resourceCulture);

	internal static string XamlCompiler_MustSetLocalAssembly => ResourceManager.GetString("XamlCompiler_MustSetLocalAssembly", resourceCulture);

	internal static string XamlCompiler_NoEventsInAppXaml => ResourceManager.GetString("XamlCompiler_NoEventsInAppXaml", resourceCulture);

	internal static string XamlCompiler_NotConstructibleObj => ResourceManager.GetString("XamlCompiler_NotConstructibleObj", resourceCulture);

	internal static string XamlCompiler_NoXamlGiven => ResourceManager.GetString("XamlCompiler_NoXamlGiven", resourceCulture);

	internal static string XamlCompiler_NullablePropertyType => ResourceManager.GetString("XamlCompiler_NullablePropertyType", resourceCulture);

	internal static string XamlCompiler_OnlyOneLocalAssembly => ResourceManager.GetString("XamlCompiler_OnlyOneLocalAssembly", resourceCulture);

	internal static string XamlCompiler_PhaseMustBeUsedWithinADataTemplate => ResourceManager.GetString("XamlCompiler_PhaseMustBeUsedWithinADataTemplate", resourceCulture);

	internal static string XamlCompiler_PhaseMustHaveAssociatedBind => ResourceManager.GetString("XamlCompiler_PhaseMustHaveAssociatedBind", resourceCulture);

	internal static string XamlCompiler_PlatformUnsupported => ResourceManager.GetString("XamlCompiler_PlatformUnsupported", resourceCulture);

	internal static string XamlCompiler_Preview => ResourceManager.GetString("XamlCompiler_Preview", resourceCulture);

	internal static string XamlCompiler_SetterMustHaveValue => ResourceManager.GetString("XamlCompiler_SetterMustHaveValue", resourceCulture);

	internal static string XamlCompiler_SetterPropertyMustBeDP => ResourceManager.GetString("XamlCompiler_SetterPropertyMustBeDP", resourceCulture);

	internal static string XamlCompiler_SettersMustHaveProperty => ResourceManager.GetString("XamlCompiler_SettersMustHaveProperty", resourceCulture);

	internal static string XamlCompiler_SetterValueWrongType => ResourceManager.GetString("XamlCompiler_SetterValueWrongType", resourceCulture);

	internal static string XamlCompiler_StyleBasedOnBadStyleTargetType => ResourceManager.GetString("XamlCompiler_StyleBasedOnBadStyleTargetType", resourceCulture);

	internal static string XamlCompiler_StyleBasedOnMustBeStyle_BadObj => ResourceManager.GetString("XamlCompiler_StyleBasedOnMustBeStyle_BadObj", resourceCulture);

	internal static string XamlCompiler_StyleBasedOnMustBeStyle_SR => ResourceManager.GetString("XamlCompiler_StyleBasedOnMustBeStyle_SR", resourceCulture);

	internal static string XamlCompiler_StyleBasedOnMustBeStyle_Text => ResourceManager.GetString("XamlCompiler_StyleBasedOnMustBeStyle_Text", resourceCulture);

	internal static string XamlCompiler_StyleMustHaveTargetType => ResourceManager.GetString("XamlCompiler_StyleMustHaveTargetType", resourceCulture);

	internal static string XamlCompiler_TwoWayTargetNotADependencyProperty => ResourceManager.GetString("XamlCompiler_TwoWayTargetNotADependencyProperty", resourceCulture);

	internal static string XamlCompiler_TypeContractDoesNotExist => ResourceManager.GetString("XamlCompiler_TypeContractDoesNotExist", resourceCulture);

	internal static string XamlCompiler_TypeMustHaveANamespace => ResourceManager.GetString("XamlCompiler_TypeMustHaveANamespace", resourceCulture);

	internal static string XamlCompiler_UnknownAttachableMember => ResourceManager.GetString("XamlCompiler_UnknownAttachableMember", resourceCulture);

	internal static string XamlCompiler_UnknownMember => ResourceManager.GetString("XamlCompiler_UnknownMember", resourceCulture);

	internal static string XamlCompiler_UnknownObject => ResourceManager.GetString("XamlCompiler_UnknownObject", resourceCulture);

	internal static string XamlCompiler_UnknownSetterAttachableMember => ResourceManager.GetString("XamlCompiler_UnknownSetterAttachableMember", resourceCulture);

	internal static string XamlCompiler_UnknownStyleTargetType => ResourceManager.GetString("XamlCompiler_UnknownStyleTargetType", resourceCulture);

	internal static string XamlCompiler_UnknownTypeError => ResourceManager.GetString("XamlCompiler_UnknownTypeError", resourceCulture);

	internal static string XamlCompiler_UnresolvedForwardedTypeAssembly => ResourceManager.GetString("XamlCompiler_UnresolvedForwardedTypeAssembly", resourceCulture);

	internal static string XamlCompiler_WrongMemberContract => ResourceManager.GetString("XamlCompiler_WrongMemberContract", resourceCulture);

	internal static string XamlCompiler_WrongTypeContract => ResourceManager.GetString("XamlCompiler_WrongTypeContract", resourceCulture);

	internal static string XamlCompiler_XamlFileMustEndInDotXaml => ResourceManager.GetString("XamlCompiler_XamlFileMustEndInDotXaml", resourceCulture);

	internal static string XamlCompiler_XamlFilesHaveTheSameName => ResourceManager.GetString("XamlCompiler_XamlFilesHaveTheSameName", resourceCulture);

	internal static string XamlCompiler_XBindInsideXBind => ResourceManager.GetString("XamlCompiler_XBindInsideXBind", resourceCulture);

	internal static string XamlCompiler_XBindOnControlTemplate => ResourceManager.GetString("XamlCompiler_XBindOnControlTemplate", resourceCulture);

	internal static string XamlCompiler_XBindOutOfScopeUnsupported => ResourceManager.GetString("XamlCompiler_XBindOutOfScopeUnsupported", resourceCulture);

	internal static string XamlCompiler_XBindRootMustHaveLoading => ResourceManager.GetString("XamlCompiler_XBindRootMustHaveLoading", resourceCulture);

	internal static string XamlCompiler_XBindTargetNullValueOnNonNullableType => ResourceManager.GetString("XamlCompiler_XBindTargetNullValueOnNonNullableType", resourceCulture);

	internal static string XamlCompiler_XBindWithoutCodeBehind => ResourceManager.GetString("XamlCompiler_XBindWithoutCodeBehind", resourceCulture);

	internal static string XamlCompiler_XClassDerivesFromXClass => ResourceManager.GetString("XamlCompiler_XClassDerivesFromXClass", resourceCulture);

	internal static string XamlCompiler_xClassTypeDoesntMatchWinmd => ResourceManager.GetString("XamlCompiler_xClassTypeDoesntMatchWinmd", resourceCulture);

	internal static string XamlCompiler_xClassTypeIsNotFound => ResourceManager.GetString("XamlCompiler_xClassTypeIsNotFound", resourceCulture);

	internal static string XamlCompiler_XPropertyUsageNotSupportedForLanguage => ResourceManager.GetString("XamlCompiler_XPropertyUsageNotSupportedForLanguage", resourceCulture);

	internal static string XamlDom_IncorrectMemberConstructor => ResourceManager.GetString("XamlDom_IncorrectMemberConstructor", resourceCulture);

	internal static string XamlDom_MemberDifferentSchemas => ResourceManager.GetString("XamlDom_MemberDifferentSchemas", resourceCulture);

	internal static string XamlDom_MemberHasMoreThanOneItem => ResourceManager.GetString("XamlDom_MemberHasMoreThanOneItem", resourceCulture);

	internal static string XamlDom_SealedNamespaceCollection => ResourceManager.GetString("XamlDom_SealedNamespaceCollection", resourceCulture);

	internal static string XamlDom_SealedXamlDomNode => ResourceManager.GetString("XamlDom_SealedXamlDomNode", resourceCulture);

	internal static string XamlDom_TypeDifferentSchemas => ResourceManager.GetString("XamlDom_TypeDifferentSchemas", resourceCulture);

	internal static string XamlDom_UnknownAttachableMember => ResourceManager.GetString("XamlDom_UnknownAttachableMember", resourceCulture);

	internal static string XamlDom_UseHasAttachedMember => ResourceManager.GetString("XamlDom_UseHasAttachedMember", resourceCulture);

	internal static string XamlDom_UseHasGetAttachedMember => ResourceManager.GetString("XamlDom_UseHasGetAttachedMember", resourceCulture);

	internal static string XamlInternlError => ResourceManager.GetString("XamlInternlError", resourceCulture);

	internal static string XamlRewriter_CompiledBindingsCannotBeInElementForm => ResourceManager.GetString("XamlRewriter_CompiledBindingsCannotBeInElementForm", resourceCulture);

	internal static string XamlRewriter_EventsAcrossLine => ResourceManager.GetString("XamlRewriter_EventsAcrossLine", resourceCulture);

	internal static string XamlRewriter_EventsCannotBeInElementForm => ResourceManager.GetString("XamlRewriter_EventsCannotBeInElementForm", resourceCulture);

	internal static string XamlRewriter_XamlRewriterErrorDataTypeLongForm => ResourceManager.GetString("XamlRewriter_XamlRewriterErrorDataTypeLongForm", resourceCulture);

	internal static string XamlValidationError_AmbiguousEvent => ResourceManager.GetString("XamlValidationError_AmbiguousEvent", resourceCulture);

	internal static string XamlValidationError_DataTypeOnlyAllowedOnDataTemplate => ResourceManager.GetString("XamlValidationError_DataTypeOnlyAllowedOnDataTemplate", resourceCulture);

	internal static string XamlValidationError_DefaultBindModeInvalidValue => ResourceManager.GetString("XamlValidationError_DefaultBindModeInvalidValue", resourceCulture);

	internal static string XamlValidationError_DeferLoadStrategyInvalidValue => ResourceManager.GetString("XamlValidationError_DeferLoadStrategyInvalidValue", resourceCulture);

	internal static string XamlValidationError_InvalidAttributeValue => ResourceManager.GetString("XamlValidationError_InvalidAttributeValue", resourceCulture);

	internal static string XamlValidationError_SuccinctSyntaxError => ResourceManager.GetString("XamlValidationError_SuccinctSyntaxError", resourceCulture);

	internal static string XamlXmlParsingError => ResourceManager.GetString("XamlXmlParsingError", resourceCulture);

	internal static string XbfGeneration_CouldNotLoadXbfGenerator => ResourceManager.GetString("XbfGeneration_CouldNotLoadXbfGenerator", resourceCulture);

	internal static string XbfGeneration_GeneralFailure => ResourceManager.GetString("XbfGeneration_GeneralFailure", resourceCulture);

	internal static string XbfGeneration_MissingGenXbfPath => ResourceManager.GetString("XbfGeneration_MissingGenXbfPath", resourceCulture);

	internal static string XbfGeneration_MissingXbfApi => ResourceManager.GetString("XbfGeneration_MissingXbfApi", resourceCulture);

	internal static string XbfGeneration_PropertyNotFoundError => ResourceManager.GetString("XbfGeneration_PropertyNotFoundError", resourceCulture);

	internal static string XbfGeneration_SyntaxError => ResourceManager.GetString("XbfGeneration_SyntaxError", resourceCulture);

	internal static string XbfGeneration_SyntaxErrorME => ResourceManager.GetString("XbfGeneration_SyntaxErrorME", resourceCulture);

	internal static string XbfGeneration_XamlInputFileOpenFailure => ResourceManager.GetString("XbfGeneration_XamlInputFileOpenFailure", resourceCulture);

	internal static string XbfGeneration_XbfOutputFileOpenFailure => ResourceManager.GetString("XbfGeneration_XbfOutputFileOpenFailure", resourceCulture);

	internal XamlCompilerResources()
	{
	}
}
