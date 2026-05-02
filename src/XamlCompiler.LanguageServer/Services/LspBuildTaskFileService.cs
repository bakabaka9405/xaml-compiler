using System;
using System.IO;
using Microsoft.UI.Xaml.Markup.Compiler;

namespace XamlCompiler.LanguageServer.Services;

/// <summary>
/// BuildTaskFileService 的子类，重写文件读取以从 LSP 内存缓冲区获取内容，
/// 而非直接从磁盘文件系统读取。支持 IDE 宿主场景。
/// </summary>
public class LspBuildTaskFileService : BuildTaskFileService
{
    private readonly XamlDocumentStore _documentStore;

    /// <summary>
    /// 表示此实例运行在 IDE 宿主中（而非完整 MSBuild 构建）。
    /// </summary>
    public override bool HasIdeHost => true;

    /// <summary>
    /// 表示此实例不执行真实的文件系统构建。
    /// </summary>
    public override bool IsRealBuild => false;

    /// <summary>
    /// 初始化 LspBuildTaskFileService。
    /// </summary>
    /// <param name="documentStore">包含所有已打开文档的内存文档存储。</param>
    /// <param name="languageExtension">目标语言的扩展名（如 ".cs"、".vb"）。</param>
    public LspBuildTaskFileService(XamlDocumentStore documentStore, string languageExtension = ".cs")
        : base(languageExtension)
    {
        _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
    }

    /// <summary>
    /// 从内存文档存储中获取文件内容，若不在存储中则回退到文件系统。
    /// </summary>
    /// <param name="srcFile">源文件路径（用作存储键）。</param>
    /// <returns>包含文件内容的 TextReader。</returns>
    public override TextReader GetFileContents(string srcFile)
    {
        string? text = _documentStore.GetText(srcFile);
        if (text != null)
        {
            return new StringReader(text);
        }

        // 回退到文件系统（用于 x:Class 代码后置文件等未在编辑器中打开的关联文件）
        return base.GetFileContents(srcFile);
    }
}
