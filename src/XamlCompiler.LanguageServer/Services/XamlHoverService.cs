using System;
using System.Collections.Generic;
using System.Text;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace XamlCompiler.LanguageServer.Services;

/// <summary>
/// 基于当前文档文本、XAML DOM 与 XML 文档索引生成 LSP hover 内容。
/// </summary>
public sealed class XamlHoverService
{
    private readonly XamlDocumentStore _documentStore;
    private readonly XamlParserService _parser;
    private readonly XamlDocumentationService _documentation;

    public XamlHoverService(
        XamlDocumentStore documentStore,
        XamlParserService parser,
        XamlDocumentationService documentation)
    {
        _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _documentation = documentation ?? throw new ArgumentNullException(nameof(documentation));
    }

    /// <summary>
    /// 生成 textDocument/hover 响应；无法定位符号时返回 null。
    /// </summary>
    public Hover? GetHover(TextDocumentPositionParams parameters, XamlSchemaContext schema)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        if (schema == null)
            throw new ArgumentNullException(nameof(schema));

        string uri = parameters.TextDocument.Uri.AbsoluteUri;
        string? text = _documentStore.GetText(uri);
        if (text == null || text.Length == 0)
            return null;

        HoverToken? token = HoverToken.TryCreate(text, parameters.Position);
        if (token == null)
            return null;

        XamlParseResult parseResult = _parser.Parse(text, uri, schema);
        if (parseResult.DomRoot == null)
            return null;

        XamlDomObject? contextObject = FindInnermostObject(
            parseResult.DomRoot,
            parameters.Position.Line + 1,
            parameters.Position.Character + 1);
        if (contextObject == null)
            return null;

        string? markdown = token.Kind == HoverTokenKind.Element
            ? CreateTypeHover(contextObject.Type)
            : CreateMemberHover(contextObject, token.Name);

        if (markdown == null || string.IsNullOrWhiteSpace(markdown))
            return null;

        string hoverMarkdown = markdown;

