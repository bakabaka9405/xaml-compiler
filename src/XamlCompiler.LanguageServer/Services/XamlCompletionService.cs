using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xaml;
using System.Xaml.Schema;
using Microsoft.UI.Xaml.Markup.Compiler.DirectUI;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace XamlCompiler.LanguageServer.Services;

/// <summary>
/// 基于当前 XAML 编辑上下文、DOM 与 WinMD 元数据生成 LSP completion 候选。
/// </summary>
public sealed class XamlCompletionService
{
    private const string PresentationNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private const string DirectUiNamespace = "http://schemas.microsoft.com/windows/2010/directui";
    private const string XamlLanguageNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

    private static readonly string[] XamlDirectiveNames =
    {
        "Class",
        "ClassModifier",
        "DataType",
        "DefaultBindMode",
        "DeferLoadStrategy",
        "FieldModifier",
        "Key",
        "Load",
        "Name",
        "Phase",
        "SuppressXamlTrimWarnings",
        "Uid"
    };

    private static readonly string[] MarkupExtensionNames =
    {
        "Binding",
        "StaticResource",
        "ThemeResource",
        "TemplateBinding"
    };

    private readonly XamlDocumentStore _documentStore;
    private readonly XamlParserService _parser;
    private readonly XamlDocumentationService _documentation;
    private readonly object _cacheGate = new();
    private readonly Dictionary<string, CompletionItem[]> _typeCompletionCache = new(StringComparer.Ordinal);
    private XamlSchemaContext? _cachedSchema;

    public XamlCompletionService(
        XamlDocumentStore documentStore,
        XamlParserService parser,
        XamlDocumentationService documentation)
    {
        _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _documentation = documentation ?? throw new ArgumentNullException(nameof(documentation));
    }

    /// <summary>
    /// 生成 textDocument/completion 响应；无法识别上下文时返回空列表而非 null。
    /// </summary>
    public CompletionList GetCompletion(CompletionParams parameters, XamlSchemaContext schema)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        if (schema == null)
            throw new ArgumentNullException(nameof(schema));

        string uri = parameters.TextDocument.Uri.AbsoluteUri;
        string? text = _documentStore.GetText(uri);
        if (text == null || text.Length == 0)
            return CreateList(Array.Empty<CompletionItem>());

        string documentText = text;

        CompletionContextInfo? context = CompletionContextInfo.TryCreate(documentText, parameters.Position);
        if (context == null)
            return CreateList(Array.Empty<CompletionItem>());

        XamlDomObject? domRoot = TryParseDom(documentText, uri, schema);
        XamlDomObject? contextObject = domRoot == null
            ? null
            : FindInnermostObject(domRoot, parameters.Position.Line + 1, parameters.Position.Character + 1);

        CompletionItem[] items = context.Kind switch
        {
            CompletionContextKind.ElementName => CreateElementCompletions(schema, contextObject, context),
            CompletionContextKind.AttributeName => CreateAttributeNameCompletions(schema, contextObject, context),
            CompletionContextKind.AttributeValue => CreateAttributeValueCompletions(schema, contextObject, context),
            _ => Array.Empty<CompletionItem>()
        };

