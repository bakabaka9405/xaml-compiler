using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlProjectInfo
{
	public IEnumerable<IFileItem> AdditionalXamlTypeInfoIncludes { get; set; }

	public string RootNamespace { get; set; }

	public string ProjectName { get; set; }

	public bool IsLibrary { get; set; }

	public bool IsCLSCompliant { get; set; }

	public CodeGenCtrlFlags CodeGenFlags { get; set; }

	public bool ShouldGenerateDisableXBind { get; set; }

	public Dictionary<string, string> ClassToHeaderFileMap { get; set; }

	public string GenXbf32Path { get; set; }

	public string GenXbf64Path { get; set; }

	public string GenXbfArm64Path { get; set; }

	public bool VSDesignerDontLoadAsDll { get; set; }

	public bool EnableTypeInfoReflection { get; set; }

	public bool EnableDefaultValidationContextGeneration { get; set; }

	public bool GenerateIncrementalTypeInfo => HasCodeGenFlag(CodeGenCtrlFlags.IncrementalTypeInfoCodeGen);

	public bool GenerateProviderCode => !HasCodeGenFlag(CodeGenCtrlFlags.DoNotGenerateOtherProviders);

	public bool GenerateCppWinRTStaticAsserts => !HasCodeGenFlag(CodeGenCtrlFlags.DoNotGenerateCppWinRTStaticAsserts);

	public bool GenerateOtherProvidersForCX
	{
		get
		{
			if (IsLibrary)
			{
				return GenerateFullXamlMetadataProvider;
			}
			return true;
		}
	}

	public bool GenerateFullXamlMetadataProvider => HasCodeGenFlag(CodeGenCtrlFlags.FullXamlMetadataProvider);

	public bool IsInputValidationEnabled { get; set; }

	public Version TargetPlatformMinVersion { get; set; }

	public string XamlTypeInfoNamespace
	{
		get
		{
			string text = (string.IsNullOrWhiteSpace(RootNamespace) ? "XamlDefaultRootNamespace" : RootNamespace);
			string text2 = (string.IsNullOrWhiteSpace(ProjectName) ? "XamlDefaultProjectName" : ProjectName);
			return text + "." + text2 + "_XamlTypeInfo";
		}
	}

	public string XamlTypeInfoReflectionNamespace => "Microsoft.UI.Xaml.Markup";

	public bool IsWin32App { get; set; }

	public bool UsingCSWinRT { get; set; }

	public string PrecompiledHeaderFile { get; set; }

	public bool HasCodeGenFlag(CodeGenCtrlFlags flag)
	{
		return (CodeGenFlags & flag) == flag;
	}
}
