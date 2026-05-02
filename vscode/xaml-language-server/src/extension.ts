import * as path from 'path';
import * as fs from 'fs';
import { ExtensionContext, workspace } from 'vscode';
import { discoverLspContext } from './lspAssemblyDiscovery';
import {
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    TransportKind,
    Trace
} from 'vscode-languageclient/node';

let client: LanguageClient;

/**
 * VS Code 扩展激活入口。
 * 启动 XAML 语言服务器进程并通过 stdio 建立 LSP 连接。
 */
export async function activate(context: ExtensionContext): Promise<void> {
    const config = workspace.getConfiguration('xaml.server');

    // 确定语言服务器 exe 路径（捆绑在扩展的 server 目录中）
    const serverExe = context.asAbsolutePath(
        path.join('server', 'XamlCompiler.LanguageServer.exe')
    );

    // 检查语言服务器可执行文件是否存在
    if (!fs.existsSync(serverExe)) {
        throw new Error(
            `XAML Language Server executable not found at: ${serverExe}. Run "bun run prepare-server" to build it.`
        );
    }

    // ── 发现 WinUI 参考程序集（不依赖 xmake 构建）──
    const workspaceRoot = workspace.workspaceFolders?.[0]?.uri.fsPath;
    let initializationOptions: Record<string, unknown> | undefined;
    if (workspaceRoot) {
        const lspContext = await discoverLspContext(workspaceRoot);
        if (lspContext) {
            initializationOptions = {
                referenceAssemblies: lspContext.referenceAssemblies,
                namespace: lspContext.namespace,
                sourceDir: lspContext.sourceDir,
                winSdkRoot: lspContext.winSdkRoot,
            };
            console.log(
                `[XAML LS] Discovered ${lspContext.referenceAssemblies.length} reference assemblies`
            );
        } else {
            console.log('[XAML LS] No WinUI assemblies found — semantic validation disabled');
        }
    }

    // 直接启动 .NET 语言服务器进程（net472 Framework 可执行文件）
    const serverOptions: ServerOptions = {
        run: {
            command: serverExe,
            args: [],
            transport: TransportKind.stdio
        },
        debug: {
            command: serverExe,
            args: [],
            transport: TransportKind.stdio
        }
    };

    // 配置客户端选项：文件选择器和同步设置
    const clientOptions: LanguageClientOptions = {
        documentSelector: [
            { scheme: 'file', language: 'xml', pattern: '**/*.xaml' }
        ],
        synchronize: {
            configurationSection: 'xaml.server',
            fileEvents: workspace.createFileSystemWatcher('**/*.xaml')
        },
        outputChannelName: 'XAML Language Server',
        initializationOptions,
    };

    // 创建语言客户端实例
    client = new LanguageClient(
        'xamlLanguageServer',
        'XAML Language Server',
        serverOptions,
        clientOptions
    );

    // 配置日志跟踪级别
    const traceLevel: string = config.get('trace.server', 'off');
    switch (traceLevel) {
        case 'verbose':
            client.setTrace(Trace.Verbose);
            break;
        case 'messages':
            client.setTrace(Trace.Messages);
            break;
        default:
            client.setTrace(Trace.Off);
            break;
    }

    // 启动客户端（start() 返回 Promise<void>，resolve 时表示服务已就绪）
    // 将 disposable 注册到 context 以便停用时自动释放
    context.subscriptions.push(client);

    await client.start();
    console.log('XAML Language Server started successfully');
}

/**
 * VS Code 扩展停用入口。
 * 优雅关闭语言服务器进程。
 */
export function deactivate(): Thenable<void> | undefined {
    if (!client) {
        return undefined;
    }
    return client.stop();
}
