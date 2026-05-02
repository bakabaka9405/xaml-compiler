using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using StreamJsonRpc;
using XamlCompiler.LanguageServer.Services;

namespace XamlCompiler.LanguageServer.Handlers;

/// <summary>
/// StreamJsonRpc 目标对象，接收并分发 LSP JSON-RPC 消息。
/// 每个公共方法对应一个 LSP 方法，由 StreamJsonRpc 通过 <see cref="JsonRpcMethodAttribute"/> 反射调用。
/// 此处理器仅负责消息路由，不包含具体的功能实现。
///
/// 注意：方法参数使用 <see cref="JToken"/> 接收原始 JSON，
/// 并在内部手动反序列化，以绕过 StreamJsonRpc 在 net472 上对
/// 命名参数对象反序列化的限制。
/// </summary>
public class LspMessageHandler
{
    private readonly LspMessageDispatcher _dispatcher;

    /// <summary>
    /// 用于 LSP 请求参数反序列化的 JsonSerializer 实例，
    /// 配置 CamelCase 命名策略以匹配 LSP 协议的 JSON 键名约定。
    /// </summary>
    private static readonly JsonSerializer Serializer = new JsonSerializer
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    /// <summary>
    /// 初始化 LspMessageHandler 实例。
    /// </summary>
    /// <param name="dispatcher">消息分发器，负责将 LSP 事件委托至各服务。</param>
    public LspMessageHandler(LspMessageDispatcher dispatcher)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    /// <summary>
    /// 处理 LSP initialize 请求，返回服务器能力声明。
    /// </summary>
    [JsonRpcMethod("initialize")]
    public InitializeResult Initialize(JToken token)
    {
        var parameters = token.ToObject<InitializeParams>(Serializer);

        // Extract referenceAssemblies and pass to dispatcher
        if (parameters?.InitializationOptions is JObject options)
        {
            var assemblyPaths = options["referenceAssemblies"] as JArray;
            var winSdkRoot = options["winSdkRoot"]?.Value<string>();
            if (assemblyPaths != null && assemblyPaths.Count > 0)
            {
                var paths = new List<string>();
                foreach (var pathToken in assemblyPaths)
                {
                    var filePath = pathToken.Value<string>();
                    if (filePath is not null && filePath.Length > 0)
                        paths.Add(filePath);
                }
                _dispatcher.OnReferenceAssembliesDiscovered(paths, winSdkRoot);
            }
        }

        return new InitializeResult
        {
            Capabilities = new ServerCapabilities
            {
                TextDocumentSync = new TextDocumentSyncOptions
                {
                    OpenClose = true,
                    Change = TextDocumentSyncKind.Full
                },
                HoverProvider = true,
                CompletionProvider = new CompletionOptions
                {
                    TriggerCharacters = new[] { "<", ".", "\"", " ", ":" }
                }
            }
        };
    }

    /// <summary>
    /// 处理 LSP initialized 通知，客户端握手完成后的回调。
    /// </summary>
    [JsonRpcMethod("initialized")]
    public void Initialized()
    {
        System.Diagnostics.Debug.WriteLine("XAML LSP Server initialized");
    }

    /// <summary>
    /// 处理 textDocument/didOpen 通知，委托至调度器执行文档打开逻辑。
    /// </summary>
    [JsonRpcMethod("textDocument/didOpen")]
    public void DidOpen(JToken token)
    {
        var parameters = token.ToObject<DidOpenTextDocumentParams>(Serializer)!;
        _dispatcher.OnDocumentOpened(
            parameters.TextDocument.Uri.AbsoluteUri,
            parameters.TextDocument.Text);
    }

    /// <summary>
    /// 处理 textDocument/didChange 通知，委托至调度器执行文档变更逻辑。
    /// 当前使用全量同步模式（TextDocumentSyncKind.Full），取最后一条变更的全量文本。
    /// </summary>
    [JsonRpcMethod("textDocument/didChange")]
    public void DidChange(JToken token)
    {
        var parameters = token.ToObject<DidChangeTextDocumentParams>(Serializer)!;
        _dispatcher.OnDocumentChanged(
            parameters.TextDocument.Uri.AbsoluteUri,
            parameters.ContentChanges);
    }

    /// <summary>
    /// 处理 textDocument/didClose 通知，委托至调度器执行文档关闭逻辑。
    /// </summary>
    [JsonRpcMethod("textDocument/didClose")]
    public void DidClose(JToken token)
    {
        var parameters = token.ToObject<DidCloseTextDocumentParams>(Serializer)!;
        _dispatcher.OnDocumentClosed(parameters.TextDocument.Uri.AbsoluteUri);
    }

    /// <summary>
    /// 处理 LSP shutdown 请求，返回 null 表示服务器接受关闭。
    /// </summary>
    [JsonRpcMethod("shutdown")]
    public object? Shutdown()
    {
        return null;
    }

    /// <summary>
    /// 处理 LSP exit 通知，触发进程优雅关闭。
    /// </summary>
    [JsonRpcMethod("exit")]
    public void Exit()
    {
        Program.SignalShutdown();
    }

    /// <summary>
    /// 处理 textDocument/hover 请求。
    /// 根据光标位置解析 XAML 类型或成员，并返回从 WinMD 同名 XML 文档中提取的悬停说明。
    /// </summary>
    [JsonRpcMethod("textDocument/hover")]
    public Hover? Hover(JToken token)
    {
        var parameters = token.ToObject<TextDocumentPositionParams>(Serializer)!;
        return _dispatcher.GetHover(parameters);
    }

    /// <summary>
    /// 处理 textDocument/completion 请求。
    /// 根据当前 XAML 编辑上下文返回元素、属性、枚举值等补全建议。
    /// </summary>
    [JsonRpcMethod("textDocument/completion")]
    public CompletionList Completion(JToken token)
    {
        var parameters = token.ToObject<CompletionParams>(Serializer)!;
        return _dispatcher.GetCompletion(parameters);
    }
}
