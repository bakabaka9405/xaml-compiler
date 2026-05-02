using System;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace XamlCompiler.LanguageServer.Services;

/// <summary>
/// XAML 解析结果，携带成功时的 DOM 根对象或失败时的错误详情。
/// 行号与列号均为 1-based（与编译器、XamlException 一致）。
/// </summary>
public sealed class XamlParseResult
{
    /// <summary>
    /// 解析成功时返回 DOM 根对象；失败时为 null。
    /// </summary>
    public XamlDomObject? DomRoot { get; }

    /// <summary>
    /// 解析失败时的具体错误消息（中文）；成功时为 null。
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// 错误发生的行号（1-based）。无有效位置时为 1。
    /// </summary>
    public int ErrorLine { get; }

    /// <summary>
    /// 错误发生的列号（1-based）。无有效位置时为 1。
    /// </summary>
    public int ErrorColumn { get; }

    /// <summary>
    /// 错误码（如 "XLS0001"）。成功时为 null。
    /// </summary>
    public string? ErrorCode { get; }

    private XamlParseResult(XamlDomObject? domRoot, string? errorMessage, int errorLine, int errorColumn, string? errorCode)
    {
        DomRoot = domRoot;
        ErrorMessage = errorMessage;
        ErrorLine = errorLine;
        ErrorColumn = errorColumn;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// 创建一个成功的解析结果。
    /// </summary>
    public static XamlParseResult Success(XamlDomObject domRoot)
        => new(domRoot ?? throw new ArgumentNullException(nameof(domRoot)), null, 0, 0, null);

    /// <summary>
    /// 从异常创建失败结果，自动提取 LineNumber / LinePosition（XamlException / XmlException）。
    /// </summary>
    public static XamlParseResult FromException(Exception ex)
    {
        int line = 1;
        int col = 1;

        if (ex is System.Xaml.XamlException xamlEx)
        {
            line = xamlEx.LineNumber > 0 ? xamlEx.LineNumber : 1;
            col = xamlEx.LinePosition > 0 ? xamlEx.LinePosition : 1;
        }
        else if (ex is System.Xml.XmlException xmlEx)
        {
            line = xmlEx.LineNumber > 0 ? xmlEx.LineNumber : 1;
            col = xmlEx.LinePosition > 0 ? xmlEx.LinePosition : 1;
        }

        string message = ex.Message;
        string? code = ex is System.Xaml.XamlParseException ? "XLS0001" :
                       ex is System.Xml.XmlException ? "XLS0002" : "XLS0003";

        return new XamlParseResult(null, message, line, col, code);
    }
}
