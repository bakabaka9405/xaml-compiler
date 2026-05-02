using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Adds;
using System.Text;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.Core;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.Lmr;
using Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.RootLog;
using Microsoft.UI.Xaml.Markup.Compiler.Tracing;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;
using Microsoft.UI.Xaml.Markup.Compiler.XBF;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class CompileXamlInternal
{
	private static readonly char[] _separator = new char[2]
	{
		Path.DirectorySeparatorChar,
		Path.AltDirectorySeparatorChar
	};

	private List<IXbfFileNameInfo> _newlyGeneratedXamlFiles = new List<IXbfFileNameInfo>();

	private List<Assembly> _loadedAssemblies;

	private List<Assembly> _loadedSystemAssemblies;

	private List<Assembly> _loadedNonSystemAssemblies;

	private List<string> _systemExtraReferenceItems;

	private SourceFileManager _SourceFileManager;

	private string _projectFolderFullpath;

	private List<string> _includeFolderList;

	private Assembly _localAssembly;

	private string _localAssemblyName;

	private DirectUISchemaContext _schemaContext;

	private IXbfMetadataProvider _xamlMetadataProvider;

	private TypeInfoCollector _typeInfoCollector;

	private XamlProjectInfo _projectInfo;

	private static XamlTypeUniverse s_typeUniverse;

	private static TypeResolver s_typeResolver;

	private Dictionary<string, XamlClassCodeInfo> _classCodeInfos = new Dictionary<string, XamlClassCodeInfo>();

	private List<string> _suppressedWarnings = new List<string>();

	private string _xamlPlatformString;

	private XamlCodeGenerator _codeGenerator;

	internal IList<string> _generatedCodeFiles = new List<string>();

	internal IList<string> _generatedXamlFiles = new List<string>();

	internal IList<string> _generatedXbfFiles = new List<string>();

	internal IList<string> _generatedXamlPagesFiles = new List<string>();

	public bool DisableXbfGeneration { get; set; }

	public bool Fingerprint { get; set; }

	public bool UseVCMetaManaged { get; set; } = true;

	public string[] FingerprintIgnorePaths { get; set; }

	public string VCInstallDir { get; set; }

	public string VCInstallPath32 { get; set; }

	public string VCInstallPath64 { get; set; }

	public string WindowsSdkPath { get; set; }

	public string[] SuppressWarnings { get; set; }

	public string ProjectPath { get; set; }

	public string RootsLog { get; set; }

	public string TargetPlatformMinVersion { get; set; }

	public Language Language { get; set; }

	public string LanguageSourceExtension { get; set; }

	public string OutputPath { get; set; }

	public IList<IAssemblyItem> ReferenceAssemblies { get; set; }

	public string[] ReferenceAssemblyPaths { get; set; }

	public string[] CIncludeDirectories { get; set; }

	public IList<IFileItem> ClIncludeFiles { get; set; }

	public IList<IFileItem> XamlApplications { get; set; }

	public IList<IFileItem> XamlPages { get; set; }

	public IList<IFileItem> SdkXamlPages { get; set; }

	public IAssemblyItem LocalAssembly { get; set; }

	public string ProjectName { get; set; }

	public bool IsPass1 { get; set; }

	public bool IsDesignTimeBuild { get; set; }

	public string RootNamespace { get; set; }

	public string OutputType { get; set; }

	public string PriIndexName { get; set; }

	public CodeGenCtrlFlags CodeGenerationControlFlags { get; set; }

	public FeatureCtrlFlags FeatureControlFlags { get; set; }

	public ILog Log { get; set; }

	public uint XbfGenerationFlags { get; set; }

	public string XamlResourceMapName { get; set; }

	public string XamlComponentResourceLocation { get; set; }

	public string GenXbfPath { get; set; }

	public string PrecompiledHeaderFile { get; set; }

	public string XamlPlatformString
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_xamlPlatformString))
			{
				return "UWP";
			}
			return _xamlPlatformString;
		}
		set
		{
			_xamlPlatformString = value;
		}
	}

	public Platform XamlPlatform
	{
		get
		{
			if (Enum.TryParse<Platform>(XamlPlatformString, out var result))
			{
				return result;
			}
			throw new ArgumentOutOfRangeException(ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_PlatformUnsupported, XamlPlatformString));
		}
	}

	public bool EnableXBindDiagnostics { get; private set; }

	public bool EnableTypeInfoReflection { get; private set; }

	public bool EnableDefaultValidationContextGeneration { get; private set; }

	public bool EnableWin32Codegen { get; private set; }

	public bool UsingCSWinRT { get; private set; }

	public bool EnableBindingDiagnostics { get; private set; }

	public bool IgnoreSpecifiedTargetPlatformMinVersion { get; set; }

	public IList<string> GeneratedCodeFiles => _generatedCodeFiles;

	public IList<string> GeneratedXamlFiles => _generatedXamlFiles;

	public IList<string> GeneratedXbfFiles => _generatedXbfFiles;

	public IList<string> GeneratedXamlPagesFiles => _generatedXamlPagesFiles;

	internal SavedStateManager SaveState { get; set; }

	internal string ProjectFolderFullpath
	{
		get
		{
			if (_projectFolderFullpath == null)
			{
				_projectFolderFullpath = Path.GetDirectoryName(ProjectPath);
			}
			return _projectFolderFullpath;
		}
	}

	internal List<string> IncludeFolderList
	{
		get
		{
			if (_includeFolderList == null)
			{
				string directoryName = Path.GetDirectoryName(ProjectPath);
				_includeFolderList = XamlHelper.EnsureFullpaths(CIncludeDirectories, ProjectFolderFullpath);
			}
			return _includeFolderList;
		}
	}

	internal string GeneratedExtension
	{
		get
		{
			if (!IsPass1)
			{
				return Language.Pass2Extension;
			}
			return Language.Pass1Extension;
		}
	}

	internal string OutputFolderFullpath
	{
		get
		{
			if (!Path.IsPathRooted(OutputPath))
			{
				return Path.Combine(Path.GetDirectoryName(ProjectPath), OutputPath);
			}
			return OutputPath;
		}
	}

	private SourceFileManager SourceFileManager
	{
		get
		{
			if (_SourceFileManager == null)
			{
				_SourceFileManager = new SourceFileManager(this);
			}
			return _SourceFileManager;
		}
	}

	private bool IsOutputTypeLibrary => KS.EqIgnoreCase(OutputType, "Library");

	private bool IsOutputTypeWinMd => KS.EqIgnoreCase(OutputType, "WinMdObj");

	internal BuildTaskFileService TaskFileService { get; set; }

	public void PopulateFromCompilerInputs(CompilerInputs i)
	{
		FeatureControlFlags = TryParseFeatureFlags(i.FeatureControlFlags);
		ClIncludeFiles = GetFileItems(i.ClIncludeFiles);
		CIncludeDirectories = ((i.CIncludeDirectories != null) ? i.CIncludeDirectories.Split(';') : null);
		CodeGenerationControlFlags = TryParseCodeGenFlags(i.CodeGenerationControlFlags);
		DisableXbfGeneration = i.DisableXbfGeneration;
		Fingerprint = i.XAMLFingerprint;
		UseVCMetaManaged = i.UseVCMetaManaged;
		FingerprintIgnorePaths = i.FingerprintIgnorePaths;
		IsPass1 = i.IsPass1;
		GenXbfPath = i.GenXbfPath;
		PrecompiledHeaderFile = i.PrecompiledHeaderFile;
		Language = Language.Parse(i.Language);
		LanguageSourceExtension = i.LanguageSourceExtension;
		LocalAssembly = ((i.LocalAssembly?[0] != null) ? i.LocalAssembly[0] : null);
		ProjectName = i.ProjectName;
		OutputPath = i.OutputPath;
		OutputType = i.OutputType;
		PriIndexName = i.PriIndexName;
		ProjectPath = i.ProjectPath;
		ReferenceAssemblies = GetAssemblyItems(i.ReferenceAssemblies);
		ReferenceAssemblyPaths = GetStringsFromItems(i.ReferenceAssemblyPaths);
		if (ReferenceAssemblies == null)
		{
			ReferenceAssemblyPaths = GetStringsFromItems(i.ReferenceAssemblies);
		}
		RootNamespace = i.RootNamespace;
		RootsLog = i.RootsLog;
		TargetPlatformMinVersion = i.TargetPlatformMinVersion;
		VCInstallDir = i.VCInstallDir;
		VCInstallPath32 = i.VCInstallPath32;
		VCInstallPath64 = i.VCInstallPath64;
		SuppressWarnings = ((i.SuppressWarnings != null) ? i.SuppressWarnings.Split(';') : null);
		WindowsSdkPath = i.WindowsSdkPath;
		XbfGenerationFlags = (i.DisableXbfLineInfo ? 1u : 0u);
		XamlResourceMapName = i.XamlResourceMapName;
		XamlComponentResourceLocation = i.XamlComponentResourceLocation;
		XamlPlatformString = i.XamlPlatform ?? "UWP";
		EnableXBindDiagnostics = FeatureControlFlags.HasFlag(FeatureCtrlFlags.EnableXBindDiagnostics);
		EnableTypeInfoReflection = FeatureControlFlags.HasFlag(FeatureCtrlFlags.EnableTypeInfoReflection);
		EnableDefaultValidationContextGeneration = FeatureControlFlags.HasFlag(FeatureCtrlFlags.EnableDefaultValidationContextGeneration);
		EnableWin32Codegen = FeatureControlFlags.HasFlag(FeatureCtrlFlags.EnableWin32Codegen);
		UsingCSWinRT = FeatureControlFlags.HasFlag(FeatureCtrlFlags.UsingCSWinRT);
		EnableBindingDiagnostics = FeatureControlFlags.HasFlag(FeatureCtrlFlags.EnableBindingDiagnostics);
		IgnoreSpecifiedTargetPlatformMinVersion = IgnoreSpecifiedTargetPlatformMinVersion;
		XamlApplications = GetFileItems(i.XamlApplications);
		XamlPages = GetFileItems(i.XamlPages);
		SdkXamlPages = GetFileItems(i.SdkXamlPages);
	}

	private IList<IFileItem> GetFileItems(List<MSBuildItem> list)
	{
		List<IFileItem> list2 = new List<IFileItem>();
		if (list != null && list.Count > 0)
		{
			foreach (MSBuildItem item in list)
			{
				list2.Add(item);
			}
		}
		return list2;
	}

	private IList<IAssemblyItem> GetAssemblyItems(List<MSBuildItem> list)
	{
		List<IAssemblyItem> list2 = new List<IAssemblyItem>();
		foreach (MSBuildItem item in list)
		{
			list2.Add(item);
		}
		return list2;
	}

	internal static string[] GetStringsFromItems(List<MSBuildItem> items)
	{
		if (items == null)
		{
			return null;
		}
		string[] array = new string[items.Count];
		for (int i = 0; i < items.Count; i++)
		{
			array[i] = items[i].ItemSpec;
		}
		return array;
	}

	private CodeGenCtrlFlags TryParseCodeGenFlags(string flags)
	{
		CodeGenCtrlFlags codeGenCtrlFlags = CodeGenCtrlFlags.Nothing;
		if (!string.IsNullOrWhiteSpace(flags))
		{
			string[] array = flags.Split(';');
			foreach (string text in array)
			{
				if (!Enum.TryParse<CodeGenCtrlFlags>(text, out var result))
				{
					LogError_BadCodeGenFlags(text);
				}
				codeGenCtrlFlags |= result;
			}
		}
		return codeGenCtrlFlags;
	}

	private FeatureCtrlFlags TryParseFeatureFlags(string flags)
	{
		FeatureCtrlFlags featureCtrlFlags = FeatureCtrlFlags.Nothing;
		if (!string.IsNullOrWhiteSpace(flags))
		{
			string[] array = flags.Split(';');
			foreach (string text in array)
			{
				if (!Enum.TryParse<FeatureCtrlFlags>(text, out var result))
				{
					LogError_BadCodeGenFlags(text);
				}
				featureCtrlFlags |= result;
			}
		}
		return featureCtrlFlags;
	}

	private void CleanUpSavedState()
	{
		List<string> list = new List<string>();
		foreach (string key in SaveState.XamlPerFileInfo.Keys)
		{
			bool flag = false;
			foreach (TaskItemFilename projectXamlTaskItem in SourceFileManager.ProjectXamlTaskItems)
			{
				if (key == projectXamlTaskItem.XamlGivenPath)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				list.Add(key);
			}
		}
		foreach (string item in list)
		{
			SaveState.XamlPerFileInfo.Remove(item);
		}
	}

	internal bool DidAssembliesChange()
	{
		if (!Fingerprint)
		{
			return true;
		}
		FingerPrinter fingerPrinter = new FingerPrinter(LocalAssembly, ReferenceAssemblies, FingerprintIgnorePaths, VCInstallDir, VCInstallPath32, VCInstallPath64, UseVCMetaManaged);
		if (string.IsNullOrEmpty(SaveState.LocalAssemblyName))
		{
			SaveState.LocalAssemblyName = fingerPrinter.LocalAssemblyPath;
		}
		bool flag = fingerPrinter.HasAssemblyFileListChanged(SaveState.ReferenceAssemblyList);
		bool flag2 = fingerPrinter.HasLocalAssemblyHashChanged(SaveState.ReferenceAssemblyGuids);
		bool flag3 = fingerPrinter.HaveReferenceAssembliesHashesChanged(SaveState.ReferenceAssemblyGuids);
		if (flag)
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "List of Assemblies Changed");
		}
		if (flag2)
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "Local Assembly FingerPrint Changed");
			if (SaveState.ReferenceAssemblyGuids.TryGetValue(SaveState.LocalAssemblyName, out var value))
			{
				PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, $"{value.ToString()} - {SaveState.LocalAssemblyName}");
			}
		}
		if (flag3)
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_FingerprintCheck, "Reference Assemblies Fingerprint Changed");
		}
		return flag || flag2 || flag3;
	}

	public bool DidXAMLFilesChange()
	{
		if (CodeGenerationControlFlags.HasFlag(CodeGenCtrlFlags.NoTypeInfoCodeGen))
		{
			return false;
		}
		bool result = false;
		foreach (TaskItemFilename projectXamlTaskItem in SourceFileManager.ProjectXamlTaskItems)
		{
			if (projectXamlTaskItem.OutOfDate())
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private bool DidFeatureControlFlagsChange()
	{
		string text = FeatureControlFlags.ToString();
		if (string.Compare(SaveState.XamlFeatureControlFlags, text, StringComparison.OrdinalIgnoreCase) != 0)
		{
			SaveState.XamlFeatureControlFlags = text;
			return true;
		}
		return false;
	}

	private bool IsXAMLTypeInfoNeeded()
	{
		bool result = false;
		string text = Language.Pass2Extension;
		if (text == ".g.hpp")
		{
			text = ".g.cpp";
		}
		string fileName = Path.Combine(OutputFolderFullpath, "XamlTypeInfo" + text);
		FileInfo fileInfo = new FileInfo(fileName);
		if (!fileInfo.Exists)
		{
			result = true;
		}
		else if (Language.IsNative && fileInfo.Length == 0L)
		{
			result = true;
		}
		return result;
	}

	private void ReportExistingGeneratedXamlFiles(TaskItemFilename tif)
	{
		_generatedXamlFiles.Add(tif.XamlOutputFilename);
		if (!DisableXbfGeneration)
		{
			AddGeneratedXbfFile(tif.XbfOutputFilename);
		}
	}

	internal void ReportExistingGeneratedCodeFile(string targetFolder, string codeFileName)
	{
		string item = Path.Combine(targetFolder, codeFileName + Language.Pass1Extension);
		if (Language.IsNative)
		{
			_generatedCodeFiles.Add(item);
			if (!IsPass1 && CodeGenerationControlFlags.HasFlag(CodeGenCtrlFlags.IncrementalTypeInfoCodeGen))
			{
				string item2 = Path.Combine(targetFolder, codeFileName + Language.Pass2Extension);
				_generatedXamlPagesFiles.Add(item2);
			}
		}
		else if (IsPass1)
		{
			_generatedCodeFiles.Add(item);
			string text = Path.Combine(targetFolder, codeFileName + Language.Pass2Extension);
			CreateFileIfNecessary(text);
			_generatedCodeFiles.Add(text);
		}
	}

	internal bool ShortcutBackupRestoreGeneratedPass2Files_WhenNothingExternalHasChanged()
	{
		if (Language.IsNative)
		{
			return true;
		}
		if (IsPass1)
		{
			if (!IsDesignTimeBuild)
			{
				foreach (string item in SourceFileManager.CodeGenFiles.Select((ClassCodeGenFile cgf) => Path.Combine(cgf.TargetFolderFullPath, cgf.BaseFileName + Language.Pass2Extension)))
				{
					FileHelpers.BackupIfExistsAndTruncateToNull(item);
				}
			}
			return true;
		}
		bool flag = true;
		foreach (string item2 in SourceFileManager.CodeGenFiles.Select((ClassCodeGenFile cgf) => Path.Combine(cgf.TargetFolderFullPath, cgf.BaseFileName + Language.Pass2Extension)))
		{
			flag &= FileHelpers.RestoreBackupFile(item2);
		}
		foreach (TaskItemFilename projectXamlTaskItem in SourceFileManager.ProjectXamlTaskItems)
		{
			projectXamlTaskItem.Refresh(SaveState);
		}
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_RestoredGeneratedPass2CodeFileBackup);
		return flag;
	}

	internal bool ShortcutBackupRestoreXamlTypeInfoFile_WhenNothingExternalHasChanged()
	{
		if (Language.IsNative)
		{
			return true;
		}
		string text = Path.Combine(OutputFolderFullpath, "XamlTypeInfo" + Language.Pass2Extension);
		if (IsPass1)
		{
			if (!IsDesignTimeBuild)
			{
				FileHelpers.BackupIfExistsAndTruncateToNull(text);
			}
			_generatedCodeFiles.Add(text);
			return true;
		}
		bool result = FileHelpers.RestoreBackupFile(text);
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_RestoredTypeInfoBackup);
		return result;
	}

	internal void UpdateGeneratedFilesLists()
	{
		foreach (ClassCodeGenFile codeGenFile in SourceFileManager.CodeGenFiles)
		{
			ReportExistingGeneratedCodeFile(codeGenFile.TargetFolderFullPath, codeGenFile.BaseFileName);
		}
		foreach (TaskItemFilename projectXamlTaskItem in SourceFileManager.ProjectXamlTaskItems)
		{
			ReportExistingGeneratedXamlFiles(projectXamlTaskItem);
		}
		if (SourceFileManager.SdkXamlTaskItems != null && SourceFileManager.SdkXamlTaskItems.Count > 0 && !DisableXbfGeneration)
		{
			foreach (TaskItemFilename sdkXamlTaskItem in SourceFileManager.SdkXamlTaskItems)
			{
				AddGeneratedXbfFile(sdkXamlTaskItem.XbfOutputFilename);
			}
		}
		if (!Language.IsNative)
		{
			return;
		}
		List<string> list = new List<string>();
		if (IsPass1)
		{
			list.Add(Path.Combine(SourceFileManager.OutputFolderFullpath, "XamlMetaDataProvider.h"));
			list.Add(Path.Combine(SourceFileManager.OutputFolderFullpath, "XamlLibMetadataProvider.g.cpp"));
			list.Add(Path.Combine(SourceFileManager.OutputFolderFullpath, "XamlTypeInfo.Impl.g.cpp"));
		}
		else
		{
			list.Add(Path.Combine(SourceFileManager.OutputFolderFullpath, "XamlTypeInfo.g.cpp"));
		}
		foreach (string item in list)
		{
			if (File.Exists(item))
			{
				_generatedCodeFiles.Add(item);
			}
		}
	}

	public bool VerifyWorkDone()
	{
		if (IsPass1)
		{
			return true;
		}
		foreach (TaskItemFilename projectXamlTaskItem in SourceFileManager.ProjectXamlTaskItems)
		{
			projectXamlTaskItem.Refresh(SaveState);
		}
		return !DidXAMLFilesChange();
	}

	public void SaveStateBeforeFinishing()
	{
		SourceFileManager.SaveState();
	}

	public bool DoExecute()
	{
		bool flag = true;
		bool flag2 = false;
		PerformanceUtility.Initialize(Log);
		PerformanceUtility.FireCodeMarker((!IsPass1) ? CodeMarkerEvent.perfXC_StartPass2 : CodeMarkerEvent.perfXC_StartPass1, ProjectName);
		_generatedCodeFiles.Clear();
		_generatedXamlFiles.Clear();
		_generatedXbfFiles.Clear();
		_generatedXamlPagesFiles.Clear();
		if (s_typeUniverse != null && !string.Equals(s_typeUniverse.ProjectPath, ProjectPath))
		{
			UnloadReferences();
		}
		if (!BuildWarningSuppressionList())
		{
			return false;
		}
		if ((XamlApplications == null || !XamlApplications.Any()) && (XamlPages == null || XamlPages.Count == 0))
		{
			LogWarning(new XamlValidationWarningNoXaml());
			return true;
		}
		if (!CheckTaskArgumentsValid())
		{
			return false;
		}
		if (CodeGenerationControlFlags != CodeGenCtrlFlags.Nothing)
		{
			LogWarningAsInfo(new XamlValidationWarningUsingCodeGenFlags(CodeGenerationControlFlags));
		}
		if (Language.IsExperimental)
		{
			LogWarning(new XamlValidationWarningPreview(ErrorCode.WMC1502, Language.Name));
		}
		CleanUpSavedState();
		bool flag3 = false;
		bool flag4 = DidAssembliesChange();
		bool flag5 = IsXAMLTypeInfoNeeded();
		bool flag6 = DidFeatureControlFlagsChange();
		bool enableTypeInfoReflection = EnableTypeInfoReflection;
		if (!flag5 && !flag4 && !flag6)
		{
			bool flag7 = ShortcutBackupRestoreGeneratedPass2Files_WhenNothingExternalHasChanged();
			if (!DidXAMLFilesChange())
			{
				UpdateGeneratedFilesLists();
				flag3 = true;
				bool flag8 = ShortcutBackupRestoreXamlTypeInfoFile_WhenNothingExternalHasChanged();
				if (IsPass1 || (flag7 && flag8))
				{
					return true;
				}
			}
		}
		if (s_typeUniverse == null)
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_CreatingTypeUniverse);
			s_typeUniverse = new XamlTypeUniverse(Language.IsManaged);
			s_typeUniverse.ProjectPath = ProjectPath;
			s_typeUniverse.ReferenceAssemblyPaths = ReferenceAssemblyPaths;
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_CreatedTypeUniverse);
		}
		SetupLmrAssemblyResolveHandler();
		try
		{
			_schemaContext = LoadSchemaContext();
			if (_schemaContext == null)
			{
				return false;
			}
			SourceFileManager.PropagateOutOfDateStatus(_schemaContext);
			_xamlMetadataProvider = new XbfMetadataProvider(_schemaContext);
			_projectInfo = GetProjectInfo();
			_typeInfoCollector = new TypeInfoCollector(_schemaContext, XamlPlatform, EnableBindingDiagnostics);
			Type iXamlType = GetIXamlType(_loadedAssemblies);
			if (iXamlType == null)
			{
				LogError_CannotResolveWinUIMetadata();
				return false;
			}
			foreach (TaskItemFilename projectXamlTaskItem in SourceFileManager.ProjectXamlTaskItems)
			{
				bool flag9 = flag4 || flag6;
				if (IsPass1 && !projectXamlTaskItem.OutOfDate() && !flag9)
				{
					ReportExistingGeneratedXamlFiles(projectXamlTaskItem);
					continue;
				}
				PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageStart, projectXamlTaskItem.XamlGivenPath);
				XamlDomObject xamlDomRoot = null;
				bool flag10 = false;
				if (!IsPass1 && (!enableTypeInfoReflection || projectXamlTaskItem.IsApplication))
				{
					flag10 |= !ProcessXamlFile_XamlTypeInfo(projectXamlTaskItem, ref xamlDomRoot);
				}
				if (projectXamlTaskItem.OutOfDate() || flag9)
				{
					flag10 |= !ProcessXamlFile_PerPageInfo(projectXamlTaskItem, ref xamlDomRoot);
				}
				PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageDone, projectXamlTaskItem.XamlGivenPath);
				if (flag10)
				{
					flag = false;
				}
				else if (IsPass1)
				{
					SaveState.SetXamlFileTimeAtLastCompile(projectXamlTaskItem.XamlGivenPath, projectXamlTaskItem.XamlLastChangeTime.Ticks);
					SaveState.SetClassFullName(projectXamlTaskItem.XamlGivenPath, projectXamlTaskItem.ClassFullName);
				}
			}
			if (!flag)
			{
				return false;
			}
			if (!IsPass1 && !ShouldSuppressTypeInfoCodeGen())
			{
				_typeInfoCollector.AddMetadataAndBindableTypes(_loadedNonSystemAssemblies, _localAssembly);
				if (!ReportSchemaErrors(string.Empty))
				{
					return false;
				}
				if (!enableTypeInfoReflection && !ValidateXamlTypeInfo())
				{
					return false;
				}
			}
			_codeGenerator = new XamlCodeGenerator(Language, IsPass1, _projectInfo, _typeInfoCollector.SchemaInfo);
			foreach (TaskItemFilename classlessXamlFile in SourceFileManager.ClasslessXamlFiles)
			{
				if (!GenerateClasslessXamlOutputFile(classlessXamlFile.SourceXamlFullPath, classlessXamlFile.XamlGivenPath, classlessXamlFile.TargetFolder, classlessXamlFile.FileNameNoExtension, classlessXamlFile.XamlOutputFilename))
				{
					return false;
				}
			}
			foreach (XamlClassCodeInfo value in _classCodeInfos.Values)
			{
				if (!GeneratePageOutputFiles(value))
				{
					return false;
				}
			}
			if (EnableTypeInfoReflection && !IsPass1)
			{
				_typeInfoCollector.AddAllConstructibleTypesFromLocalAssembly(_localAssembly);
			}
			GenerateBindingInfo();
			if (!flag3)
			{
				UpdateGeneratedFilesLists();
				flag3 = true;
			}
			if (!IsPass1 && !string.IsNullOrWhiteSpace(RootsLog) && !WriteRootsFile(_typeInfoCollector.RootLog, RootsLog))
			{
				return false;
			}
			if (!GenerateTypeInfo())
			{
				return false;
			}
			try
			{
				if (!IsPass1 && !DisableXbfGeneration)
				{
					DetermineGenXbfPath(_projectInfo);
					if (!GenerateXbfFiles(_newlyGeneratedXamlFiles))
					{
						return false;
					}
					if (!GenerateSdkXbfFiles())
					{
						return false;
					}
				}
			}
			catch (TypeLoadException)
			{
				LogError("CompileXaml", ErrorCode.WMC9998, null, GeneratedXamlFiles[0], 0, 0, 0, 0, XamlCompilerResources.XbfGeneration_MissingXbfApi);
				return false;
			}
			flag2 = true;
		}
		catch (UnresolvedAssemblyException ex2)
		{
			LogError_CannotResolveAssembly(ex2.Message);
		}
		catch (Exception e)
		{
			flag = false;
			LogError_XamlInternalError(e, null);
		}
		finally
		{
			if (!flag3)
			{
				UpdateGeneratedFilesLists();
			}
			RemoveLmrAssemblyResolveHandler();
			if (flag2)
			{
				bool flag11 = VerifyWorkDone();
			}
			InstanceCacheManager.ClearCache();
			if (IsPass1)
			{
				PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_EndPass1, ProjectName);
			}
			else
			{
				PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_EndPass2, ProjectName);
			}
			PerformanceUtility.Shutdown();
		}
		return flag;
	}

	private void DetermineGenXbfPath(XamlProjectInfo projectInfo)
	{
		if (!string.IsNullOrEmpty(GenXbfPath))
		{
			projectInfo.GenXbf32Path = Path.Combine(GenXbfPath, "x86\\genxbf.dll");
			projectInfo.GenXbf64Path = Path.Combine(GenXbfPath, "x64\\genxbf.dll");
			projectInfo.GenXbfArm64Path = Path.Combine(GenXbfPath, "arm64\\genxbf.dll");
		}
	}

	private bool CheckTaskArgumentsValid()
	{
		if (XamlApplications != null && XamlApplications.Any() && XamlApplications.Count() > 1)
		{
			LogError_MoreThanOneApplicationXaml();
			return false;
		}
		bool flag = false;
		for (int i = 0; i < SourceFileManager.ProjectXamlTaskItems.Count; i++)
		{
			TaskItemFilename taskItemFilename = SourceFileManager.ProjectXamlTaskItems[i];
			for (int j = i + 1; j < SourceFileManager.ProjectXamlTaskItems.Count; j++)
			{
				TaskItemFilename taskItemFilename2 = SourceFileManager.ProjectXamlTaskItems[j];
				if (KS.EqIgnoreCase(taskItemFilename.ApparentRelativePath, taskItemFilename2.ApparentRelativePath))
				{
					LogError_XamlFilesWithSameApparentPath(taskItemFilename.XamlGivenPath, taskItemFilename2.XamlGivenPath, taskItemFilename.ApparentRelativePath);
					flag = true;
				}
			}
		}
		return !flag;
	}

	private List<Assembly> LoadAssemblyItems(List<string> referenceAssemblies, bool isSystemAssembly)
	{
		List<Assembly> list = new List<Assembly>();
		foreach (string referenceAssembly in referenceAssemblies)
		{
			Assembly assembly = LoadAssemblyItem(referenceAssembly, isSystemAssembly);
			if (assembly != null)
			{
				list.Add(assembly);
			}
		}
		if (DoesMscorlibNeedToBeLoaded(out var mscorlib))
		{
			list.Add(mscorlib);
		}
		return list;
	}

	private Assembly LoadAssemblyItem(string item, bool isSystemAssembly)
	{
		if (isSystemAssembly)
		{
			return LoadAssembly(item);
		}
		try
		{
			return LoadAssembly(item);
		}
		catch (FileNotFoundException)
		{
			return null;
		}
	}

	public Assembly LoadAssembly(string reference)
	{
		Assembly assembly = TryLoadAssembly(reference);
		if (assembly == null)
		{
			LogError_CannotResolveAssembly(reference);
		}
		return assembly;
	}

	public static Assembly TryLoadAssembly(string reference)
	{
		string fullPath = Path.GetFullPath(reference);
		if (!File.Exists(fullPath))
		{
			return null;
		}
		return s_typeUniverse.LoadAssemblyFromFile(fullPath);
	}

	internal Assembly LoadAssemblyFromReferencePath(string fileName)
	{
		string[] referenceAssemblyPaths = ReferenceAssemblyPaths;
		foreach (string path in referenceAssemblyPaths)
		{
			string text = Path.Combine(path, fileName);
			if (File.Exists(text))
			{
				return LoadAssembly(text);
			}
		}
		return null;
	}

	internal Assembly LoadAssemblyFromSystemExtraReferences(string fileName)
	{
		foreach (string systemExtraReferenceItem in _systemExtraReferenceItems)
		{
			if (Path.GetFileName(systemExtraReferenceItem) == fileName)
			{
				return LoadAssembly(systemExtraReferenceItem);
			}
		}
		return null;
	}

	internal bool DoesMscorlibNeedToBeLoaded(out Assembly mscorlib)
	{
		mscorlib = null;
		if (!s_typeUniverse.IsSystemAssemblyLoaded)
		{
			mscorlib = s_typeUniverse.GetSystemAssembly();
		}
		return mscorlib != null;
	}

	protected void SetupLmrAssemblyResolveHandler()
	{
		s_typeUniverse.OnResolveEvent += universe_OnResolveEvent;
	}

	protected void RemoveLmrAssemblyResolveHandler()
	{
		s_typeUniverse.OnResolveEvent -= universe_OnResolveEvent;
	}

	private void universe_OnResolveEvent(object sender, ResolveAssemblyNameEventArgs e)
	{
		string fileName = ((e.Name.ContentType != AssemblyContentType.WindowsRuntime) ? (e.Name.Name + ".dll") : (e.Name.Name + ".winmd"));
		e.Target = LoadAssemblyFromReferencePath(fileName);
		if (e.Target == null)
		{
			e.Target = LoadAssemblyFromSystemExtraReferences(fileName);
		}
	}

	public void UnloadReferences()
	{
		if (s_typeUniverse != null)
		{
			s_typeResolver = null;
			s_typeUniverse.Dispose();
			s_typeUniverse = null;
			Microsoft.UI.Xaml.Markup.Compiler.DirectUI.ReflectionHelper.Release();
		}
	}

	private DirectUISchemaContext LoadSchemaContext()
	{
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_CreatingSchemaContext);
		SortReferenceAssemblies(out var systemItems, out var nonSystemItems, out var systemExtraItems);
		_loadedSystemAssemblies = LoadAssemblyItems(systemItems, isSystemAssembly: true);
		_loadedNonSystemAssemblies = LoadAssemblyItems(nonSystemItems, isSystemAssembly: false);
		_systemExtraReferenceItems = systemExtraItems;
		if (LocalAssembly != null)
		{
			_localAssemblyName = LocalAssembly.ItemSpec;
			_localAssembly = LoadAssembly(_localAssemblyName);
			_loadedNonSystemAssemblies.Add(_localAssembly);
		}
		_loadedAssemblies = new List<Assembly>(_loadedSystemAssemblies);
		_loadedAssemblies.AddRange(_loadedNonSystemAssemblies);
		ISet<string> set = new HashSet<string>();
		foreach (IAssemblyItem item in ReferenceAssemblies.Where((IAssemblyItem a) => a.IsStaticLibraryReference))
		{
			set.Add(item.ItemSpec);
		}
		if (s_typeResolver == null)
		{
			s_typeResolver = new TypeResolver(s_typeUniverse);
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_InitializeTypeNameMapStart);
			s_typeResolver.InitializeTypeNameMap();
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_InitializeTypeNameMapEnd);
		}
		if (_localAssembly != null)
		{
			s_typeResolver.AddLocalAssemblyToTypeNameMap(_localAssembly);
		}
		else if (!IsPass1)
		{
			LogWarning(new XamlLocalAssemblyNotFound());
		}
		DirectUISchemaContext directUISchemaContext = new DirectUISchemaContext(_loadedAssemblies, IsPass1 ? null : _systemExtraReferenceItems, _localAssembly, set, WindowsSdkPath, Language.IsStringNullable);
		directUISchemaContext.TypeResolver = s_typeResolver;
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_CreatedSchemaContext);
		return directUISchemaContext;
	}

	protected void WriteOutputFilesToDisk(List<FileNameAndContentPair> generatedFiles, string targetFolder, bool updateOnlyIfContentNew)
	{
		if (generatedFiles == null || generatedFiles.Count == 0)
		{
			return;
		}
		Directory.CreateDirectory(targetFolder);
		foreach (FileNameAndContentPair generatedFile in generatedFiles)
		{
			string text = Path.Combine(targetFolder, generatedFile.FileName);
			if (updateOnlyIfContentNew)
			{
				WriteOnlyIfContentsAreNew(text, generatedFile.Contents);
			}
			else
			{
				TaskFileService.WriteFile(generatedFile.Contents, text);
			}
		}
	}

	private bool WriteOnlyIfContentsAreNew(string outputFilename, string fileContents)
	{
		bool flag = false;
		if (File.Exists(outputFilename))
		{
			string oldString = File.ReadAllText(outputFilename);
			flag = AreGeneratedCodeStringsTheSame(oldString, fileContents);
		}
		if (!flag)
		{
			TaskFileService.WriteFile(fileContents, outputFilename);
			return true;
		}
		return false;
	}

	protected void WriteXamlTypeInfoFilesToDisk(List<FileNameAndContentPair> codeFiles)
	{
		if (codeFiles == null)
		{
			return;
		}
		if (codeFiles.Count != 1 || Language.IsNative)
		{
			WriteOutputFilesToDisk(codeFiles, OutputFolderFullpath, updateOnlyIfContentNew: true);
			return;
		}
		FileNameAndContentPair fileNameAndContentPair = codeFiles[0];
		bool flag = false;
		string text = Path.Combine(OutputFolderFullpath, fileNameAndContentPair.FileName);
		string path = text + ".backup";
		if (File.Exists(path))
		{
			string oldString = File.ReadAllText(path);
			flag = AreGeneratedCodeStringsTheSame(oldString, fileNameAndContentPair.Contents);
		}
		if (flag)
		{
			FileHelpers.RestoreBackupFile(text);
		}
		else
		{
			TaskFileService.WriteFile(fileNameAndContentPair.Contents, text);
		}
	}

	private bool WriteRootsFile(Roots roots, string filename)
	{
		XamlObjectReaderSettings xamlObjectReaderSettings = new XamlObjectReaderSettings();
		xamlObjectReaderSettings.LocalAssembly = Assembly.GetExecutingAssembly();
		XamlObjectReader xamlObjectReader = new XamlObjectReader(roots, xamlObjectReaderSettings);
		MemoryStream memoryStream = new MemoryStream();
		XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
		xmlWriterSettings.Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
		xmlWriterSettings.ConformanceLevel = ConformanceLevel.Document;
		xmlWriterSettings.Indent = true;
		XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
		XamlXmlWriter xamlWriter = new XamlXmlWriter(xmlWriter, xamlObjectReader.SchemaContext);
		XamlServices.Transform(xamlObjectReader, xamlWriter);
		xmlWriter.Flush();
		xmlWriter.Close();
		string text = Encoding.UTF8.GetString(memoryStream.ToArray());
		string oldValue = "xmlns=\"clr-namespace:Microsoft.UI.Xaml.Markup.Compiler.RootLog;assembly=Microsoft.UI.Xaml.Markup.Compiler\"";
		string fileContents = text.Replace(oldValue, string.Empty);
		string outputFilename = Path.Combine(OutputFolderFullpath, filename);
		WriteOnlyIfContentsAreNew(outputFilename, fileContents);
		return true;
	}

	private bool AreGeneratedCodeStringsTheSame(string oldString, string newString)
	{
		string text = oldString;
		string text2 = newString;
		if (oldString.Length != newString.Length)
		{
			if (oldString.Length < newString.Length)
			{
				text = newString;
				text2 = oldString;
			}
			string value = text.Substring(text2.Length);
			text = text.Substring(0, text2.Length);
			if (!string.IsNullOrWhiteSpace(value))
			{
				return false;
			}
		}
		return text.Equals(text2, StringComparison.Ordinal);
	}

	private bool GenerateTypeInfo()
	{
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_GenerateTypeInfoStart);
		bool flag = true;
		List<FileNameAndContentPair> list;
		if (ShouldSuppressTypeInfoCodeGen())
		{
			list = null;
			flag = false;
		}
		else
		{
			list = _codeGenerator.GenerateTypeInfo(_typeInfoCollector.AppXamlInfo);
		}
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_GenerateTypeInfoEnd);
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_WriteTypeinfoFilesToDiskStart);
		WriteXamlTypeInfoFilesToDisk(list);
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_WriteTypeinfoFilesToDiskEnd);
		if (flag)
		{
			if (Language.IsNative)
			{
				if (list != null && list.Count > 0)
				{
					foreach (FileNameAndContentPair item in list)
					{
						string text = Path.Combine(OutputFolderFullpath, item.FileName);
						if (text.EndsWith(LanguageSourceExtension))
						{
							_generatedCodeFiles.Add(text);
						}
					}
				}
			}
			else if (IsPass1)
			{
				string text2 = Path.Combine(OutputFolderFullpath, "XamlTypeInfo" + Language.Pass2Extension);
				if (!IsDesignTimeBuild || !File.Exists(text2))
				{
					FileHelpers.BackupIfExistsAndTruncateToNull(text2);
				}
				_generatedCodeFiles.Add(text2);
			}
		}
		return true;
	}

	private void GenerateBindingInfo()
	{
		if (!Language.IsNative)
		{
			return;
		}
		Dictionary<string, XamlType> dictionary = new Dictionary<string, XamlType>();
		Dictionary<string, XamlType> dictionary2 = new Dictionary<string, XamlType>();
		Dictionary<string, XamlMember> dictionary3 = new Dictionary<string, XamlMember>();
		bool flag = false;
		SaveState.ProcessBindingInfo();
		foreach (SaveStatePerXamlFile value in SaveState.XamlPerFileInfo.Values)
		{
			if (!IsPass1)
			{
				foreach (SaveStateXamlType value2 in value.BindingObservableVectorTypes.Values)
				{
					if (!dictionary.ContainsKey(value2.FullName))
					{
						Type typeByFullName = _schemaContext.TypeResolver.GetTypeByFullName(value2.FullName);
						if (typeByFullName == null)
						{
							LogError(new XamlSchemaError_UnknownTypeError(value2.FullName));
							continue;
						}
						XamlType xamlType = _schemaContext.GetXamlType(typeByFullName);
						dictionary.Add(value2.FullName, xamlType);
					}
				}
				foreach (SaveStateXamlType value3 in value.BindingObservableMapTypes.Values)
				{
					if (!dictionary2.ContainsKey(value3.FullName))
					{
						Type typeByFullName2 = _schemaContext.TypeResolver.GetTypeByFullName(value3.FullName);
						if (typeByFullName2 == null)
						{
							LogError(new XamlSchemaError_UnknownTypeError(value3.FullName));
							continue;
						}
						XamlType xamlType2 = _schemaContext.GetXamlType(typeByFullName2);
						dictionary2.Add(value3.FullName, xamlType2);
					}
				}
				foreach (SaveStateXamlMember value4 in value.BindingSetters.Values)
				{
					if (dictionary3.ContainsKey(value4.ToString()))
					{
						continue;
					}
					Type typeByFullName3 = _schemaContext.TypeResolver.GetTypeByFullName(value4.DeclaringTypeFullName);
					if (typeByFullName3 == null)
					{
						LogError(new XamlSchemaError_UnknownTypeError(value4.DeclaringTypeFullName));
						continue;
					}
					XamlType xamlType3 = _schemaContext.GetXamlType(typeByFullName3);
					XamlMember xamlMember = xamlType3.GetMember(value4.Name);
					if (xamlMember == null)
					{
						xamlMember = xamlType3.GetAttachableMember(value4.Name);
					}
					dictionary3.Add(value4.ToString(), xamlMember);
				}
			}
			flag |= value.HasBoundEventAssignments;
		}
		List<FileNameAndContentPair> generatedFiles = _codeGenerator.GenerateBindingInfo(dictionary, dictionary2, dictionary3, flag);
		WriteOutputFilesToDisk(generatedFiles, OutputFolderFullpath, updateOnlyIfContentNew: true);
	}

	private Type GetIXamlType(List<Assembly> loadedAssemblies)
	{
		return GetType("Microsoft.UI.Xaml.Markup.IXamlType", loadedAssemblies);
	}

	private Type GetType(string typeName, List<Assembly> loadedAssemblies)
	{
		foreach (Assembly loadedAssembly in loadedAssemblies)
		{
			Type type = loadedAssembly.GetType(typeName);
			if (type != null)
			{
				return type;
			}
		}
		return null;
	}

	private bool TypeExists(string typeName, List<Assembly> loadedAssemblies)
	{
		return GetType(typeName, loadedAssemblies) != null;
	}

	private bool ReportSchemaErrors(string xamlFile)
	{
		if (_schemaContext.SchemaErrors.Count > 0)
		{
			foreach (XamlCompileError schemaError in _schemaContext.SchemaErrors)
			{
				LogError("XamlCompiler", schemaError.Code, null, xamlFile, schemaError.LineNumber, schemaError.LineOffset, 0, 0, schemaError.Message);
			}
		}
		if (_schemaContext.SchemaWarnings.Count > 0)
		{
			foreach (XamlCompileWarning schemaWarning in _schemaContext.SchemaWarnings)
			{
				LogWarning(schemaWarning, xamlFile);
			}
		}
		_schemaContext.SchemaWarnings.Clear();
		bool result = _schemaContext.SchemaErrors.Count == 0;
		_schemaContext.SchemaErrors.Clear();
		return result;
	}

	private bool ReportXbfErrors(XbfGenerator xbfGenerator)
	{
		if (xbfGenerator.XbfErrors.Count > 0)
		{
			foreach (XamlCompileError xbfError in xbfGenerator.XbfErrors)
			{
				LogError("XamlCompiler", xbfError.Code, null, xbfError.FileName, xbfError.LineNumber, xbfError.LineOffset, 0, 0, xbfError.Message);
			}
		}
		if (xbfGenerator.XbfWarnings.Count > 0)
		{
			foreach (XamlCompileWarning xbfWarning in xbfGenerator.XbfWarnings)
			{
				LogWarning(xbfWarning);
			}
		}
		xbfGenerator.XbfWarnings.Clear();
		bool result = xbfGenerator.XbfErrors.Count == 0;
		xbfGenerator.XbfErrors.Clear();
		return result;
	}

	private XamlProjectInfo GetProjectInfo()
	{
		XamlProjectInfo xamlProjectInfo = new XamlProjectInfo();
		xamlProjectInfo.CodeGenFlags = CodeGenerationControlFlags;
		xamlProjectInfo.ProjectName = FileHelpers.GetSafeName(ProjectName) ?? string.Empty;
		xamlProjectInfo.RootNamespace = RootNamespace;
		xamlProjectInfo.IsLibrary = IsOutputTypeLibrary || IsOutputTypeWinMd;
		xamlProjectInfo.IsCLSCompliant = _localAssembly != null && _localAssembly.IsClsCompliant();
		xamlProjectInfo.ShouldGenerateDisableXBind = EnableXBindDiagnostics;
		xamlProjectInfo.EnableTypeInfoReflection = EnableTypeInfoReflection;
		xamlProjectInfo.EnableDefaultValidationContextGeneration = EnableDefaultValidationContextGeneration;
		xamlProjectInfo.ClassToHeaderFileMap = GetClassToHeaderFileMap();
		xamlProjectInfo.AdditionalXamlTypeInfoIncludes = GetAdditionalXamlTypeInfoIncludes();
		if (IgnoreSpecifiedTargetPlatformMinVersion)
		{
			xamlProjectInfo.TargetPlatformMinVersion = ReleaseDefinition.MaxSupportedVersion;
		}
		else if (!string.IsNullOrEmpty(TargetPlatformMinVersion))
		{
			xamlProjectInfo.TargetPlatformMinVersion = new Version(TargetPlatformMinVersion);
		}
		xamlProjectInfo.IsInputValidationEnabled = TypeExists("Microsoft.UI.Xaml.Controls.IInputValidationControl", _loadedAssemblies);
		xamlProjectInfo.IsWin32App = EnableWin32Codegen;
		xamlProjectInfo.UsingCSWinRT = UsingCSWinRT;
		xamlProjectInfo.PrecompiledHeaderFile = PrecompiledHeaderFile;
		return xamlProjectInfo;
	}

	private bool ProcessXamlFile_XamlTypeInfo(TaskItemFilename tif, ref XamlDomObject xamlDomRoot)
	{
		if (!LoadAndValidateXamlDom(tif.SourceXamlFullPath, tif.ApparentRelativePath, out xamlDomRoot))
		{
			return false;
		}
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageTypeCollectStart);
		_typeInfoCollector.Collect(xamlDomRoot);
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageTypeCollectEnd);
		if (!ReportSchemaErrors(tif.XamlGivenPath))
		{
			return false;
		}
		string stringValueOfProperty = DomHelper.GetStringValueOfProperty(xamlDomRoot, XamlLanguage.Class);
		if (!string.IsNullOrWhiteSpace(stringValueOfProperty))
		{
			if (tif.IsApplication)
			{
				_typeInfoCollector.AppXamlInfo = new ClassName(stringValueOfProperty);
			}
			else
			{
				XamlTypeName xamlTypeNameFromFullName = XamlSchemaCodeInfo.GetXamlTypeNameFromFullName(stringValueOfProperty);
				XamlType xamlType = _schemaContext.GetXamlType(xamlTypeNameFromFullName);
				if (!(xamlType != null))
				{
					AssemblyName name = _localAssembly.GetName();
					bool flag = name.ContentType == AssemblyContentType.WindowsRuntime;
					string name2 = name.Name;
					if (flag && !stringValueOfProperty.StartsWith(name2))
					{
						LogError_ClassDoesntMatchWinmdName(stringValueOfProperty, name2 + ".winmd", tif.XamlGivenPath);
						return false;
					}
					LogError_ClassIsNotFoundInAssembly(stringValueOfProperty, name2, tif.XamlGivenPath);
					return false;
				}
				DirectUIXamlType directUIXamlType = (DirectUIXamlType)xamlType;
				if (!directUIXamlType.IsHardDeprecated)
				{
					_typeInfoCollector.SchemaInfo.AddTypeAndProperties(xamlType);
				}
				_typeInfoCollector.AddTypeToRootLog((DirectUIXamlType)xamlType);
			}
		}
		return true;
	}

	private bool ProcessXamlFile_PerPageInfo(TaskItemFilename tif, ref XamlDomObject xamlDomRoot)
	{
		string directoryName = Path.GetDirectoryName(ProjectPath);
		XamlHarvester xamlHarvester = new XamlHarvester(directoryName, IsPass1, XamlPlatform);
		if (xamlDomRoot == null && !LoadAndValidateXamlDom(tif.SourceXamlFullPath, tif.ApparentRelativePath, out xamlDomRoot))
		{
			return false;
		}
		if (Language.IsNative)
		{
			xamlHarvester.SkipNameFieldsForRootElements = true;
		}
		string classFullName = XamlHarvester.GetClassFullName(xamlDomRoot);
		if (!string.IsNullOrWhiteSpace(classFullName))
		{
			XamlClassCodeInfo value = null;
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageHarvestStart, tif.XamlGivenPath);
			if (!_classCodeInfos.TryGetValue(classFullName, out value))
			{
				value = xamlHarvester.HarvestClassInfo(classFullName, xamlDomRoot, tif.IsApplication);
				value.RootNamespace = RootNamespace;
				value.TargetFolder = tif.TargetFolder;
				value.PriIndexName = PriIndexName;
				if (!string.IsNullOrWhiteSpace(XamlResourceMapName) || !string.IsNullOrWhiteSpace(tif.XamlResourceMapName))
				{
					value.XamlResourceMapName = (string.IsNullOrEmpty(tif.XamlResourceMapName) ? XamlResourceMapName : tif.XamlResourceMapName);
				}
				if (!string.IsNullOrWhiteSpace(XamlComponentResourceLocation) || !string.IsNullOrWhiteSpace(tif.XamlComponentResourceLocation))
				{
					value.XamlComponentResourceLocation = GetComponentResourceLocation(tif.XamlComponentResourceLocation);
				}
				_classCodeInfos.Add(classFullName, value);
			}
			else
			{
				value.TargetFolder = FileHelpers.ComputeBaseFolder(value.TargetFolder, tif.TargetFolder);
			}
			XamlFileCodeInfo xamlFileCodeInfo = xamlHarvester.HarvestXamlFileInfo(value, xamlDomRoot);
			if (xamlFileCodeInfo != null)
			{
				xamlFileCodeInfo.ApparentRelativePath = tif.ApparentRelativePath;
				xamlFileCodeInfo.FullPathToXamlFile = tif.SourceXamlFullPath;
				xamlFileCodeInfo.SourceXamlGivenPath = tif.XamlGivenPath;
				xamlFileCodeInfo.RelativePathFromGeneratedCodeToXamlFile = tif.RelativePathFromGeneratedCodeToXamlFile;
				xamlFileCodeInfo.XamlOutputFilename = tif.XamlOutputFilename;
				value.AddXamlFileInfo(xamlFileCodeInfo);
				if (Language.IsNative)
				{
					SaveState.AddBindingInfo(xamlFileCodeInfo);
				}
				if (xamlFileCodeInfo.XPropertyInfo?.xProperties?.FirstOrDefault() != null)
				{
					LogWarning(new XamlValidationWarningExperimental(ErrorCode.WMC1503, "x:Property"));
				}
			}
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageHarvestEnd, tif.XamlGivenPath);
			if (!ReportSchemaErrors(tif.XamlGivenPath))
			{
				return false;
			}
			if (tif.IsApplication && value.HasEventAssignments)
			{
				LogError_EventsInAppXaml(tif.XamlGivenPath);
				return false;
			}
		}
		return true;
	}

	private bool GenerateClasslessXamlOutputFile(string sourceXamlFullPath, string givenXamlPath, string targetFolder, string codeFileName, string xamlOutputFileName)
	{
		List<FileNameAndContentPair> list = null;
		List<FileNameAndContentPair> list2 = null;
		string path = Path.Combine(targetFolder, codeFileName + GeneratedExtension);
		string outputXbf = Path.Combine(targetFolder, codeFileName + ".xbf");
		list = new List<FileNameAndContentPair>();
		list.Add(new FileNameAndContentPair(Path.GetFileName(path), GeneratedExtension.EndsWith("cpp") ? " #include \"pch.h\"" : " "));
		if (!IsPass1)
		{
			list2 = new List<FileNameAndContentPair>();
			list2.Add(new FileNameAndContentPair(Path.GetFileName(givenXamlPath), File.ReadAllText(givenXamlPath)));
			_newlyGeneratedXamlFiles.Add(new XbfFileNameInfo(sourceXamlFullPath, givenXamlPath, xamlOutputFileName, outputXbf));
		}
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_WriteFilesToDiskStart);
		WriteOutputFilesToDisk(list, targetFolder, updateOnlyIfContentNew: true);
		WriteOutputFilesToDisk(list2, targetFolder, updateOnlyIfContentNew: true);
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_WriteFilesToDiskEnd);
		return true;
	}

	private void PruneBindUniverses(XamlClassCodeInfo classCodeInfo)
	{
		List<BindUniverse> list = new List<BindUniverse>();
		foreach (BindUniverse bindUniverse in classCodeInfo.BindUniverses)
		{
			bool flag = true;
			if (bindUniverse.HasBindAssignments || bindUniverse.HasBoundEventAssignments || bindUniverse.NeededForOuterScopeElement)
			{
				flag = false;
			}
			if (flag)
			{
				list.Add(bindUniverse);
			}
		}
		foreach (BindUniverse item in list)
		{
			classCodeInfo.BindUniverses.Remove(item);
			foreach (BindUniverse bindUniverse2 in classCodeInfo.BindUniverses)
			{
				if (bindUniverse2.Children.Contains(item))
				{
					bindUniverse2.Children.Remove(item);
				}
				if (bindUniverse2.Parent == item)
				{
					bindUniverse2.Parent = null;
				}
			}
		}
	}

	private bool GeneratePageOutputFiles(XamlClassCodeInfo classCodeInfo)
	{
		List<FileNameAndContentPair> list = null;
		List<FileNameAndContentPair> generatedSources = null;
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageCodeGenStart, classCodeInfo.BaseFileName);
		if (!IsPass1 && (classCodeInfo.HasBindAssignments || classCodeInfo.HasBoundEventAssignments))
		{
			List<XamlCompileErrorBase> list2 = new List<XamlCompileErrorBase>();
			foreach (BindUniverse bindUniverse in classCodeInfo.BindUniverses)
			{
				IEnumerable<XamlCompileErrorBase> collection = bindUniverse.Parse(classCodeInfo, _projectInfo.TargetPlatformMinVersion);
				list2.AddRange(collection);
			}
			PruneBindUniverses(classCodeInfo);
			foreach (XamlCompileError item in list2.OfType<XamlCompileError>())
			{
				XamlCompileError xamlCompileError = item;
				LogError("XamlCompiler", xamlCompileError.Code, null, xamlCompileError.FileName, xamlCompileError.LineNumber, xamlCompileError.LineOffset, 0, 0, xamlCompileError.Message);
			}
			foreach (XamlCompileWarning item2 in list2.OfType<XamlCompileWarning>())
			{
				XamlCompileWarning warning = item2;
				LogWarning(warning);
			}
			if (list2.OfType<XamlCompileError>().Any())
			{
				return false;
			}
		}
		list = _codeGenerator.GenerateCodeBehind(classCodeInfo, out var xamlFilesChecksumPairs);
		if (!classCodeInfo.IsApplication && ShouldSuppressPageCodeGen())
		{
			list = null;
		}
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageCodeGenEnd, classCodeInfo.BaseFileName);
		if (IsPass1 && !IsDesignTimeBuild)
		{
			string text = Path.Combine(classCodeInfo.TargetFolder, classCodeInfo.BaseFileName + Language.Pass2Extension);
			if (File.Exists(text))
			{
				FileHelpers.BackupFile(text);
				File.Delete(text);
			}
		}
		if (!IsPass1)
		{
			foreach (XamlFileCodeInfo fileCodeInfo in classCodeInfo.PerXamlFileInfo)
			{
				if (!GenerateEditedXamlFile(ref generatedSources, classCodeInfo, fileCodeInfo))
				{
					return false;
				}
				TaskItemFilename taskItemFilename = SourceFileManager.FindTaskItemByFullPath(fileCodeInfo.FullPathToXamlFile);
				string checksum = xamlFilesChecksumPairs.Where((FileNameAndChecksumPair x) => x.FileName == fileCodeInfo.FullPathToXamlFile).FirstOrDefault().Checksum;
				_newlyGeneratedXamlFiles.Add(new XbfFileNameInfo(taskItemFilename.SourceXamlFullPath, taskItemFilename.XamlGivenPath, taskItemFilename.XamlOutputFilename, taskItemFilename.XbfOutputFilename, checksum));
			}
		}
		if (!IsPass1)
		{
			string fullPathFileName = Path.Combine(classCodeInfo.TargetFolder, classCodeInfo.BaseFileName + Language.Pass2Extension);
			FileHelpers.RestoreBackupFile(fullPathFileName);
		}
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_WriteFilesToDiskStart);
		WriteOutputFilesToDisk(list, classCodeInfo.TargetFolder, updateOnlyIfContentNew: true);
		WriteOutputFilesToDisk(generatedSources, classCodeInfo.TargetFolder, updateOnlyIfContentNew: true);
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_WriteFilesToDiskEnd);
		return true;
	}

	private void CreateDirectoryIfNecessary(DirectoryInfo directoryInfo)
	{
		if (!directoryInfo.Exists)
		{
			CreateDirectoryIfNecessary(directoryInfo.Parent);
			directoryInfo.Create();
		}
	}

	private void CreateFileIfNecessary(string filename)
	{
		FileInfo fileInfo = new FileInfo(filename);
		CreateDirectoryIfNecessary(fileInfo.Directory);
		if (!fileInfo.Exists)
		{
			using (fileInfo.Create())
			{
			}
		}
	}

	private bool GenerateEditedXamlFile(ref List<FileNameAndContentPair> generatedSources, XamlClassCodeInfo classCodeInfo, XamlFileCodeInfo fileCodeInfo)
	{
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageEditStart, fileCodeInfo.SourceXamlGivenPath);
		XamlConnectionIdRewriter xamlConnectionIdRewriter = new XamlConnectionIdRewriter();
		string contents = xamlConnectionIdRewriter.Edit(fileCodeInfo.FullPathToXamlFile, classCodeInfo, fileCodeInfo);
		if (xamlConnectionIdRewriter.Errors.Count > 0)
		{
			foreach (XamlCompileError error in xamlConnectionIdRewriter.Errors)
			{
				LogError("XamlCompiler", error.Code, null, fileCodeInfo.SourceXamlGivenPath, error.LineNumber, error.LineOffset, 0, 0, error.Message);
			}
			return false;
		}
		string relativePath = FileHelpers.GetRelativePath(classCodeInfo.TargetFolder, fileCodeInfo.XamlOutputFilename);
		if (generatedSources == null)
		{
			generatedSources = new List<FileNameAndContentPair>();
		}
		generatedSources.Add(new FileNameAndContentPair(relativePath, contents));
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageEditEnd, fileCodeInfo.SourceXamlGivenPath);
		return true;
	}

	private bool GenerateXbfFiles(IEnumerable<IXbfFileNameInfo> xamlList)
	{
		if (xamlList.Any())
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_GenerateXBFStart, xamlList.Count().ToString());
			XbfGenerator xbfGenerator = new XbfGenerator(_projectInfo, _xamlMetadataProvider);
			xbfGenerator.SetXamlInputFiles(xamlList);
			bool flag = xbfGenerator.GenerateXbfFiles(XbfGenerationFlags);
			if (flag)
			{
				AddToGeneratedXbfFiles(xamlList);
			}
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_GenerateXBFEnd);
			bool flag2 = ReportXbfErrors(xbfGenerator);
			return flag;
		}
		return true;
	}

	private bool GenerateSdkXbfFiles()
	{
		bool flag = true;
		if (SourceFileManager.SdkXamlTaskItems != null && SourceFileManager.SdkXamlTaskItems.Count > 0)
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_GenerateSdkXBFStart, SourceFileManager.SdkXamlTaskItems.Count.ToString());
			XbfGenerator xbfGenerator = new XbfGenerator(_projectInfo, _xamlMetadataProvider);
			if (SourceFileManager.SdkNon80XamlTaskItems != null && SourceFileManager.SdkNon80XamlTaskItems.Count > 0)
			{
				xbfGenerator.SetXamlInputFilesFromTaskItems(SourceFileManager.SdkNon80XamlTaskItems, isSdk: true);
				flag = xbfGenerator.GenerateXbfFiles(XbfGenerationFlags);
			}
			if (flag && SourceFileManager.Sdk80XamlTaskItems != null && SourceFileManager.Sdk80XamlTaskItems.Count > 0)
			{
				xbfGenerator.SetXamlInputFilesFromTaskItems(SourceFileManager.Sdk80XamlTaskItems, isSdk: true);
				flag = xbfGenerator.GenerateXbfFiles(XbfGenerationFlags, v80Compat: true);
			}
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_GenerateSdkXBFEnd);
			bool flag2 = ReportXbfErrors(xbfGenerator);
		}
		return flag;
	}

	private void AddGeneratedXbfFile(string filename)
	{
		if (_generatedXbfFiles.Where((string x) => x == filename).FirstOrDefault() == null)
		{
			_generatedXbfFiles.Add(filename);
		}
	}

	private void AddToGeneratedXbfFiles(IEnumerable<IXbfFileNameInfo> xamlXbfInfoList)
	{
		if (xamlXbfInfoList == null)
		{
			return;
		}
		foreach (IXbfFileNameInfo xamlXbfInfo in xamlXbfInfoList)
		{
			AddGeneratedXbfFile(xamlXbfInfo.OutputXbfName);
		}
	}

	private bool ProcessXProperty(XamlDomObject domRoot, XamlDomItem domItem, string xamlApparentRelativeName, string[] xamlLines)
	{
		XamlDomObject xamlDomObject = domItem as XamlDomObject;
		xProperty xProperty2 = new xProperty();
		xProperty2.OriginalXProperty = xamlDomObject;
		xProperty2.CodegenComment = $"{xamlApparentRelativeName}, line {xamlDomObject.StartLineNumber}";
		foreach (XamlDomMember memberNode in xamlDomObject.MemberNodes)
		{
			switch (memberNode.Member.Name)
			{
			case "Name":
				xProperty2.Name = (memberNode.Item as XamlDomValue).Value as string;
				break;
			case "ChangedHandler":
				xProperty2.ChangedHandler = (memberNode.Item as XamlDomValue).Value as string;
				break;
			case "IsReadOnly":
				xProperty2.IsReadOnly = bool.Parse((memberNode.Item as XamlDomValue).Value as string);
				break;
			case "Type":
			{
				string xName = (memberNode.Item as XamlDomValue).Value as string;
				string text7 = null;
				XamlType xamlType = domRoot.ResolveXmlName(xName);
				if (xamlType != null)
				{
					string fullGenericNestedName = XamlSchemaCodeInfo.GetFullGenericNestedName(xamlType.UnderlyingType, "WinRT", globalized: false);
					text7 = xamlType.UnderlyingType.Namespace + "." + xamlType.Name;
					xProperty2.PropertyType = xamlType;
				}
				else
				{
					XamlTypeName xamlTypeName = domRoot.ResolveXmlNameToTypeName(xName);
					text7 = xamlTypeName.Namespace.StripUsingPrefix() + "." + xamlTypeName.Name;
				}
				xProperty2.FullTypeName = text7;
				break;
			}
			case "DefaultValue":
			case "_UnknownContent":
				if (memberNode.Item is XamlDomObject xamlDomObject2)
				{
					if (!IsPass1)
					{
						_typeInfoCollector.Collect(xamlDomObject2);
					}
					FixedSourceInfo fixedSourceInfo = XamlSourceInfoFixer.GetFixedSourceInfo(new StrippableObject(xamlDomObject2), xamlLines);
					string text = XamlSourceInfoFixer.ReadMarkup(fixedSourceInfo.StartOpeningTag, fixedSourceInfo.StartClosingTag, xamlLines);
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append(" ");
					foreach (NamespaceDeclaration namespacePrefix in domRoot.GetNamespacePrefixes())
					{
						stringBuilder.Append("xmlns");
						if (!string.IsNullOrEmpty(namespacePrefix.Prefix))
						{
							stringBuilder.Append(":");
							stringBuilder.Append(namespacePrefix.Prefix);
						}
						stringBuilder.Append("=\"");
						stringBuilder.Append(namespacePrefix.Namespace);
						stringBuilder.Append("\" ");
					}
					string text2 = stringBuilder.ToString();
					int num = text.LastIndexOf('>') - 1;
					string text3 = "";
					if (fixedSourceInfo.SelfClosing)
					{
						num--;
						text3 = "/";
					}
					string text4 = text.Substring(0, num + 1) + text2 + text3;
					string text5 = XamlSourceInfoFixer.ReadMarkup(fixedSourceInfo.StartClosingTag, fixedSourceInfo.EndClosingTag, xamlLines);
					string value = text4 + text5;
					using StringWriter stringWriter = new StringWriter();
					string text6 = null;
					string name = Language.Name;
					if (!(name == "C#"))
					{
						if (!(name == "VB"))
						{
							LogError(new ErrorXPropertyUsageNotSupported(xamlDomObject, Language));
							return false;
						}
						text6 = "vb";
					}
					else
					{
						text6 = "cs";
					}
					using CodeDomProvider codeDomProvider = CodeDomProvider.CreateProvider(text6);
					codeDomProvider.GenerateCodeFromExpression(new CodePrimitiveExpression(value), stringWriter, null);
					xProperty2.DefaultValueMarkup = stringWriter.ToString();
				}
				else
				{
					xProperty2.DefaultValueString = (memberNode.Item as XamlDomValue).Value as string;
				}
				break;
			}
		}
		domRoot.XPropertyInfo.xProperties.Add(xProperty2);
		return true;
	}

	private bool PreprocessxProperties(XamlDomObject domRoot, string xamlFileName, string xamlApparentRelativeName)
	{
		XamlMember contentProperty = domRoot.Type.ContentProperty;
		if (contentProperty == null)
		{
			return true;
		}
		XamlDomMember xamlDomMember = null;
		foreach (XamlDomMember memberNode in domRoot.MemberNodes)
		{
			if (!memberNode.Member.Equals(contentProperty) || !(memberNode?.Item is XamlDomObject xamlDomObject) || !xamlDomObject.Type.Equals(_schemaContext.DirectUIXamlLanguage.Properties))
			{
				continue;
			}
			xamlDomMember = memberNode;
			domRoot.XPropertyInfo = new xPropertyInfo();
			domRoot.XPropertyInfo.xProperties = new List<xProperty>();
			domRoot.XPropertyInfo.xPropertiesNode = xamlDomObject;
			domRoot.XPropertyInfo.xPropertiesRoot = domRoot;
			string[] array = null;
			try
			{
				array = File.ReadAllLines(xamlFileName);
			}
			catch (Exception ex)
			{
				XamlRewriterErrorFileOpenFailure xamlRewriterErrorFileOpenFailure = new XamlRewriterErrorFileOpenFailure(xamlFileName, ex.Message);
				LogError("XamlCompiler", xamlRewriterErrorFileOpenFailure.Code, null, xamlFileName, xamlRewriterErrorFileOpenFailure.LineNumber, xamlRewriterErrorFileOpenFailure.LineOffset, 0, 0, xamlRewriterErrorFileOpenFailure.Message);
				return false;
			}
			foreach (XamlDomMember memberNode2 in xamlDomObject.MemberNodes)
			{
				if (!memberNode2.Member.Name.Equals("_Items"))
				{
					continue;
				}
				foreach (XamlDomItem item in memberNode2.Items)
				{
					if (!ProcessXProperty(domRoot, item, xamlApparentRelativeName, array))
					{
						return false;
					}
				}
			}
			break;
		}
		if (xamlDomMember != null)
		{
			domRoot.MemberNodes.Remove(xamlDomMember);
		}
		return true;
	}

	private bool LoadAndValidateXamlDom(string xamlFileName, string xamlApparentRelativeName, out XamlDomObject xamlDomRoot)
	{
		xamlDomRoot = LoadXamlDom(xamlFileName);
		if (xamlDomRoot == null)
		{
			return false;
		}
		if (!PreprocessxProperties(xamlDomRoot, xamlFileName, xamlApparentRelativeName))
		{
			return false;
		}
		return ValidateXaml(xamlDomRoot, xamlFileName);
	}

	private XamlDomObject LoadXamlDom(string xamlPath)
	{
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageLoadStart, xamlPath);
		XamlDomObject result = null;
		try
		{
			using TextReader input = TaskFileService.GetFileContents(xamlPath);
			using XmlReader xmlReader = XmlReader.Create(input);
			XamlXmlReaderSettings xamlXmlReaderSettings = new XamlXmlReaderSettings();
			xamlXmlReaderSettings.LocalAssembly = _localAssembly;
			xamlXmlReaderSettings.AllowProtectedMembersOnRoot = true;
			xamlXmlReaderSettings.ProvideLineInfo = true;
			xamlXmlReaderSettings.IgnoreUidsOnPropertyElements = true;
			XamlXmlReader xamlReader = new XamlXmlReader(xmlReader, _schemaContext, xamlXmlReaderSettings);
			XamlDomWriter xamlDomWriter = new XamlDomWriter(_schemaContext, xamlPath);
			XamlServices.Transform(xamlReader, xamlDomWriter, closeWriter: true);
			result = xamlDomWriter.RootNode as XamlDomObject;
		}
		catch (XmlException e)
		{
			LogError_XamlXMLParsingError(e, xamlPath);
		}
		catch (XamlParseException e2)
		{
			LogError_XamlXMLParsingError(e2, xamlPath);
		}
		catch (Exception e3)
		{
			LogError_XamlInternalError(e3, xamlPath);
		}
		finally
		{
			PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageLoadEnd, xamlPath);
		}
		return result;
	}

	private bool ValidateXaml(XamlDomObject domRoot, string xamlRelativePath)
	{
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageValidateStart, xamlRelativePath);
		XamlDomValidator xamlDomValidator = new XamlDomValidator();
		xamlDomValidator.IsPass1 = IsPass1;
		xamlDomValidator.TargetPlatformMinVersion = _projectInfo.TargetPlatformMinVersion;
		xamlDomValidator.XamlPlatform = XamlPlatform;
		xamlDomValidator.Validate(domRoot);
		bool flag = !ReportSchemaErrors(xamlRelativePath);
		if (xamlDomValidator.HasErrors)
		{
			foreach (XamlCompileError error in xamlDomValidator.Errors)
			{
				LogError("XamlCompiler", error.Code, null, xamlRelativePath, error.LineNumber, error.LineOffset, 0, 0, error.Message);
			}
		}
		if (xamlDomValidator.HasWarnings)
		{
			foreach (XamlCompileWarning warning in xamlDomValidator.Warnings)
			{
				LogWarning(warning, xamlRelativePath);
			}
		}
		PerformanceUtility.FireCodeMarker(CodeMarkerEvent.perfXC_PageValidateEnd, xamlRelativePath);
		if (xamlDomValidator.HasErrors || flag)
		{
			return false;
		}
		return true;
	}

	private bool ValidateXamlTypeInfo()
	{
		XamlTypeInfoValidator xamlTypeInfoValidator = new XamlTypeInfoValidator(_schemaContext);
		xamlTypeInfoValidator.Validate(_typeInfoCollector.SchemaInfo.TypeTableFromAllAssemblies);
		foreach (XamlCompileError error in xamlTypeInfoValidator.Errors)
		{
			LogError(error);
		}
		foreach (XamlCompileWarning warning in xamlTypeInfoValidator.Warnings)
		{
			LogWarning(warning);
		}
		return !xamlTypeInfoValidator.Errors.Any();
	}

	private string GetComponentResourceLocation(string xamlComponentResourceLocation)
	{
		string text = (string.IsNullOrEmpty(xamlComponentResourceLocation) ? XamlComponentResourceLocation : xamlComponentResourceLocation);
		if (!string.IsNullOrEmpty(text))
		{
			if (text.ToLower(CultureInfo.InvariantCulture) == "application")
			{
				text = "Application";
			}
			else if (text.ToLower(CultureInfo.InvariantCulture) == "nested")
			{
				text = "Nested";
			}
			else
			{
				LogError_XamlInternalError(new Exception("ComponentResourceLocation '" + text + "' unrecognized."), null);
			}
		}
		return text;
	}

	private void SortReferenceAssemblies(out List<string> systemItems, out List<string> nonSystemItems, out List<string> systemExtraItems)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		foreach (IAssemblyItem referenceAssembly in ReferenceAssemblies)
		{
			string text = Path.GetFileName(referenceAssembly.ItemSpec).ToLower(CultureInfo.InvariantCulture);
			bool flag = false;
			switch (text)
			{
			case "microsoft.ui.text.winmd":
			case "microsoft.ui.xaml.winmd":
			case "microsoft.winui.dll":
			case "microsoft.ui.winmd":
			case "mscorlib.dll":
			case "windows.winmd":
			case "microsoft.csharp.metadata_dll":
			case "microsoft.visualbasic.metadata_dll":
			case "system.runtime.windowsruntime.ui.xaml.metadata_dll":
			case "system.runtime.windowsruntime.metadata_dll":
			case "microsoft.csharp.dll":
			case "microsoft.visualbasic.dll":
			case "system.runtime.windowsruntime.ui.xaml.dll":
			case "system.runtime.windowsruntime.dll":
				flag = true;
				break;
			}
			bool flag2 = text.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase);
			if (referenceAssembly.IsSystemReference || flag2 || (IsDesignTimeBuild && referenceAssembly.IsNuGetReference))
			{
				if (flag)
				{
					list.Add(referenceAssembly.ItemSpec);
				}
				else
				{
					list3.Add(referenceAssembly.ItemSpec);
				}
			}
			else
			{
				list2.Add(referenceAssembly.ItemSpec);
			}
		}
		systemItems = list;
		nonSystemItems = list2;
		systemExtraItems = list3;
	}

	private IEnumerable<IFileItem> GetAdditionalXamlTypeInfoIncludes()
	{
		return ClIncludeFiles?.Where((IFileItem h) => !string.IsNullOrEmpty(h.DependentUpon)).Where(delegate(IFileItem h)
		{
			FileInfo fileInfo = new FileInfo(h.DependentUpon);
			return string.Compare(fileInfo.Extension, ".idl", ignoreCase: false) == 0;
		});
	}

	private Dictionary<string, string> GetClassToHeaderFileMap()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (IsPass1)
		{
			return dictionary;
		}
		if (Language.IsManaged || ClIncludeFiles == null || ClIncludeFiles.Count == 0)
		{
			return dictionary;
		}
		foreach (IFileItem clIncludeFile in ClIncludeFiles)
		{
			string dependentUpon = clIncludeFile.DependentUpon;
			if (string.IsNullOrWhiteSpace(dependentUpon))
			{
				continue;
			}
			string directoryName = Path.GetDirectoryName(dependentUpon);
			dependentUpon = Path.GetFileName(dependentUpon);
			string itemSpec = clIncludeFile.ItemSpec;
			string fileName = Path.GetFileName(itemSpec);
			string directoryName2 = Path.GetDirectoryName(itemSpec);
			string text = Path.Combine(string.IsNullOrEmpty(directoryName) ? directoryName2 : directoryName, dependentUpon);
			string text2 = null;
			if (!Path.IsPathRooted(text))
			{
				text = Path.Combine(ProjectFolderFullpath, text);
			}
			try
			{
				if (text.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) && File.Exists(text))
				{
					using (TextReader fileStream = TaskFileService.GetFileContents(text))
					{
						text2 = XamlNodeStreamHelper.ReadXClassFromXamlFileStream(fileStream, _schemaContext);
					}
					if (text2 != null)
					{
						string fullPath = Path.GetFullPath(itemSpec);
						string directoryName3 = Path.GetDirectoryName(Path.GetFullPath(ProjectPath));
						string defaultXamlLinkMetadata = GetDefaultXamlLinkMetadata(fullPath, fullPath, ProjectFolderFullpath, IncludeFolderList);
						dictionary.Add(text2, defaultXamlLinkMetadata);
					}
				}
			}
			catch (Exception e)
			{
				LogError_XamlInternalError(e, text);
			}
		}
		return dictionary;
	}

	private bool BuildWarningSuppressionList()
	{
		if (SuppressWarnings != null)
		{
			string[] suppressWarnings = SuppressWarnings;
			foreach (string text in suppressWarnings)
			{
				if (!int.TryParse(text, out var result))
				{
					LogError_BadSuppressWarningsString(text);
					return false;
				}
				_suppressedWarnings.Add(result.AsErrorCode());
			}
		}
		return true;
	}

	internal static string GetDefaultXamlLinkMetadata(string fileFullpath, string fileItemSpec, string projectFolderFullpath, IEnumerable<string> includeFullpathList)
	{
		string text = null;
		if (!IsFilePathInOrUnderFolderPath(fileFullpath, projectFolderFullpath, out var _))
		{
			text = GetBestSubPath(fileFullpath, includeFullpathList);
			if (text == null)
			{
				text = Path.GetFileName(fileFullpath);
			}
		}
		else if (Path.IsPathRooted(fileItemSpec) || PathContainsComponent(fileItemSpec, ".."))
		{
			text = fileFullpath.Substring(projectFolderFullpath.Length + 1);
		}
		return text;
	}

	internal static bool IsFilePathInOrUnderFolderPath(string file, string folder, out string relativePath)
	{
		relativePath = null;
		if (string.IsNullOrWhiteSpace(file) || string.IsNullOrWhiteSpace(folder))
		{
			return false;
		}
		if (file.Length <= folder.Length)
		{
			return false;
		}
		if (!folder.EndsWith("\\"))
		{
			folder += "\\";
		}
		if (file.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
		{
			relativePath = file.Substring(folder.Length);
			return true;
		}
		return false;
	}

	internal static string GetBestSubPath(string fullpath, IEnumerable<string> folderList)
	{
		string text = null;
		foreach (string folder in folderList)
		{
			string relativePath = null;
			if (IsFilePathInOrUnderFolderPath(fullpath, folder, out relativePath) && (text == null || relativePath.Length < text.Length))
			{
				text = relativePath;
			}
		}
		return text;
	}

	internal static bool PathContainsComponent(string path, string component)
	{
		string[] array = path.Split(_separator);
		string[] array2 = array;
		foreach (string a in array2)
		{
			if (string.Equals(a, component, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private bool ShouldSuppressTypeInfoCodeGen()
	{
		return _projectInfo.HasCodeGenFlag(CodeGenCtrlFlags.NoTypeInfoCodeGen);
	}

	private bool ShouldSuppressPageCodeGen()
	{
		return _projectInfo.HasCodeGenFlag(CodeGenCtrlFlags.NoPageCodeGen);
	}

	private void LogWarning(XamlCompileWarning warning, string xamlFile)
	{
		if (!_suppressedWarnings.Contains(warning.Code.AsErrorCode()))
		{
			string subcategory = "XamlCompiler";
			Log.LogWarning(subcategory, warning.Code.AsErrorCode(), null, xamlFile, warning.LineNumber, warning.LineOffset, 0, 0, warning.Message);
		}
	}

	private void LogWarningAsInfo(XamlCompileWarning warning)
	{
		Log.LogDiagnosticMessage(warning.Message);
	}

	private void LogWarning(XamlCompileWarning warning)
	{
		LogWarning(warning, warning.FileName);
	}

	private void LogError(XamlCompileError error)
	{
		Log.LogError("XamlCompiler", error.Code.AsErrorCode(), null, error.FileName, error.LineNumber, error.LineOffset, 0, 0, error.Message);
	}

	public void LogError(string subcategory, ErrorCode code, string helpKeyword, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message)
	{
		if (Log != null)
		{
			Log.LogError(subcategory, code.AsErrorCode(), helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message);
		}
	}

	private void LogError_ClassIsNotFoundInAssembly(string className, string localAssemblyName, string xamlFile)
	{
		string message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_xClassTypeIsNotFound, className, localAssemblyName);
		LogError("XamlCompiler", ErrorCode.WMC1002, null, xamlFile, 0, 0, 0, 0, message);
	}

	public void LogError_BadCodeGenFlags(string flag)
	{
		string message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_CodeGenString_Bad, flag);
		LogError("XamlCompiler", ErrorCode.WMC1003, null, null, 0, 0, 0, 0, message);
	}

	private void LogError_EventsInAppXaml(string fileName)
	{
		LogError("XamlCompiler", ErrorCode.WMC1005, null, fileName, 0, 0, 0, 0, XamlCompilerResources.XamlCompiler_NoEventsInAppXaml);
	}

	private void LogError_CannotResolveAssembly(string name)
	{
		LogError("XamlCompiler", ErrorCode.WMC1006, null, ProjectPath, 0, 0, 0, 0, string.Format(XamlCompilerResources.XamlCompiler_CantResolveAssembly, name));
	}

	private void LogError_CannotResolveWinUIMetadata()
	{
		LogError("XamlCompiler", ErrorCode.WMC1007, null, ProjectPath, 0, 0, 0, 0, XamlCompilerResources.XamlCompiler_CantResolveWinUIMetadata);
	}

	private void LogError_ClassDoesntMatchWinmdName(string className, string winmdName, string xamlFile)
	{
		string message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_xClassTypeDoesntMatchWinmd, className, winmdName);
		LogError("XamlCompiler", ErrorCode.WMC1009, null, xamlFile, 0, 0, 0, 0, message);
	}

	private void LogError_XamlFileMustEndInDotXaml(string xamlFile)
	{
		LogError("XamlCompiler", ErrorCode.WMC1010, null, xamlFile, 0, 0, 0, 0, XamlCompilerResources.XamlCompiler_XamlFileMustEndInDotXaml);
	}

	private void LogError_BadSuppressWarningsString(string warn)
	{
		string message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_BadValueInSupressWarningsList, warn);
		LogError("XamlCompiler", ErrorCode.WMC1011, null, ProjectPath, 0, 0, 0, 0, message);
	}

	private void LogError_MoreThanOneApplicationXaml()
	{
		LogError("XamlCompiler", ErrorCode.WMC1012, null, ProjectPath, 0, 0, 0, 0, XamlCompilerResources.XamlCompiler_MoreThanOneApplicationXaml);
	}

	private void LogError_XamlFilesWithSameApparentPath(string xamlFile1, string xamlFile2, string commonApparentPath)
	{
		string message = ResourceUtilities.FormatString(XamlCompilerResources.XamlCompiler_XamlFilesHaveTheSameName, xamlFile1, xamlFile2, commonApparentPath);
		LogError("XamlCompiler", ErrorCode.WMC1013, null, ProjectPath, 0, 0, 0, 0, message);
	}

	private void LogError_XamlXMLParsingError(Exception e, string file)
	{
		LogUnhandledException(XamlCompilerResources.XamlXmlParsingError, ErrorCode.WMC9997, e, file);
	}

	internal void LogError_XamlInternalError(Exception e, string file)
	{
		LogUnhandledException(XamlCompilerResources.XamlInternlError, ErrorCode.WMC9999, e, file);
	}

	internal void LogUnhandledException(string subcategory, ErrorCode code, Exception e, string file)
	{
		XamlException ex = e as XamlException;
		int lineNumber = ex?.LineNumber ?? 0;
		int columnNumber = ex?.LinePosition ?? 0;
		string message = e.Message;
		LogError(subcategory, code, null, file, lineNumber, columnNumber, 0, 0, message);
		Log.LogDiagnosticMessage(e.StackTrace);
	}
}
