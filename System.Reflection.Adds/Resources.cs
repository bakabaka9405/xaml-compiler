using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace System.Reflection.Adds;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
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
				ResourceManager resourceManager = new ResourceManager("Microsoft.UI.Xaml.Markup.Compiler.Microsoft.Lmr.ReflectionAdds.Resources", typeof(Resources).Assembly);
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

	internal static string ArrayInsideArrayInAttributeNotSupported => ResourceManager.GetString("ArrayInsideArrayInAttributeNotSupported", resourceCulture);

	internal static string AssemblyRefTokenExpected => ResourceManager.GetString("AssemblyRefTokenExpected", resourceCulture);

	internal static string CannotDetermineSystemAssembly => ResourceManager.GetString("CannotDetermineSystemAssembly", resourceCulture);

	internal static string CannotFindTypeInModule => ResourceManager.GetString("CannotFindTypeInModule", resourceCulture);

	internal static string CannotResolveModuleRefOnNetModule => ResourceManager.GetString("CannotResolveModuleRefOnNetModule", resourceCulture);

	internal static string CannotResolveRVA => ResourceManager.GetString("CannotResolveRVA", resourceCulture);

	internal static string CaseInsensitiveTypeLookupNotImplemented => ResourceManager.GetString("CaseInsensitiveTypeLookupNotImplemented", resourceCulture);

	internal static string CorruptImage => ResourceManager.GetString("CorruptImage", resourceCulture);

	internal static string DefaultTokenResolverRequired => ResourceManager.GetString("DefaultTokenResolverRequired", resourceCulture);

	internal static string DifferentTokenResolverForOuterType => ResourceManager.GetString("DifferentTokenResolverForOuterType", resourceCulture);

	internal static string EscapeSequenceMissingCharacter => ResourceManager.GetString("EscapeSequenceMissingCharacter", resourceCulture);

	internal static string ExpectedPositiveNumberOfGenericParameters => ResourceManager.GetString("ExpectedPositiveNumberOfGenericParameters", resourceCulture);

	internal static string ExpectedPropertyOrFieldId => ResourceManager.GetString("ExpectedPropertyOrFieldId", resourceCulture);

	internal static string ExpectedTokenType => ResourceManager.GetString("ExpectedTokenType", resourceCulture);

	internal static string ExtraAssemblyManifest => ResourceManager.GetString("ExtraAssemblyManifest", resourceCulture);

	internal static string ExtraCharactersAfterTypeName => ResourceManager.GetString("ExtraCharactersAfterTypeName", resourceCulture);

	internal static string ExtraInformationAfterLastParameter => ResourceManager.GetString("ExtraInformationAfterLastParameter", resourceCulture);

	internal static string HostSpecifierMissing => ResourceManager.GetString("HostSpecifierMissing", resourceCulture);

	internal static string IdTokenTypeExpected => ResourceManager.GetString("IdTokenTypeExpected", resourceCulture);

	internal static string IllegalElementType => ResourceManager.GetString("IllegalElementType", resourceCulture);

	internal static string IllegalLayoutMask => ResourceManager.GetString("IllegalLayoutMask", resourceCulture);

	internal static string IncorrectElementTypeValue => ResourceManager.GetString("IncorrectElementTypeValue", resourceCulture);

	internal static string InvalidCustomAttributeFormat => ResourceManager.GetString("InvalidCustomAttributeFormat", resourceCulture);

	internal static string InvalidCustomAttributeFormatForEnum => ResourceManager.GetString("InvalidCustomAttributeFormatForEnum", resourceCulture);

	internal static string InvalidElementTypeInAttribute => ResourceManager.GetString("InvalidElementTypeInAttribute", resourceCulture);

	internal static string InvalidFileFormat => ResourceManager.GetString("InvalidFileFormat", resourceCulture);

	internal static string InvalidFileName => ResourceManager.GetString("InvalidFileName", resourceCulture);

	internal static string InvalidMetadata => ResourceManager.GetString("InvalidMetadata", resourceCulture);

	internal static string InvalidMetadataSignature => ResourceManager.GetString("InvalidMetadataSignature", resourceCulture);

	internal static string InvalidMetadataToken => ResourceManager.GetString("InvalidMetadataToken", resourceCulture);

	internal static string InvalidPublicKeyTokenLength => ResourceManager.GetString("InvalidPublicKeyTokenLength", resourceCulture);

	internal static string JaggedArrayInAttributeNotSupported => ResourceManager.GetString("JaggedArrayInAttributeNotSupported", resourceCulture);

	internal static string ManifestModuleMustBeProvided => ResourceManager.GetString("ManifestModuleMustBeProvided", resourceCulture);

	internal static string MethodIsUsingUnsupportedBindingFlags => ResourceManager.GetString("MethodIsUsingUnsupportedBindingFlags", resourceCulture);

	internal static string MethodTokenExpected => ResourceManager.GetString("MethodTokenExpected", resourceCulture);

	internal static string NoAssemblyManifest => ResourceManager.GetString("NoAssemblyManifest", resourceCulture);

	internal static string OperationInvalidOnAutoLayoutFields => ResourceManager.GetString("OperationInvalidOnAutoLayoutFields", resourceCulture);

	internal static string OperationValidOnArrayTypeOnly => ResourceManager.GetString("OperationValidOnArrayTypeOnly", resourceCulture);

	internal static string OperationValidOnEnumOnly => ResourceManager.GetString("OperationValidOnEnumOnly", resourceCulture);

	internal static string OperationValidOnLiteralFieldsOnly => ResourceManager.GetString("OperationValidOnLiteralFieldsOnly", resourceCulture);

	internal static string OperationValidOnRVAFieldsOnly => ResourceManager.GetString("OperationValidOnRVAFieldsOnly", resourceCulture);

	internal static string ResolvedAssemblyMustBeWithinSameUniverse => ResourceManager.GetString("ResolvedAssemblyMustBeWithinSameUniverse", resourceCulture);

	internal static string ResolverMustResolveToValidAssembly => ResourceManager.GetString("ResolverMustResolveToValidAssembly", resourceCulture);

	internal static string ResolverMustResolveToValidModule => ResourceManager.GetString("ResolverMustResolveToValidModule", resourceCulture);

	internal static string ResolverMustSetAssemblyProperty => ResourceManager.GetString("ResolverMustSetAssemblyProperty", resourceCulture);

	internal static string RVAUnsupported => ResourceManager.GetString("RVAUnsupported", resourceCulture);

	internal static string TypeArgumentCannotBeResolved => ResourceManager.GetString("TypeArgumentCannotBeResolved", resourceCulture);

	internal static string TypeTokenExpected => ResourceManager.GetString("TypeTokenExpected", resourceCulture);

	internal static string UnexpectedCharacterFound => ResourceManager.GetString("UnexpectedCharacterFound", resourceCulture);

	internal static string UnexpectedEndOfInput => ResourceManager.GetString("UnexpectedEndOfInput", resourceCulture);

	internal static string UniverseCannotResolveAssembly => ResourceManager.GetString("UniverseCannotResolveAssembly", resourceCulture);

	internal static string UnrecognizedAssemblyAttribute => ResourceManager.GetString("UnrecognizedAssemblyAttribute", resourceCulture);

	internal static string UnsupportedExceptionFlags => ResourceManager.GetString("UnsupportedExceptionFlags", resourceCulture);

	internal static string UnsupportedImageType => ResourceManager.GetString("UnsupportedImageType", resourceCulture);

	internal static string UnsupportedTypeInAttributeSignature => ResourceManager.GetString("UnsupportedTypeInAttributeSignature", resourceCulture);

	internal static string ValidOnGenericParameterTypeOnly => ResourceManager.GetString("ValidOnGenericParameterTypeOnly", resourceCulture);

	internal static string VarargSignaturesNotImplemented => ResourceManager.GetString("VarargSignaturesNotImplemented", resourceCulture);

	internal static string VersionAlreadyDefined => ResourceManager.GetString("VersionAlreadyDefined", resourceCulture);

	internal static string WindowsRuntimeTypeNotFound => ResourceManager.GetString("WindowsRuntimeTypeNotFound", resourceCulture);

	internal static string WrongNumberOfGenericArguments => ResourceManager.GetString("WrongNumberOfGenericArguments", resourceCulture);

	internal Resources()
	{
	}
}