        return CreateList(items);
    }

    private XamlDomObject? TryParseDom(string text, string uri, XamlSchemaContext schema)
    {
        XamlParseResult result = _parser.Parse(text, uri, schema);
        return result.DomRoot;
    }

    private CompletionItem[] CreateElementCompletions(
        XamlSchemaContext schema,
        XamlDomObject? contextObject,
        CompletionContextInfo context)
    {
        string? xamlNamespace = ResolveCompletionNamespace(schema, contextObject, context, preferElementPrefix: true);
        if (xamlNamespace == null)
            return Array.Empty<CompletionItem>();

        if (xamlNamespace.Equals(XamlLanguageNamespace, StringComparison.Ordinal))
            return ApplyReplacementEdits(CreateXamlLanguageTypeItems(context.PrefixQualifier), context.ReplacementRange);

        CompletionItem[] namespaceItems = GetTypeCompletionItems(schema, xamlNamespace, context.PrefixQualifier);
        return ApplyReplacementEdits(FilterByPrefix(namespaceItems, context.ReplacementPrefix), context.ReplacementRange);
    }

    private CompletionItem[] CreateAttributeNameCompletions(
        XamlSchemaContext schema,
        XamlDomObject? contextObject,
        CompletionContextInfo context)
    {
        XamlType? contextType = ResolveContextType(schema, contextObject, context);
        var items = new List<CompletionItem>();

        // Phase 1: 当前元素类型的实例属性与事件。
        if (contextType != null)
        {
            AddMemberCompletions(items, contextType, context.ExistingAttributes);
            AddAttachableMemberCompletions(items, contextType, context);
        }

        // Phase 2: x: 指令与 XML 命名空间声明。
        AddDirectiveCompletions(items, context);
        AddXmlNamespaceCompletions(items, context);

        return ApplyReplacementEdits(DeduplicateAndFilter(items, context.ReplacementPrefix), context.ReplacementRange);
    }

    private CompletionItem[] CreateAttributeValueCompletions(
        XamlSchemaContext schema,
        XamlDomObject? contextObject,
        CompletionContextInfo context)
    {
        if (IsXmlNamespaceAttribute(context.AttributeName))
            return ApplyReplacementEdits(FilterByPrefix(CreateXmlNamespaceValueItems(), context.ValuePrefix), context.ReplacementRange);

        if (context.IsMarkupExtensionValue)
            return ApplyReplacementEdits(
                FilterByPrefix(CreateMarkupExtensionItems(context, includeBraces: false), context.MarkupExtensionPrefix),
                context.ReplacementRange);

        XamlType? contextType = ResolveContextType(schema, contextObject, context);
        XamlMember? member = contextType == null
            ? null
            : TryResolveMember(schema, contextObject, context, contextType, context.AttributeName);

        if (member == null)
            return Array.Empty<CompletionItem>();

        var items = new List<CompletionItem>();

        // Phase 1: 枚举值。
        DirectUIXamlType? directType = member.Type as DirectUIXamlType;
        if (directType != null && directType.IsEnum && directType.EnumNames != null)
        {
            foreach (string enumName in directType.EnumNames)
            {
                items.Add(new CompletionItem
                {
                    Label = enumName,
                    Kind = CompletionItemKind.EnumMember,
                    Detail = GetTypeDisplayName(member.Type),
                    Documentation = CreateEnumValueDocumentation(member.Type, enumName),
                    InsertTextFormat = InsertTextFormat.Plaintext
                });
            }
        }

        // Phase 2: 布尔值。
        string? typeName = TryGetTypeFullName(member.Type);
        if (typeName == "System.Boolean")
        {
            items.Add(CreateKeywordItem("True", "Boolean value"));
            items.Add(CreateKeywordItem("False", "Boolean value"));
        }

        // Phase 3: 通用标记扩展入口。
        items.AddRange(CreateMarkupExtensionItems(context, includeBraces: true));

        return ApplyReplacementEdits(DeduplicateAndFilter(items, context.ValuePrefix), context.ReplacementRange);
    }

    private void AddMemberCompletions(
        ICollection<CompletionItem> items,
        XamlType contextType,
        ISet<string> existingAttributes)
    {
        IEnumerable<XamlMember>? members = TryGetAllMembers(contextType);
        if (members == null)
            return;

        foreach (XamlMember member in members)
        {
            if (!IsCompletableMember(member) || existingAttributes.Contains(member.Name))
                continue;

            items.Add(new CompletionItem
            {
                Label = member.Name,
                Kind = member.IsEvent ? CompletionItemKind.Event : CompletionItemKind.Property,
                Detail = GetMemberDetail(member),
                Documentation = CreateMemberDocumentation(member),
                InsertText = member.Name + "=\"$0\"",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }
    }

    private void AddAttachableMemberCompletions(
        ICollection<CompletionItem> items,
        XamlType contextType,
        CompletionContextInfo context)
    {
        string? ownerName = context.AttachableOwnerPrefix;
        if (ownerName == null || string.IsNullOrWhiteSpace(ownerName))
            return;

        XamlType? ownerType = TryResolveElementType(null, context, contextType.SchemaContext, ownerName);
        IEnumerable<XamlMember>? attachableMembers = ownerType == null ? null : TryGetAllAttachableMembers(ownerType);
        if (attachableMembers == null)
            return;

        string ownerLabel = ownerName;
        foreach (XamlMember member in attachableMembers)
        {
            string label = ownerLabel + "." + member.Name;
            if (context.ExistingAttributes.Contains(label))
                continue;

            items.Add(new CompletionItem
            {
                Label = label,
                Kind = CompletionItemKind.Property,
                Detail = GetMemberDetail(member),
                Documentation = CreateMemberDocumentation(member),
                InsertText = label + "=\"$0\"",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }
    }

    private void AddDirectiveCompletions(ICollection<CompletionItem> items, CompletionContextInfo context)
    {
        string xamlPrefix = context.GetPrefixForNamespace(XamlLanguageNamespace) ?? "x";
        foreach (string directiveName in XamlDirectiveNames)
        {
            string label = xamlPrefix + ":" + directiveName;
            if (context.ExistingAttributes.Contains(label))
                continue;

            items.Add(new CompletionItem
            {
                Label = label,
                Kind = CompletionItemKind.Keyword,
                Detail = "XAML language directive",
                Documentation = CreatePlainDocumentation("XAML language directive `" + label + "`."),
                InsertText = label + "=\"$0\"",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }
    }

    private static void AddXmlNamespaceCompletions(ICollection<CompletionItem> items, CompletionContextInfo context)
    {
        if (!context.ExistingAttributes.Contains("xmlns"))
        {
            items.Add(new CompletionItem
            {
                Label = "xmlns",
                Kind = CompletionItemKind.Property,
                Detail = "Default XML namespace declaration",
                Documentation = CreatePlainDocumentation("Declares the default XML namespace for XAML elements."),
                InsertText = "xmlns=\"$0\"",
                InsertTextFormat = InsertTextFormat.Snippet
            });
        }

        items.Add(new CompletionItem
        {
            Label = "xmlns:x",
            Kind = CompletionItemKind.Property,
            Detail = "XAML language namespace declaration",
            Documentation = CreatePlainDocumentation("Declares the conventional `x` namespace for XAML language directives."),
            InsertText = "xmlns:x=\"" + XamlLanguageNamespace + "\"",
            InsertTextFormat = InsertTextFormat.Plaintext
        });
    }

    private CompletionItem[] GetTypeCompletionItems(
        XamlSchemaContext schema,
        string xamlNamespace,
        string? prefixQualifier)
    {
        lock (_cacheGate)
        {
            if (!ReferenceEquals(_cachedSchema, schema))
            {
                _cachedSchema = schema;
                _typeCompletionCache.Clear();
            }

            string cacheKey = xamlNamespace + "|" + (prefixQualifier ?? string.Empty);
            if (_typeCompletionCache.TryGetValue(cacheKey, out CompletionItem[] cachedItems))
                return cachedItems;

            CompletionItem[] items = CreateTypeCompletionItems(schema, xamlNamespace, prefixQualifier);
            _typeCompletionCache[cacheKey] = items;
            return items;
        }
    }

    private CompletionItem[] CreateTypeCompletionItems(
        XamlSchemaContext schema,
        string xamlNamespace,
        string? prefixQualifier)
    {
        ICollection<XamlType>? types = TryGetAllXamlTypes(schema, xamlNamespace);
        if (types == null)
            return Array.Empty<CompletionItem>();

        var items = new List<CompletionItem>();
        foreach (XamlType type in types)
        {
            if (!IsCompletableType(type))
                continue;

            string label = string.IsNullOrEmpty(prefixQualifier)
                ? type.Name
                : prefixQualifier + ":" + type.Name;

            items.Add(new CompletionItem
            {
                Label = label,
                Kind = CompletionItemKind.Class,
                Detail = TryGetTypeFullName(type) ?? xamlNamespace,
                Documentation = CreateTypeDocumentation(type),
                InsertText = label,
                InsertTextFormat = InsertTextFormat.Plaintext,
                SortText = "1_" + label
            });
        }

        return items
            .GroupBy(item => item.Label, StringComparer.Ordinal)
            .Select(group => group.First())
            .OrderBy(item => item.Label, StringComparer.Ordinal)
            .ToArray();
    }

    private static CompletionItem[] CreateXamlLanguageTypeItems(string? prefixQualifier)
    {
        string prefix = prefixQualifier == null || prefixQualifier.Length == 0 ? "x" : prefixQualifier;
        string[] names = { "Null", "String", "Double", "Int32", "Boolean", "Object" };
        return names.Select(name => new CompletionItem
        {
            Label = prefix + ":" + name,
            Kind = CompletionItemKind.Class,
            Detail = "XAML language type",
            Documentation = CreatePlainDocumentation("Built-in XAML language type `" + prefix + ":" + name + "`."),
            InsertText = prefix + ":" + name,
            InsertTextFormat = InsertTextFormat.Plaintext
        }).ToArray();
    }

    private CompletionItem[] CreateMarkupExtensionItems(CompletionContextInfo context, bool includeBraces)
    {
        string xamlPrefix = context.GetPrefixForNamespace(XamlLanguageNamespace) ?? "x";

        return MarkupExtensionNames
            .Concat(new[] { xamlPrefix + ":Bind", xamlPrefix + ":Null" })
            .Select(name => new CompletionItem
            {
                Label = name,
                Kind = CompletionItemKind.Snippet,
                Detail = "XAML markup extension",
                Documentation = CreatePlainDocumentation("XAML markup extension `{" + name + " ...}`."),
                InsertText = includeBraces ? "{" + name + " $0}" : name + " $0}",
                InsertTextFormat = InsertTextFormat.Snippet
            }).ToArray();
    }

    private static CompletionItem[] CreateXmlNamespaceValueItems()
    {
        return new[]
        {
            new CompletionItem
            {
                Label = PresentationNamespace,
                Kind = CompletionItemKind.Namespace,
                Detail = "WinUI XAML presentation namespace",
                Documentation = CreatePlainDocumentation("WinUI XAML presentation namespace for controls, panels, media, animation, and related framework types."),
                InsertTextFormat = InsertTextFormat.Plaintext
            },
            new CompletionItem
            {
                Label = DirectUiNamespace,
                Kind = CompletionItemKind.Namespace,
                Detail = "DirectUI XAML namespace",
                Documentation = CreatePlainDocumentation("DirectUI XAML namespace accepted by the WinUI XAML compiler."),
                InsertTextFormat = InsertTextFormat.Plaintext
            },
            new CompletionItem
            {
                Label = XamlLanguageNamespace,
                Kind = CompletionItemKind.Namespace,
                Detail = "XAML language namespace",
                Documentation = CreatePlainDocumentation("XAML language namespace for directives such as `x:Class`, `x:Name`, and `x:Bind`."),
                InsertTextFormat = InsertTextFormat.Plaintext
            },
            new CompletionItem
            {
                Label = "using:",
                Kind = CompletionItemKind.Namespace,
                Detail = "Project namespace mapping",
                Documentation = CreatePlainDocumentation("Maps an XML namespace prefix to a projected CLR namespace using WinUI `using:` syntax."),
                InsertTextFormat = InsertTextFormat.Plaintext
            },
            new CompletionItem
            {
                Label = "clr-namespace:",
                Kind = CompletionItemKind.Namespace,
                Detail = "CLR namespace mapping",
                Documentation = CreatePlainDocumentation("Maps an XML namespace prefix to a CLR namespace and optional assembly."),
                InsertTextFormat = InsertTextFormat.Plaintext
            }
        };
    }

    private MarkupContent? CreateTypeDocumentation(XamlType type)
    {
        string signature = GetTypeDisplayName(type);
        string? fullName = TryGetTypeFullName(type);
        XamlDocumentationEntry? documentation = null;
        if (fullName != null)
        {
            _documentation.TryGetEntry("T:" + fullName, out documentation);
        }

        return CreateDocumentation(signature, documentation);
    }

    private MarkupContent? CreateMemberDocumentation(XamlMember member)
    {
        XamlDocumentationEntry? documentation = null;
        foreach (string memberId in GetMemberDocumentationIds(member))
        {
            if (_documentation.TryGetEntry(memberId, out documentation))
                break;
        }

        return CreateDocumentation(GetMemberDetail(member), documentation);
    }

    private MarkupContent? CreateEnumValueDocumentation(XamlType enumType, string enumName)
    {
        string signature = GetTypeDisplayName(enumType) + "." + enumName;
        XamlDocumentationEntry? documentation = null;
        string? fullName = TryGetTypeFullName(enumType);
        if (fullName != null)
        {
            _documentation.TryGetEntry("F:" + fullName + "." + enumName, out documentation);
        }

        return CreateDocumentation(signature, documentation);
    }

    private static MarkupContent CreatePlainDocumentation(string value)
    {
        return new MarkupContent
        {
            Kind = MarkupKind.Markdown,
            Value = value
        };
    }

    private static MarkupContent? CreateDocumentation(string signature, XamlDocumentationEntry? documentation)
    {
        string markdown = BuildMarkdown(signature, documentation);
        if (string.IsNullOrWhiteSpace(markdown))
            return null;

        return new MarkupContent
        {
            Kind = MarkupKind.Markdown,
            Value = markdown
        };
    }

    private static string BuildMarkdown(string signature, XamlDocumentationEntry? documentation)
    {
        var builder = new StringBuilder();

        builder.AppendLine("```xaml");
        builder.AppendLine(signature);
        builder.AppendLine("```");

        if (documentation == null)
            return builder.ToString().TrimEnd();

        AppendParagraph(builder, documentation.Summary);

        if (documentation.Parameters.Count > 0)
        {
            builder.AppendLine();
            builder.AppendLine("**Parameters**");
            foreach (KeyValuePair<string, string> parameter in documentation.Parameters)
            {
                builder.Append("- `");
                builder.Append(parameter.Key);
                builder.Append("`: ");
                builder.AppendLine(parameter.Value);
            }
        }

        AppendSection(builder, "Returns", documentation.Returns);
        AppendSection(builder, "Remarks", documentation.Remarks);

        return builder.ToString().TrimEnd();
    }

    private static void AppendParagraph(StringBuilder builder, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        builder.AppendLine();
        builder.AppendLine(value);
    }

    private static void AppendSection(StringBuilder builder, string title, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        builder.AppendLine();
        builder.Append("**");
        builder.Append(title);
        builder.AppendLine("**");
        builder.AppendLine(value);
    }

    private static IEnumerable<string> GetMemberDocumentationIds(XamlMember member)
    {
        string? declaringTypeName = TryGetTypeFullName(member.DeclaringType);
        if (declaringTypeName == null)
            yield break;

        string baseId = declaringTypeName + "." + member.Name;

        yield return "P:" + baseId;
        yield return "E:" + baseId;
        yield return "F:" + baseId;
    }

    private static CompletionItem[] ApplyReplacementEdits(IEnumerable<CompletionItem> items, Range replacementRange)
    {
        return items.Select(item => new CompletionItem
        {
            Label = item.Label,
            Kind = item.Kind,
            Detail = item.Detail,
            Documentation = item.Documentation,
            SortText = item.SortText,
            FilterText = item.FilterText,
            InsertText = item.InsertText,
            InsertTextFormat = item.InsertTextFormat,
            TextEdit = new TextEdit
            {
                Range = replacementRange,
                NewText = item.InsertText ?? item.Label
            }
        }).ToArray();
    }

    private static CompletionItem CreateKeywordItem(string label, string detail)
    {
        return new CompletionItem
        {
            Label = label,
            Kind = CompletionItemKind.Keyword,
            Detail = detail,
            InsertTextFormat = InsertTextFormat.Plaintext
        };
    }

    private static XamlType? ResolveContextType(
        XamlSchemaContext schema,
        XamlDomObject? contextObject,
        CompletionContextInfo context)
    {
        if (contextObject?.Type != null)
            return contextObject.Type;

        return TryResolveElementType(contextObject, context, schema, context.ElementName);
    }

    private static XamlType? TryResolveElementType(
        XamlDomObject? contextObject,
        CompletionContextInfo context,
        XamlSchemaContext schema,
        string? elementName)
    {
        if (elementName == null || string.IsNullOrWhiteSpace(elementName))
            return null;

        string qualifiedElementName = elementName;

        if (contextObject != null)
        {
            try
            {
                XamlType? resolved = contextObject.ResolveXmlName(qualifiedElementName);
                if (resolved != null)
                    return resolved;
            }
            catch (Exception ex) when (IsXamlMetadataException(ex))
            {
                // 文本正在编辑时 DOM 命名空间解析可能失败，继续使用文本中 xmlns 声明作为备用方案。
            }
        }

        SplitQualifiedName(qualifiedElementName, out string prefix, out string name);
        string? xamlNamespace = context.GetNamespaceForPrefix(prefix);
        if (xamlNamespace == null)
            return null;

        try
        {
            return schema.GetXamlType(new XamlTypeName(xamlNamespace, name));
        }
        catch (Exception ex) when (IsXamlMetadataException(ex))
        {
            return null;
        }
    }

    private static XamlMember? TryResolveMember(
        XamlSchemaContext schema,
        XamlDomObject? contextObject,
        CompletionContextInfo context,
        XamlType contextType,
        string attributeName)
    {
        if (string.IsNullOrWhiteSpace(attributeName) || IsXmlNamespaceAttribute(attributeName))
            return null;

        string localName = StripXmlPrefix(attributeName);
        int dotIndex = localName.IndexOf('.');
        if (dotIndex > 0 && dotIndex < localName.Length - 1)
        {
            string ownerName = localName.Substring(0, dotIndex);
            string memberName = localName.Substring(dotIndex + 1);
            XamlType? ownerType = TryResolveElementType(contextObject, context, schema, ownerName);
            return ownerType == null
                ? null
                : TryGetAttachableMember(ownerType, memberName) ?? TryGetMember(ownerType, memberName);
        }

        return TryGetMember(contextType, localName);
    }

    private static string? ResolveCompletionNamespace(
        XamlSchemaContext schema,
        XamlDomObject? contextObject,
        CompletionContextInfo context,
        bool preferElementPrefix)
    {
        string prefix = preferElementPrefix ? context.PrefixQualifier ?? string.Empty : string.Empty;
        if (contextObject != null)
        {
            try
            {
                string? resolved = contextObject.ResolveXmlPrefix(prefix);
                if (resolved != null)
                    return resolved;
            }
            catch (Exception ex) when (IsXamlMetadataException(ex))
            {
                // 继续使用文本扫描得到的 xmlns 映射。
            }
        }

        return context.GetNamespaceForPrefix(prefix) ?? PresentationNamespace;
    }

    private static IEnumerable<XamlMember>? TryGetAllMembers(XamlType type)
    {
        try
        {
            return type.GetAllMembers();
        }
        catch (Exception ex) when (IsXamlMetadataException(ex))
        {
            return null;
        }
    }

    private static IEnumerable<XamlMember>? TryGetAllAttachableMembers(XamlType type)
    {
        try
        {
            return type.GetAllAttachableMembers();
        }
        catch (Exception ex) when (IsXamlMetadataException(ex))
        {
            return null;
        }
    }

    private static ICollection<XamlType>? TryGetAllXamlTypes(XamlSchemaContext schema, string xamlNamespace)
    {
        try
        {
            return schema.GetAllXamlTypes(xamlNamespace);
        }
        catch (Exception ex) when (IsXamlMetadataException(ex) || ex is NotImplementedException)
        {
            return null;
        }
    }

    private static XamlMember? TryGetMember(XamlType type, string name)
    {
        try
        {
            return type.GetMember(name);
        }
        catch (Exception ex) when (IsXamlMetadataException(ex))
        {
            return null;
        }
    }

    private static XamlMember? TryGetAttachableMember(XamlType type, string name)
    {
        try
        {
            return type.GetAttachableMember(name);
        }
        catch (Exception ex) when (IsXamlMetadataException(ex))
        {
            return null;
        }
    }

    private static bool IsCompletableType(XamlType type)
    {
        try
        {
            return type.IsPublic && type.IsConstructible && !string.IsNullOrWhiteSpace(type.Name);
        }
        catch (Exception ex) when (IsXamlMetadataException(ex))
        {
            return false;
        }
    }

    private static bool IsCompletableMember(XamlMember member)
    {
        try
        {
            if (member.IsDirective || string.IsNullOrWhiteSpace(member.Name))
                return false;

            if (member.IsEvent)
                return true;

            return !member.IsReadOnly && member.IsWritePublic;
        }
        catch (Exception ex) when (IsXamlMetadataException(ex))
        {
            return false;
        }
    }

    private static CompletionItem[] DeduplicateAndFilter(IEnumerable<CompletionItem> items, string prefix)
    {
        return FilterByPrefix(
            items.GroupBy(item => item.Label, StringComparer.Ordinal)
                .Select(group => group.First())
                .OrderBy(item => item.Label, StringComparer.Ordinal)
                .ToArray(),
            prefix);
    }

    private static CompletionItem[] FilterByPrefix(IEnumerable<CompletionItem> items, string prefix)
    {
        if (string.IsNullOrEmpty(prefix))
            return items.ToArray();

        return items
            .Where(item => item.Label.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToArray();
    }

    private static CompletionList CreateList(CompletionItem[] items)
    {
        return new CompletionList
        {
            IsIncomplete = false,
            Items = items
        };
    }

    private static XamlDomObject? FindInnermostObject(XamlDomObject root, int line, int character)
    {
        if (!Contains(root, line, character))
            return null;

        IList<XamlDomMember> members = root.MemberNodes;
        for (int memberIndex = 0; memberIndex < members.Count; memberIndex++)
        {
            IList<XamlDomItem> items = members[memberIndex].Items;
            for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
            {
                if (items[itemIndex] is XamlDomObject child)
                {
                    XamlDomObject? found = FindInnermostObject(child, line, character);
                    if (found != null)
                        return found;
                }
            }
        }

        return root;
    }

    private static bool Contains(XamlDomNode node, int line, int character)
    {
        if (node.StartLineNumber <= 0)
            return false;

        bool startsBefore = line > node.StartLineNumber ||
            (line == node.StartLineNumber && character >= Math.Max(1, node.StartLinePosition));
        if (!startsBefore)
            return false;

        if (node.EndLineNumber <= 0)
            return true;

        return line < node.EndLineNumber ||
            (line == node.EndLineNumber && character <= Math.Max(1, node.EndLinePosition));
    }

    private static string GetMemberDetail(XamlMember member)
    {
        string? owner = TryGetTypeFullName(member.DeclaringType) ?? member.DeclaringType?.Name;
        string? memberType = TryGetTypeFullName(member.Type) ?? member.Type?.Name;
        string prefix = member.IsEvent ? "event" : "property";

        if (owner == null && memberType == null)
            return prefix;
        if (memberType == null)
            return prefix + " " + owner;
        if (owner == null)
            return prefix + " : " + memberType;
        return prefix + " " + owner + "." + member.Name + " : " + memberType;
    }

    private static string GetTypeDisplayName(XamlType type)
    {
        return TryGetTypeFullName(type) ?? type.Name;
    }

    private static string? TryGetTypeFullName(XamlType? type)
    {
        if (type == null)
            return null;

        try
        {
            return type.UnderlyingType?.FullName;
        }
        catch (Exception ex) when (IsXamlMetadataException(ex))
        {
            return null;
        }
    }

    private static bool IsXmlNamespaceAttribute(string attributeName)
    {
        return attributeName.Equals("xmlns", StringComparison.Ordinal) ||
            attributeName.StartsWith("xmlns:", StringComparison.Ordinal);
    }

    private static string StripXmlPrefix(string name)
    {
        int colonIndex = name.IndexOf(':');
        return colonIndex >= 0 && colonIndex < name.Length - 1
            ? name.Substring(colonIndex + 1)
            : name;
    }

    private static void SplitQualifiedName(string qualifiedName, out string prefix, out string name)
    {
        prefix = string.Empty;
        name = qualifiedName;
        int colonIndex = qualifiedName.IndexOf(':');
        if (colonIndex >= 0)
        {
            prefix = qualifiedName.Substring(0, colonIndex);
            name = qualifiedName.Substring(colonIndex + 1);
        }
    }

    private static bool IsXamlMetadataException(Exception exception)
    {
        return exception is XamlException ||
            exception is InvalidOperationException ||
            exception is TypeLoadException ||
            exception is ArgumentException ||
            exception is NullReferenceException;
    }

    private enum CompletionContextKind
    {
        ElementName,
        AttributeName,
        AttributeValue
    }

    private sealed class CompletionContextInfo
    {
        private static readonly Regex NamespaceRegex = new(
            @"\bxmlns(?::(?<prefix>[A-Za-z_][\w\-.]*))?\s*=\s*['""](?<namespace>[^'""]+)",
            RegexOptions.Compiled);

        private CompletionContextInfo(
            CompletionContextKind kind,
            string elementName,
            string replacementPrefix,
            string attributeName,
            string valuePrefix,
            bool isMarkupExtensionValue,
            Position position,
            IReadOnlyDictionary<string, string> namespaceMap,
            ISet<string> existingAttributes)
        {
            Kind = kind;
            ElementName = elementName;
            ReplacementPrefix = replacementPrefix;
            AttributeName = attributeName;
            ValuePrefix = valuePrefix;
            IsMarkupExtensionValue = isMarkupExtensionValue;
            NamespaceMap = namespaceMap;
            ExistingAttributes = existingAttributes;
            ReplacementRange = CreateReplacementRange(kind, replacementPrefix, valuePrefix, isMarkupExtensionValue, position);

            SplitQualifiedName(replacementPrefix, out string prefixQualifier, out _);
            PrefixQualifier = replacementPrefix.IndexOf(':') >= 0 ? prefixQualifier : null;
            AttachableOwnerPrefix = TryGetAttachableOwnerPrefix(replacementPrefix);
            MarkupExtensionPrefix = isMarkupExtensionValue && valuePrefix.StartsWith("{", StringComparison.Ordinal)
                ? valuePrefix.Substring(1)
                : valuePrefix;
        }

        public CompletionContextKind Kind { get; }

        public string ElementName { get; }

        public string ReplacementPrefix { get; }

        public string AttributeName { get; }

        public string ValuePrefix { get; }

        public bool IsMarkupExtensionValue { get; }

        public string MarkupExtensionPrefix { get; }

        public Range ReplacementRange { get; }

        public string? PrefixQualifier { get; }

        public string? AttachableOwnerPrefix { get; }

        public IReadOnlyDictionary<string, string> NamespaceMap { get; }

        public ISet<string> ExistingAttributes { get; }

        public string? GetNamespaceForPrefix(string prefix)
        {
            return NamespaceMap.TryGetValue(prefix, out string? xamlNamespace) ? xamlNamespace : null;
        }

        public string? GetPrefixForNamespace(string xamlNamespace)
        {
            foreach (KeyValuePair<string, string> pair in NamespaceMap)
            {
                if (pair.Value.Equals(xamlNamespace, StringComparison.Ordinal))
                    return pair.Key.Length == 0 ? null : pair.Key;
            }

            return null;
        }

        public static CompletionContextInfo? TryCreate(string text, Position position)
        {
            string[] lines = GetLines(text);
            int offset = GetOffset(lines, position);
            if (offset < 0)
                return null;

            TagSnapshot? tag = FindOpenTag(text, offset);
            if (tag == null || tag.IsClosingTag)
                return null;

            string textBeforeCursor = text.Substring(0, offset);
            IReadOnlyDictionary<string, string> namespaceMap = BuildNamespaceMap(textBeforeCursor);
            ISet<string> existingAttributes = GetExistingAttributes(tag.TextBeforeCursor);
            string elementName = ReadElementName(tag.TextBeforeCursor);

            AttributeValueSnapshot? value = FindAttributeValue(tag.TextBeforeCursor);
            if (value != null)
            {
                return new CompletionContextInfo(
                    CompletionContextKind.AttributeValue,
                    elementName,
                    value.ValuePrefix,
                    value.AttributeName,
                    value.ValuePrefix,
                    value.ValuePrefix.StartsWith("{", StringComparison.Ordinal),
                    position,
                    namespaceMap,
                    existingAttributes);
            }

            if (IsInElementName(tag.TextBeforeCursor))
            {
                string replacementPrefix = ReadCurrentToken(tag.TextBeforeCursor);
                return new CompletionContextInfo(
                    CompletionContextKind.ElementName,
                    elementName,
                    replacementPrefix,
                    string.Empty,
                    string.Empty,
                    isMarkupExtensionValue: false,
                    position,
                    namespaceMap,
                    existingAttributes);
            }

            string attributePrefix = ReadCurrentToken(tag.TextBeforeCursor);
            return new CompletionContextInfo(
                CompletionContextKind.AttributeName,
                elementName,
                attributePrefix,
                string.Empty,
                string.Empty,
                isMarkupExtensionValue: false,
                position,
                namespaceMap,
                existingAttributes);
        }

        private static Range CreateReplacementRange(
            CompletionContextKind kind,
            string replacementPrefix,
            string valuePrefix,
            bool isMarkupExtensionValue,
            Position position)
        {
            int prefixLength = kind == CompletionContextKind.AttributeValue ? valuePrefix.Length : replacementPrefix.Length;
            if (kind == CompletionContextKind.AttributeValue && isMarkupExtensionValue && valuePrefix.StartsWith("{", StringComparison.Ordinal))
                prefixLength = Math.Max(0, valuePrefix.Length - 1);

            return new Range
            {
                Start = new Position
                {
                    Line = position.Line,
                    Character = Math.Max(0, position.Character - prefixLength)
                },
                End = new Position
                {
                    Line = position.Line,
                    Character = position.Character
                }
            };
        }

        private static string[] GetLines(string text)
        {
            return text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        private static int GetOffset(string[] lines, Position position)
        {
            if (position.Line < 0 || position.Line >= lines.Length || position.Character < 0)
                return -1;

            int offset = 0;
            for (int line = 0; line < position.Line; line++)
            {
                offset += lines[line].Length + 1;
            }

            return offset + Math.Min(position.Character, lines[position.Line].Length);
        }

        private static TagSnapshot? FindOpenTag(string text, int offset)
        {
            bool inQuote = false;
            char quote = '\0';
            int tagStart = -1;

            for (int index = 0; index < offset; index++)
            {
                char value = text[index];
                if (inQuote)
                {
                    if (value == quote)
                        inQuote = false;
                    continue;
                }

                if (value == '"' || value == '\'')
                {
                    inQuote = true;
                    quote = value;
                }
                else if (value == '<')
                {
                    tagStart = index;
                }
                else if (value == '>')
                {
                    tagStart = -1;
                }
            }

            if (tagStart < 0)
                return null;

            string textBeforeCursor = text.Substring(tagStart, offset - tagStart);
            bool isClosingTag = textBeforeCursor.Length > 1 && textBeforeCursor[1] == '/';
            return new TagSnapshot(textBeforeCursor, isClosingTag);
        }

        private static AttributeValueSnapshot? FindAttributeValue(string tagText)
        {
            bool inQuote = false;
            char quote = '\0';
            int quoteStart = -1;

            for (int index = 0; index < tagText.Length; index++)
            {
                char value = tagText[index];
                if (inQuote)
                {
                    if (value == quote)
                    {
                        inQuote = false;
                        quoteStart = -1;
                    }
                    continue;
                }

                if (value == '"' || value == '\'')
                {
                    inQuote = true;
                    quote = value;
                    quoteStart = index;
                }
            }

            if (!inQuote || quoteStart < 0)
                return null;

            string attributeName = ReadAttributeNameBeforeQuote(tagText, quoteStart);
            if (attributeName.Length == 0)
                return null;

            string valuePrefix = tagText.Substring(quoteStart + 1);
            return new AttributeValueSnapshot(attributeName, valuePrefix);
        }

        private static string ReadAttributeNameBeforeQuote(string tagText, int quoteStart)
        {
            int index = quoteStart - 1;
            while (index >= 0 && char.IsWhiteSpace(tagText[index]))
                index--;

            if (index < 0 || tagText[index] != '=')
                return string.Empty;

            index--;
            while (index >= 0 && char.IsWhiteSpace(tagText[index]))
                index--;

            int end = index + 1;
            while (index >= 0 && IsNameCharacter(tagText[index]))
                index--;

            return tagText.Substring(index + 1, end - index - 1);
        }

        private static bool IsInElementName(string tagText)
        {
            int index = tagText.StartsWith("</", StringComparison.Ordinal) ? 2 : 1;
            while (index < tagText.Length && char.IsWhiteSpace(tagText[index]))
                index++;

            while (index < tagText.Length && IsNameCharacter(tagText[index]))
                index++;

            return index == tagText.Length;
        }

        private static string ReadElementName(string tagText)
        {
            int index = tagText.StartsWith("</", StringComparison.Ordinal) ? 2 : 1;
            while (index < tagText.Length && char.IsWhiteSpace(tagText[index]))
                index++;

            int start = index;
            while (index < tagText.Length && IsNameCharacter(tagText[index]))
                index++;

            return tagText.Substring(start, index - start);
        }

        private static string ReadCurrentToken(string tagText)
        {
            int index = tagText.Length - 1;
            while (index >= 0 && IsNameCharacter(tagText[index]))
                index--;

            return tagText.Substring(index + 1);
        }

        private static IReadOnlyDictionary<string, string> BuildNamespaceMap(string textBeforeCursor)
        {
            var namespaces = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [string.Empty] = PresentationNamespace,
                ["x"] = XamlLanguageNamespace
            };

            foreach (Match match in NamespaceRegex.Matches(textBeforeCursor))
            {
                string prefix = match.Groups["prefix"].Success ? match.Groups["prefix"].Value : string.Empty;
                string xamlNamespace = match.Groups["namespace"].Value;
                if (!string.IsNullOrWhiteSpace(xamlNamespace))
                    namespaces[prefix] = xamlNamespace;
            }

            return namespaces;
        }

        private static ISet<string> GetExistingAttributes(string tagText)
        {
            var attributes = new HashSet<string>(StringComparer.Ordinal);
            foreach (Match match in Regex.Matches(tagText, @"(?<!<)(?<name>[A-Za-z_][\w\-.:]*)\s*="))
            {
                attributes.Add(match.Groups["name"].Value);
            }

            return attributes;
        }

        private static string? TryGetAttachableOwnerPrefix(string replacementPrefix)
        {
            int dotIndex = replacementPrefix.IndexOf('.');
            return dotIndex > 0 ? replacementPrefix.Substring(0, dotIndex) : null;
        }

        private static bool IsNameCharacter(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_' || value == ':' || value == '.' || value == '-';
        }

        private sealed class TagSnapshot
        {
            public TagSnapshot(string textBeforeCursor, bool isClosingTag)
            {
                TextBeforeCursor = textBeforeCursor;
                IsClosingTag = isClosingTag;
            }

            public string TextBeforeCursor { get; }

            public bool IsClosingTag { get; }
        }

        private sealed class AttributeValueSnapshot
        {
            public AttributeValueSnapshot(string attributeName, string valuePrefix)
            {
                AttributeName = attributeName;
                ValuePrefix = valuePrefix;
            }

            public string AttributeName { get; }

            public string ValuePrefix { get; }
        }
    }
}
