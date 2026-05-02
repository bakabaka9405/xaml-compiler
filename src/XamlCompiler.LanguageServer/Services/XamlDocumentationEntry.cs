using System.Collections.Generic;

namespace XamlCompiler.LanguageServer.Services;

/// <summary>
/// 从 .NET XML 文档文件中提取出的单个成员文档。
/// </summary>
public sealed class XamlDocumentationEntry
{
    public XamlDocumentationEntry(
        string summary,
        string remarks,
        string returns,
        IReadOnlyDictionary<string, string> parameters)
    {
        Summary = summary;
        Remarks = remarks;
        Returns = returns;
        Parameters = parameters;
    }

    public string Summary { get; }

    public string Remarks { get; }

    public string Returns { get; }

    public IReadOnlyDictionary<string, string> Parameters { get; }
}
