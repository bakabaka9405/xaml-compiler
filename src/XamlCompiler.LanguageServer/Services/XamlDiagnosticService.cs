using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.Lmr;
using Microsoft.UI.Xaml.Markup.Compiler;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace XamlCompiler.LanguageServer.Services;

/// <summary>
/// XAML 文档诊断服务。
/// 协调文档存储、解析器与诊断收集器，对单个或多个 XAML 文档执行解析与校验，
/// 并将结果汇总为 LSP <see cref="Diagnostic"/> 列表。
/// </summary>
public class XamlDiagnosticService
{
    private readonly XamlDocumentStore _documentStore;
    private readonly XamlParserService _parser;
    private readonly LspDiagnosticCollector _collector;
    private DirectUISchemaContext _schemaContext;

    /// <summary>
    /// 初始化诊断服务。
    /// 创建默认的 DirectUISchemaContext 以支持 WinUI 类型解析。
    /// 注意：无程序集参数的 DirectUISchemaContext 仅支持内置 XAML 类型；
    /// 完整的 WinUI 类型解析需要加载参考程序集（Phase 1 实现）。
    /// </summary>
    public XamlDiagnosticService(
        XamlDocumentStore documentStore,
        XamlParserService parser,
        LspDiagnosticCollector collector)
    {
        _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _collector = collector ?? throw new ArgumentNullException(nameof(collector));
        _schemaContext = new DirectUISchemaContext(
            referenceAssemblies: Array.Empty<System.Reflection.Assembly>(),
            systemExtraReferenceItems: new System.Collections.Generic.List<string>(),
            localAssembly: null!,
            staticLibraryAssemblies: null!,
            sdkPath: null!,
            isStringNullable: false);
    }

    /// <summary>
    /// 获取当前使用的 XAML 架构上下文，供外部调用方在需要时替换为自定义实例。
    /// </summary>
    public DirectUISchemaContext SchemaContext => _schemaContext;

    /// <summary>
    /// Runtime build of DirectUISchemaContext using WinMD reference assemblies
    /// discovered by the VS Code extension. Supports hot-reload without LSP restart.
    /// Uses XamlTypeUniverse.LoadAssemblyFromFile() for metadata-only loading
    /// compatible with the IAssembly2 type system required by DirectUISystem.
    /// </summary>
    /// <param name="assemblyPaths">Absolute paths to WinMD assembly files.</param>
    /// <param name="winSdkRoot">Windows SDK root directory (optional, for facade filtering).</param>
    public void BuildSchemaContext(IReadOnlyList<string> assemblyPaths, string? winSdkRoot)
    {
        if (assemblyPaths == null || assemblyPaths.Count == 0)
            return;

        var typeUniverse = new XamlTypeUniverse(useManagedProjections: false);

        var loadedAssemblies = new List<System.Reflection.Assembly>();
        foreach (var filePath in assemblyPaths)
        {
            try
            {
                var asm = typeUniverse.LoadAssemblyFromFile(filePath);
                loadedAssemblies.Add(asm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Failed to load assembly '{filePath}': {ex.Message}");
            }
        }

        if (loadedAssemblies.Count == 0)
            return;

        var typeResolver = new TypeResolver(typeUniverse);
        typeResolver.InitializeTypeNameMap();

        _schemaContext = new DirectUISchemaContext(
            referenceAssemblies: loadedAssemblies,
            systemExtraReferenceItems: new System.Collections.Generic.List<string>(),
            localAssembly: null!,
            staticLibraryAssemblies: null!,
            sdkPath: winSdkRoot ?? string.Empty,
            isStringNullable: false);

        _schemaContext.TypeResolver = typeResolver;
    }

    public IReadOnlyList<Diagnostic> AnalyzeDocument(string uri)
    {
        if (string.IsNullOrEmpty(uri))
            throw new ArgumentNullException(nameof(uri));

        _collector.Reset();
        _collector.CurrentFile = uri;

        string? text = _documentStore.GetText(uri);
        if (text == null)
            return Array.Empty<Diagnostic>();

        // 解析阶段：使用 DirectUISchemaContext 进行 WinUI 类型感知的 XAML 解析
        var result = _parser.Parse(text, uri, _schemaContext);

        if (result.DomRoot == null)
        {
            _collector.LogError(
                subcategory: "XamlParsing",
                errorCode: result.ErrorCode ?? "XLS0001",
                helpKeyword: string.Empty,
                file: uri,
                lineNumber: result.ErrorLine,
                columnNumber: result.ErrorColumn,
                endLineNumber: result.ErrorLine,
                endColumnNumber: result.ErrorColumn,
                message: result.ErrorMessage ?? "Unknown parsing failure");
        }
        else
        {
            // Validation phase: run XamlDomValidator for semantic diagnostics
            try
            {
                var validator = new XamlDomValidator
                {
                    IsPass1 = false,
                    XamlPlatform = Platform.Any
                };

                // Guard: NullReferenceException is expected when WinUI core assemblies
                // are not fully loaded (Style, FrameworkElement, etc. may be null).
                // Using a blanket catch (no 'when' filter) because:
                // 1. JIT inlining may remove property getters from the stack trace
                // 2. NRE can originate from multiple DirectUISystem properties,
                //    not just Style (e.g. FrameworkElement at line 220, domObject.Type at line 242)
                // 3. In an LSP context, incomplete assembly loading is normal
                try
                {
                    validator.Validate(result.DomRoot);
                }
                catch (NullReferenceException)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Semantic validation skipped: WinUI core assemblies may be incomplete ({uri})");
                    return new List<Diagnostic>(_collector.Diagnostics);
                }

                // Convert validation errors to LSP diagnostics
                foreach (var error in validator.Errors)
                {
                    _collector.LogError(
                        subcategory: "XamlValidation",
                        errorCode: error.Code.ToString(),
                        helpKeyword: string.Empty,
                        file: error.FileName ?? uri,
                        lineNumber: error.LineNumber,
                        columnNumber: error.LineOffset,
                        endLineNumber: error.LineNumber,
                        endColumnNumber: error.LineOffset,
                        message: error.Message);
                }

                // Convert validation warnings to LSP diagnostics
                foreach (var warning in validator.Warnings)
                {
                    _collector.LogWarning(
                        subcategory: "XamlValidation",
                        warningCode: warning.Code.ToString(),
                        helpKeyword: string.Empty,
                        file: warning.FileName ?? uri,
                        lineNumber: warning.LineNumber,
                        columnNumber: warning.LineOffset,
                        endLineNumber: warning.LineNumber,
                        endColumnNumber: warning.LineOffset,
                        message: warning.Message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Semantic validation error for {uri}: {ex.Message}");

                // Report a fallback diagnostic so silent failures are visible to the user
                _collector.LogError(
                    subcategory: "XamlValidation",
                    errorCode: "XLS0002",
                    helpKeyword: string.Empty,
                    file: uri,
                    lineNumber: 0,
                    columnNumber: 0,
                    endLineNumber: 0,
                    endColumnNumber: 0,
                    message: $"Semantic validation failed: {ex.Message}");
            }
        }

        return [.. _collector.Diagnostics];
    }

    public IReadOnlyList<Diagnostic> AnalyzeDocuments(IEnumerable<string> uris)
    {
        if (uris == null)
            throw new ArgumentNullException(nameof(uris));

        var allDiagnostics = new List<Diagnostic>();
        foreach (string uri in uris)
        {
            allDiagnostics.AddRange(AnalyzeDocument(uri));
        }
        return allDiagnostics;
    }
}
