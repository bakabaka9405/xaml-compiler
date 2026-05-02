using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using StreamJsonRpc;

namespace XamlCompiler.LanguageServer.Services;

/// <summary>
/// LSP 文档事件分发器。
/// 接收来自 <see cref="Handlers.LspMessageHandler"/> 的文档操作事件，
/// 协调文档存储、诊断分析以及向客户端的诊断推送。
/// </summary>
public class LspMessageDispatcher
{
    private readonly XamlDocumentStore _store;
    private readonly XamlDiagnosticService _diagnostics;
    private readonly XamlDocumentationService _documentation;
    private readonly XamlHoverService _hover;
    private readonly XamlCompletionService _completion;
    private JsonRpc? _rpc;

    /// <summary>
    /// 初始化 LspMessageDispatcher 实例。
    /// </summary>
    /// <param name="store">XAML 文档内存存储。</param>
    /// <param name="diagnostics">诊断分析服务。</param>
    public LspMessageDispatcher(
        XamlDocumentStore store,
        XamlDiagnosticService diagnostics,
        XamlDocumentationService documentation,
        XamlHoverService hover,
        XamlCompletionService completion)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _diagnostics = diagnostics ?? throw new ArgumentNullException(nameof(diagnostics));
        _documentation = documentation ?? throw new ArgumentNullException(nameof(documentation));
        _hover = hover ?? throw new ArgumentNullException(nameof(hover));
        _completion = completion ?? throw new ArgumentNullException(nameof(completion));
    }

    /// <summary>
    /// 设置 JsonRpc 引用，用于向客户端回发通知（如 textDocument/publishDiagnostics）。
    /// 必须在 initialize 握手完成后调用。
    /// </summary>
    /// <param name="rpc">已初始化的 JsonRpc 实例。</param>
    public void SetRpc(JsonRpc rpc)
    {
        _rpc = rpc ?? throw new ArgumentNullException(nameof(rpc));
    }

    /// <summary>
    /// Called when reference assemblies are discovered by the extension (during initialize).
    /// Builds the schema context and re-publishes diagnostics for all open documents.
    /// </summary>
    /// <param name="assemblyPaths">Absolute paths to WinMD assembly files.</param>
    /// <param name="winSdkRoot">Windows SDK root directory (optional).</param>
    public void OnReferenceAssembliesDiscovered(IReadOnlyList<string> assemblyPaths, string? winSdkRoot)
    {
        _documentation.LoadReferenceAssemblies(assemblyPaths);
        _diagnostics.BuildSchemaContext(assemblyPaths, winSdkRoot);

        // Re-publish diagnostics for all currently open documents
        foreach (var uri in _store.GetAllUris())
        {
            PublishDiagnostics(uri);
        }
    }

    /// <summary>
    /// 处理 hover 请求，使用当前 schema 与 XML 文档索引生成悬停内容。
    /// </summary>
    public Hover? GetHover(TextDocumentPositionParams parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        return _hover.GetHover(parameters, _diagnostics.SchemaContext);
    }

    /// <summary>
    /// 处理 completion 请求，使用当前 schema 生成 XAML 补全候选。
    /// </summary>
    public CompletionList GetCompletion(CompletionParams parameters)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));

        return _completion.GetCompletion(parameters, _diagnostics.SchemaContext);
    }

    /// <summary>
    /// 处理文档打开事件：更新文档存储，触发诊断分析，推送诊断结果。
    /// </summary>
    /// <param name="uri">文档 URI。</param>
    /// <param name="text">文档全文内容。</param>
    public void OnDocumentOpened(string uri, string text)
    {
        // 更新文档存储
        _store.OpenOrUpdate(uri, text);

        // 触发诊断并推送结果
        PublishDiagnostics(uri);
    }

    /// <summary>
    /// 处理文档变更事件：应用变更内容（当前采用全量同步模式），
    /// 触发诊断分析，推送诊断结果。
    /// </summary>
    /// <param name="uri">文档 URI。</param>
    /// <param name="changes">内容变更列表。</param>
    public void OnDocumentChanged(string uri, IEnumerable<TextDocumentContentChangeEvent> changes)
    {
        // 当前阶段：取最后一次变更的全量文本进行替换（TextDocumentSyncKind.Full）
        var lastChange = changes?.LastOrDefault();
        if (lastChange != null)
        {
            _store.OpenOrUpdate(uri, lastChange.Text);
        }

        // 触发诊断并推送结果
        PublishDiagnostics(uri);
    }

    /// <summary>
    /// 处理文档关闭事件：从文档存储中移除。
    /// </summary>
    /// <param name="uri">文档 URI。</param>
    public void OnDocumentClosed(string uri)
    {
        _store.Close(uri);
    }

    /// <summary>
    /// 对指定文档执行诊断分析，并将结果通知推送至客户端。
    /// 异常被捕获并记录，防止未观测任务异常导致进程崩溃。
    /// </summary>
    private void PublishDiagnostics(string uri)
    {
        if (_rpc == null)
            return;

        try
        {
            var diagnostics = _diagnostics.AnalyzeDocument(uri);

            var parameters = new PublishDiagnosticParams
            {
                Uri = new Uri(uri),
                Diagnostics = diagnostics.ToArray()
            };

            _ = _rpc.NotifyAsync("textDocument/publishDiagnostics", parameters)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"Failed to publish diagnostics for {uri}: {t.Exception?.GetBaseException().Message}");
                    }
                },
                System.Threading.CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Error during diagnostics for {uri}: {ex.Message}");
        }
    }
}
