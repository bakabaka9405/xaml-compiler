using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace XamlCompiler.LanguageServer.Services;

/// <summary>
/// 加载 WinMD 旁的同名 XML 文档文件，并按标准成员 ID 提供快速查询。
/// </summary>
public sealed class XamlDocumentationService
{
    private readonly object _gate = new();
    private Dictionary<string, XamlDocumentationEntry> _entries = new(StringComparer.Ordinal);

    /// <summary>
    /// 根据参考程序集路径重建 XML 文档索引。
    /// </summary>
    public void LoadReferenceAssemblies(IEnumerable<string> assemblyPaths)
    {
        if (assemblyPaths == null)
            throw new ArgumentNullException(nameof(assemblyPaths));

        var entries = new Dictionary<string, XamlDocumentationEntry>(StringComparer.Ordinal);

        foreach (string assemblyPath in assemblyPaths.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            string? xmlPath = GetCompanionXmlPath(assemblyPath);
            if (xmlPath == null)
                continue;

            LoadXmlDocument(xmlPath, entries);
        }

        lock (_gate)
        {
            _entries = entries;
        }
    }

    /// <summary>
    /// 尝试按 XML 文档成员 ID 查询文档。
    /// </summary>
    public bool TryGetEntry(string memberId, out XamlDocumentationEntry entry)
    {
        if (memberId == null)
            throw new ArgumentNullException(nameof(memberId));

        lock (_gate)
        {
            return _entries.TryGetValue(memberId, out entry);
        }
    }

    private static string? GetCompanionXmlPath(string assemblyPath)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath))
            return null;

        string extension = Path.GetExtension(assemblyPath);
        if (!extension.Equals(".winmd", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".dll", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string xmlPath = Path.ChangeExtension(assemblyPath, ".xml");
        return File.Exists(xmlPath) ? xmlPath : null;
    }

    private static void LoadXmlDocument(
        string xmlPath,
        IDictionary<string, XamlDocumentationEntry> entries)
    {
        try
        {
            var document = XDocument.Load(xmlPath, LoadOptions.None);
            XElement? members = document.Root?.Element("members");
            if (members == null)
                return;

            foreach (XElement member in members.Elements("member"))
            {
                string? name = member.Attribute("name")?.Value;
                if (name == null || string.IsNullOrWhiteSpace(name))
                    continue;

                string memberName = name;
                entries[memberName] = new XamlDocumentationEntry(
                    summary: ReadElementText(member, "summary"),
                    remarks: ReadElementText(member, "remarks"),
                    returns: ReadElementText(member, "returns"),
                    parameters: ReadParameters(member));
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Xml.XmlException)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load XML documentation '{xmlPath}': {ex.Message}");
        }
    }

    private static IReadOnlyDictionary<string, string> ReadParameters(XElement member)
    {
        var parameters = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (XElement parameter in member.Elements("param"))
        {
            string? name = parameter.Attribute("name")?.Value;
            if (name != null && !string.IsNullOrWhiteSpace(name))
            {
                string parameterName = name;
                parameters[parameterName] = NormalizeText(parameter.Value);
            }
        }

        return parameters;
    }

    private static string ReadElementText(XElement parent, string elementName)
    {
        XElement? element = parent.Element(elementName);
        return element == null ? string.Empty : NormalizeText(element.Value);
    }

    private static string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return Regex.Replace(value, @"\s+", " ").Trim();
    }
}
