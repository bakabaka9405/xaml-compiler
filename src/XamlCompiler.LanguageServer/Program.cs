using System;
using System.Threading.Tasks;
using StreamJsonRpc;
using XamlCompiler.LanguageServer.Handlers;
using XamlCompiler.LanguageServer.Services;

namespace XamlCompiler.LanguageServer;

/// <summary>
/// 入口点：通过 stdio 启动 XAML LSP 服务器。
/// 使用 StreamJsonRpc 进行 JSON-RPC 传输，实现 LSP 协议生命周期。
/// </summary>
public static class Program
{
    private static readonly TaskCompletionSource<bool> _shutdownSource = new();

    public static async Task Main(string[] args)
    {
        // 初始化服务层
        var documentStore = new XamlDocumentStore();
        var parser = new XamlParserService();
        var collector = new LspDiagnosticCollector();
        var documentationService = new XamlDocumentationService();
        var diagnosticService = new XamlDiagnosticService(documentStore, parser, collector);
        var hoverService = new XamlHoverService(documentStore, parser, documentationService);
        var completionService = new XamlCompletionService(documentStore, parser, documentationService);

        // 初始化消息分发层
        var dispatcher = new LspMessageDispatcher(
            documentStore,
            diagnosticService,
            documentationService,
            hoverService,
            completionService);

        // 创建 StreamJsonRpc 目标对象
        var handler = new LspMessageHandler(dispatcher);

        // 通过标准输入输出流建立 JSON-RPC 连接
        // StreamJsonRpc 在 net472 平台上无法自动将命名参数对象
        // 反序列化为单一复杂类型参数；handler 在内部手动完成此步骤。
        using var rpc = JsonRpc.Attach(
            Console.OpenStandardOutput(),
            Console.OpenStandardInput(),
            handler);

        // RPC 连接建立后设置引用，消除竞态条件
        dispatcher.SetRpc(rpc);

        // 等待 shutdown 信号（由 LspMessageHandler.Exit 触发）
        await _shutdownSource.Task;

        // 优雅关闭：释放 RPC 资源
        rpc.Dispose();
    }

    /// <summary>
    /// 触发服务器关闭，由 LspMessageHandler.Exit 调用。
    /// </summary>
    public static void SignalShutdown()
    {
        _shutdownSource.TrySetResult(true);
    }
}
