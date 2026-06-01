using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace XamlCompiler.LanguageServer.Services;

/// <summary>
/// 实现 ILog 接口，将编译器的错误/警告/消息转换为 LSP Diagnostic 对象。
/// 累积诊断结果，供后续通过 textDocument/publishDiagnostics 通知推送。
/// </summary>
public class LspDiagnosticCollector : ILog
{
    private readonly List<Diagnostic> _diagnostics = new();
    private string _currentFile = string.Empty;

    /// <summary>
    /// 获取自上次 Reset 以来累积的所有诊断。
    /// </summary>
    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;

    /// <summary>
    /// 设置当前正在处理的文件路径（用于上下文关联）。
    /// </summary>
    public string CurrentFile
    {
        get => _currentFile;
        set => _currentFile = value ?? string.Empty;
    }

    /// <summary>
    /// 重置累积的诊断列表（在处理新文档前调用）。
    /// </summary>
    public void Reset()
    {
        _diagnostics.Clear();
    }

    /// <summary>
    /// 记录编译错误。
    /// </summary>
    public void LogError(string subcategory, string errorCode, string helpKeyword, string file,
        int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message)
    {
        _diagnostics.Add(CreateDiagnostic(DiagnosticSeverity.Error, errorCode, message,
            file, lineNumber, columnNumber, endLineNumber, endColumnNumber));
    }

    /// <summary>
    /// 记录编译警告。
    /// </summary>
    public void LogWarning(string subcategory, string warningCode, string helpKeyword, string file,
        int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message)
    {
        _diagnostics.Add(CreateDiagnostic(DiagnosticSeverity.Warning, warningCode, message,
            file, lineNumber, columnNumber, endLineNumber, endColumnNumber));
    }

    /// <summary>
    /// 记录纯信息消息（编译器的诊断消息，不映射为 LSP Diagnostic）。
    /// </summary>
    public void LogDiagnosticMessage(string message)
    {
        // 信息类消息不映射到 LSP Diagnostic；
        // 可以通过 LogMessage 通知单独发送（Phase 1 实现）
    }

    /// <summary>
    /// 创建 LSP Diagnostic 对象，将 1-based 行号转换为 0-based。
    /// </summary>
    private static Diagnostic CreateDiagnostic(
        DiagnosticSeverity severity,
        string code,
        string message,
        string file,
        int lineNumber,
        int columnNumber,
        int endLineNumber,
        int endColumnNumber)
    {
        // LSP 使用 0-based 位置，编译器使用 1-based
        int startLine = Math.Max(0, lineNumber - 1);
        int startChar = Math.Max(0, columnNumber - 1);
        int endLine = Math.Max(startLine, (endLineNumber > 0 ? endLineNumber : lineNumber) - 1);
        int endChar = Math.Max(startChar, (endColumnNumber > 0 ? endColumnNumber : columnNumber) - 1);

        return new Diagnostic
        {
            Severity = severity,
            Code = code,
            Message = EscapeDiagnosticMessage(message),
            Range = new Range
            {
                Start = new Position { Line = startLine, Character = startChar },
                End = new Position { Line = endLine, Character = endChar }
            },
            Source = "XamlCompiler"
        };
    }

    private static string EscapeDiagnosticMessage(string message)
    {
        return message.Replace("\r", "\\r").Replace("\n", "\\n");
    }
}