        return new Hover
        {
            Contents = new MarkupContent
            {
                Kind = MarkupKind.Markdown,
                Value = hoverMarkdown
            },
            Range = new Range
            {
                Start = new Position { Line = parameters.Position.Line, Character = token.StartCharacter },
                End = new Position { Line = parameters.Position.Line, Character = token.EndCharacter }
            }
        };
    }

    private string? CreateTypeHover(XamlType? type)
    {
        if (type == null)
            return null;

        string displayName = GetTypeDisplayName(type);
        string? fullName = TryGetTypeFullName(type);
        XamlDocumentationEntry? documentation = null;
        if (fullName != null)
        {
            _documentation.TryGetEntry("T:" + fullName, out documentation);
        }

        return BuildMarkdown(displayName, documentation);
    }

    private string? CreateMemberHover(XamlDomObject contextObject, string attributeName)
    {
        if (contextObject.Type == null || IsXmlNamespaceAttribute(attributeName))
            return null;

        XamlMember? member = TryResolveMember(contextObject, attributeName);
        if (member == null)
            return null;

        string displayName = GetMemberDisplayName(member);
        XamlDocumentationEntry? documentation = null;
        foreach (string memberId in GetMemberDocumentationIds(member))
        {
            if (_documentation.TryGetEntry(memberId, out documentation))
                break;
        }

        return BuildMarkdown(displayName, documentation);
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

    private static XamlMember? TryResolveMember(XamlDomObject contextObject, string attributeName)
    {
        string localName = StripXmlPrefix(attributeName);

        // Phase 1: 常规实例属性，例如 Width、Background。
        XamlMember? member = TryGetMember(contextObject.Type, localName);
        if (member != null)
            return member;

        // Phase 2: 附加属性，例如 Grid.Row。
        int dotIndex = localName.IndexOf('.');
        if (dotIndex > 0 && dotIndex < localName.Length - 1)
        {
            string ownerName = localName.Substring(0, dotIndex);
            string memberName = localName.Substring(dotIndex + 1);
            XamlType? ownerType = TryResolveXmlName(contextObject, ownerName);
            if (ownerType != null)
            {
                member = TryGetAttachableMember(ownerType, memberName) ?? TryGetMember(ownerType, memberName);
                if (member != null)
                    return member;
            }
        }

        return null;
    }

    private static XamlType? TryResolveXmlName(XamlDomObject contextObject, string name)
    {
        try
        {
            return contextObject.ResolveXmlName(name);
        }
        catch (Exception ex) when (ex is XamlException || ex is InvalidOperationException || ex is TypeLoadException)
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
        catch (Exception ex) when (ex is XamlException || ex is InvalidOperationException || ex is TypeLoadException)
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
        catch (Exception ex) when (ex is XamlException || ex is InvalidOperationException || ex is TypeLoadException)
        {
            return null;
        }
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

    private static string GetTypeDisplayName(XamlType type)
    {
        string? fullName = TryGetTypeFullName(type);
        return fullName ?? type.Name;
    }

    private static string GetMemberDisplayName(XamlMember member)
    {
        string? declaringTypeName = TryGetTypeFullName(member.DeclaringType);
        string owner = declaringTypeName ?? member.DeclaringType?.Name ?? "<unknown>";
        string? memberType = TryGetTypeFullName(member.Type) ?? member.Type?.Name;
        return memberType == null
            ? owner + "." + member.Name
            : owner + "." + member.Name + " : " + memberType;
    }

    private static string? TryGetTypeFullName(XamlType? type)
    {
        if (type == null)
            return null;

        try
        {
            return type.UnderlyingType?.FullName;
        }
        catch (Exception ex) when (ex is XamlException || ex is InvalidOperationException || ex is TypeLoadException)
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

    private enum HoverTokenKind
    {
        Element,
        Attribute
    }

    private sealed class HoverToken
    {
        private HoverToken(string name, int startCharacter, int endCharacter, HoverTokenKind kind)
        {
            Name = name;
            StartCharacter = startCharacter;
            EndCharacter = endCharacter;
            Kind = kind;
        }

        public string Name { get; }

        public int StartCharacter { get; }

        public int EndCharacter { get; }

        public HoverTokenKind Kind { get; }

        public static HoverToken? TryCreate(string text, Position position)
        {
            string[] lines = GetLines(text);
            string? line = GetLine(lines, position.Line);
            if (line == null)
                return null;

            if (!TryGetTokenBounds(line, position.Character, out int start, out int end))
                return null;

            string name = line.Substring(start, end - start);
            if (string.IsNullOrWhiteSpace(name))
                return null;

            TagContext? tagContext = FindTagContext(lines, position.Line, start);
            if (tagContext == null)
                return null;

            if (tagContext.IsClosingTag && name.StartsWith("/", StringComparison.Ordinal))
                name = name.Substring(1);

            if (name.StartsWith("?", StringComparison.Ordinal) || name.StartsWith("!", StringComparison.Ordinal))
                return null;

            HoverTokenKind kind = tagContext.OpenLine == position.Line &&
                IsFirstTokenAfterOpenAngle(line, tagContext.OpenCharacter, start)
                ? HoverTokenKind.Element
                : HoverTokenKind.Attribute;

            return new HoverToken(name, start, end, kind);
        }

        private static string[] GetLines(string text)
        {
            return text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        }

        private static string? GetLine(string[] lines, int zeroBasedLine)
        {
            if (zeroBasedLine < 0)
                return null;

            return zeroBasedLine < lines.Length ? lines[zeroBasedLine] : null;
        }

        private static TagContext? FindTagContext(string[] lines, int lineIndex, int character)
        {
            bool seenCloseAngle = false;

            for (int currentLine = lineIndex; currentLine >= 0; currentLine--)
            {
                string line = lines[currentLine];
                int start = currentLine == lineIndex ? Math.Min(character, line.Length - 1) : line.Length - 1;
                for (int index = start; index >= 0; index--)
                {
                    char value = line[index];
                    if (value == '>')
                    {
                        seenCloseAngle = true;
                    }
                    else if (value == '<')
                    {
                        if (seenCloseAngle)
                            return null;

                        bool isClosingTag = index + 1 < line.Length && line[index + 1] == '/';
                        return new TagContext(currentLine, index, isClosingTag);
                    }
                }
            }

            return null;
        }

        private static bool IsFirstTokenAfterOpenAngle(string line, int openCharacter, int tokenStart)
        {
            if (tokenStart <= openCharacter || openCharacter < 0 || tokenStart > line.Length)
                return false;

            string prefix = line.Substring(openCharacter + 1, tokenStart - openCharacter - 1).Trim();
            return prefix.Length == 0 || prefix == "/";
        }

        private static bool TryGetTokenBounds(string line, int character, out int start, out int end)
        {
            start = 0;
            end = 0;

            if (line.Length == 0)
                return false;

            int index = Math.Min(Math.Max(0, character), line.Length - 1);
            if (!IsTokenCharacter(line[index]) && index > 0 && IsTokenCharacter(line[index - 1]))
                index--;

            if (!IsTokenCharacter(line[index]))
                return false;

            start = index;
            while (start > 0 && IsTokenCharacter(line[start - 1]))
            {
                start--;
            }

            end = index + 1;
            while (end < line.Length && IsTokenCharacter(line[end]))
            {
                end++;
            }

            return true;
        }

        private static bool IsTokenCharacter(char value)
        {
            return char.IsLetterOrDigit(value) || value == '_' || value == ':' || value == '.' || value == '/';
        }

        private sealed class TagContext
        {
            public TagContext(int openLine, int openCharacter, bool isClosingTag)
            {
                OpenLine = openLine;
                OpenCharacter = openCharacter;
                IsClosingTag = isClosingTag;
            }

            public int OpenLine { get; }

            public int OpenCharacter { get; }

            public bool IsClosingTag { get; }
        }
    }
}
