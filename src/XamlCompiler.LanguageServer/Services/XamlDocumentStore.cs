using System.Collections.Concurrent;
using System.Collections.Generic;

namespace XamlCompiler.LanguageServer.Services;

/// <summary>
/// 线程安全的内存文档存储。
/// 管理 LSP 服务器中所有打开文档的文本内容和版本号。
/// </summary>
public class XamlDocumentStore
{
    private readonly ConcurrentDictionary<string, DocumentEntry> _documents = new();

    /// <summary>
    /// 打开或更新文档内容。写入操作在 lock 保护下执行，
    /// 与 ConcurrentDictionary 的原子操作屏障协作保证线程安全。
    /// </summary>
    public void OpenOrUpdate(string uri, string text, int version = 1)
    {
        _documents.AddOrUpdate(
            uri,
            _ => new DocumentEntry(text, version),
            (_, existing) =>
            {
                lock (existing)
                {
                    existing.Text = text;
                    existing.Version = version;
                }
                return existing;
            });
    }

    /// <summary>
    /// 关闭文档，从存储中移除。
    /// </summary>
    /// <param name="uri">文档的 URI 标识符。</param>
    public void Close(string uri)
    {
        _documents.TryRemove(uri, out _);
    }

    /// <summary>
    /// 获取文档的当前文本内容。
    /// </summary>
    /// <param name="uri">文档的 URI 标识符。</param>
    /// <returns>文档文本，若不存在则返回 null。</returns>
    public string? GetText(string uri)
    {
        return _documents.TryGetValue(uri, out var entry) ? entry.Text : null;
    }

    /// <summary>
    /// 检查文档是否已在存储中。
    /// </summary>
    /// <param name="uri">文档的 URI 标识符。</param>
    public bool Contains(string uri)
    {
        return _documents.ContainsKey(uri);
    }

    /// <summary>
    /// 获取所有当前打开文档的 URI 列表。
    /// </summary>
    public IEnumerable<string> GetAllUris()
    {
        return _documents.Keys;
    }

    private sealed class DocumentEntry
    {
        public string Text;
        public int Version;

        public DocumentEntry(string text, int version)
        {
            Text = text;
            Version = version;
        }
    }
}
