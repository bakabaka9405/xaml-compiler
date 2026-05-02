using System;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.Core;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace XamlCompiler.LanguageServer.Services;

/// <summary>
/// 无状态的 XAML 解析服务。
/// 将原始 XAML 文本或 XamlReader 流转换为 XAML DOM 对象树。
/// 解析失败时通过 <see cref="XamlParseResult"/> 携带完整错误语义（行号、列号、错误消息），
/// 而非丢弃为 null 后再拼接无效前缀。
/// </summary>
public class XamlParserService
{
    private readonly XamlCompilerReflectionHelper _reflectionHelper;

    /// <summary>
    /// 初始化 XamlParserService 实例。
    /// </summary>
    public XamlParserService()
    {
        _reflectionHelper = new XamlCompilerReflectionHelper();
    }

    /// <summary>
    /// 将 XAML 文本字符串解析为 DOM 根对象。
    /// 使用 XamlCompilerReflectionHelper.CreateDomRoot 构建完整的对象树。
    /// </summary>
    /// <param name="xamlText">原始 XAML 文本内容。</param>
    /// <param name="sourceFilePath">源文件路径，用于在 DOM 节点中关联位置信息。</param>
    /// <param name="schema">XAML 架构上下文，用于类型解析。</param>
    /// <returns>
    /// 成功时 Result.DomRoot 为非 null；失败时 Result.ErrorMessage 包含编译器原始错误，
    /// Result.ErrorLine / ErrorColumn 指向出错位置（1-based）。
    /// </returns>
    public XamlParseResult Parse(string xamlText, string sourceFilePath, XamlSchemaContext schema)
    {
        if (string.IsNullOrEmpty(xamlText))
            throw new ArgumentNullException(nameof(xamlText));
        if (schema == null)
            throw new ArgumentNullException(nameof(schema));

        try
        {
            var domRoot = _reflectionHelper.CreateDomRoot(xamlText, schema, localAssembly: null);
            return XamlParseResult.Success(domRoot);
        }
        catch (System.Xml.XmlException ex)
        {
            return XamlParseResult.FromException(ex);
        }
        catch (System.Xaml.XamlParseException ex)
        {
            return XamlParseResult.FromException(ex);
        }
        catch (Exception ex)
        {
            return XamlParseResult.FromException(ex);
        }
    }

    /// <summary>
    /// 从已有的 XamlReader 构建 DOM 根对象。
    /// 适用于已由外部创建的读取器流。
    /// </summary>
    public XamlParseResult ParseFromReader(XamlReader reader, string sourceFilePath)
    {
        if (reader == null)
            throw new ArgumentNullException(nameof(reader));

        try
        {
            var domRoot = _reflectionHelper.CreateCompilerDomRoot(reader);
            return XamlParseResult.Success(domRoot);
        }
        catch (Exception ex)
        {
            return XamlParseResult.FromException(ex);
        }
    }
}
