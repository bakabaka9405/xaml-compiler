using System.Collections.Generic;

namespace Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;

public sealed class CompilerInputs
{
	public string ProjectPath { get; set; }

	public string Language { get; set; }

	public string LanguageSourceExtension { get; set; }

	public string OutputPath { get; set; }

	public List<MSBuildItem> ReferenceAssemblies { get; set; }

	public string TargetPlatformMinVersion { get; set; }

	public List<MSBuildItem> ReferenceAssemblyPaths { get; set; }

	public string BuildConfiguration { get; set; }

	public bool ForceSharedStateShutdown { get; set; }

	public bool DisableXbfGeneration { get; set; }

	public bool DisableXbfLineInfo { get; set; }

	public bool EnableXBindDiagnostics { get; set; }

	public List<MSBuildItem> ClIncludeFiles { get; set; }

	public string CIncludeDirectories { get; set; }

	public List<MSBuildItem> XamlApplications { get; set; }

	public List<MSBuildItem> XamlPages { get; set; }

	public List<MSBuildItem> LocalAssembly { get; set; }

	public List<MSBuildItem> SdkXamlPages { get; set; }

	public string ProjectName { get; set; }

	public bool IsPass1 { get; set; }

	public string RootNamespace { get; set; }

	public string OutputType { get; set; }

	public string PriIndexName { get; set; }

	public string CodeGenerationControlFlags { get; set; }

	public string FeatureControlFlags { get; set; }

	public bool XAMLFingerprint { get; set; }

	public bool UseVCMetaManaged { get; set; } = true;

	public string[] FingerprintIgnorePaths { get; set; }

	public string VCInstallDir { get; set; }

	public string VCInstallPath32 { get; set; }

	public string VCInstallPath64 { get; set; }

	public string WindowsSdkPath { get; set; }

	public string CompileMode { get; set; }

	public string SavedStateFile { get; set; }

	public string RootsLog { get; set; }

	public string SuppressWarnings { get; set; }

	public string GenXbfPath { get; set; }

	public string PrecompiledHeaderFile { get; set; }

	public string XamlResourceMapName { get; set; }

	public string XamlComponentResourceLocation { get; set; }

	public string XamlPlatform { get; set; }

	public string TargetFileName { get; set; }

	public bool IgnoreSpecifiedTargetPlatformMinVersion { get; set; }
}
