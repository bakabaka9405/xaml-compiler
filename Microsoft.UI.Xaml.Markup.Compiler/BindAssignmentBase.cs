using System;
using System.Collections.Generic;
using System.Xaml;
using Microsoft.UI.Xaml.Markup.Compiler.CodeGen;
using Microsoft.UI.Xaml.Markup.Compiler.Properties;
using Microsoft.UI.Xaml.Markup.Compiler.Utilities;
using Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class BindAssignmentBase : ILineNumberAndErrorInfo, IXamlTypeResolver
{
	protected XamlDomObject bindItem;

	protected XamlDomMember bindMember;

	public ApiInformation ApiInformation { get; }

	public BindUniverse BindUniverse { get; }

	public ConnectionIdElement ConnectionIdElement { get; }

	public BindPathStep PathStep { get; protected set; }

	public LineNumberInfo LineNumberInfo { get; }

	public virtual LanguageSpecificString ObjectDeferredAssignmentCodeName => new LanguageSpecificString(() => $"{ConnectionIdElement.ObjectCodeName}{MemberName}DeferredValue");

	public string MemberName => bindMember.Member.Name;

	public virtual string MemberFullName => $"{MemberDeclaringType.UnderlyingType.FullName}.{MemberName}";

	public virtual XamlType MemberType => bindMember.Member.Type;

	public virtual XamlType MemberDeclaringType => bindMember.Member.DeclaringType;

	public virtual bool IsAttachable => bindMember.Member.IsAttachable;

	public virtual bool IsInputPropertyAssignment
	{
		get
		{
			string text = bindMember.Parent.Type.TryGetInputPropertyName();
			return text == MemberName;
		}
	}

	public virtual bool HasSetValueHelper => true;

	public virtual bool HasDeferredValueProxy => true;

	public virtual int LineNumber => bindMember.StartLineNumber;

	public virtual int ColumnNumber => bindMember.StartLinePosition;

	public BindAssignmentBase(XamlDomMember domMember, BindUniverse bindUniverse, ConnectionIdElement connectionIdElement)
	{
		bindMember = domMember;
		ApiInformation = domMember.ApiInformation ?? domMember.Parent.ApiInformation;
		LineNumberInfo = new LineNumberInfo(domMember);
		bindItem = domMember.Item as XamlDomObject;
		BindUniverse = bindUniverse;
		ConnectionIdElement = connectionIdElement;
		if (!bindUniverse.IsFileRoot && !bindUniverse.BoundElements.Contains(bindUniverse.RootElement))
		{
			bindUniverse.BoundElements.Add(bindUniverse.RootElement);
		}
		if (!bindUniverse.BoundElements.Contains(connectionIdElement))
		{
			bindUniverse.BoundElements.Add(connectionIdElement);
		}
	}

	public XamlCompileError GetAttributeProcessingError()
	{
		return new XamlRewriterErrorCompiledBindingLongForm(LineNumberInfo.StartLineNumber, LineNumberInfo.StartLinePosition);
	}

	public BindPathStep ParseBindPath(IList<string> warnings)
	{
		string bindingPath = GetBindingPath(bindItem);
		return ParseBindPath(bindingPath, warnings);
	}

	public BindPathStep ParseBindPath(string path, IList<string> warnings)
	{
		if (path.Length == 0)
		{
			return BindUniverse.RootStep;
		}
		ApiInformation apiInformation = bindItem.Parent.ApiInformation;
		try
		{
			return BindPathStep.Parse(path, apiInformation, BindUniverse, this, warnings);
		}
		catch (ParseException ex)
		{
			throw new CompiledBindingParseException(path, ex.Message, ColumnNumber);
		}
	}

	protected static string GetBindingPath(XamlDomObject bindItem)
	{
		string implicitPath = GetImplicitPath(bindItem);
		string stringValueOfProperty = DomHelper.GetStringValueOfProperty(bindItem.GetMemberNode("Path"));
		if (!string.IsNullOrEmpty(implicitPath) && !string.IsNullOrEmpty(stringValueOfProperty))
		{
			throw new CompiledBindingParseException(stringValueOfProperty, ResourceUtilities.FormatString(XamlCompilerResources.BindPathParser_PathSetTwice), bindItem.StartLinePosition);
		}
		if (string.IsNullOrEmpty(stringValueOfProperty))
		{
			if (string.IsNullOrEmpty(implicitPath))
			{
				return string.Empty;
			}
			return implicitPath;
		}
		return stringValueOfProperty;
	}

	private static string GetImplicitPath(XamlDomObject bindItem)
	{
		XamlDomMember memberNode = bindItem.GetMemberNode(XamlLanguage.PositionalParameters);
		if (memberNode != null)
		{
			if (memberNode.Items.Count == 0)
			{
				return string.Empty;
			}
			if (memberNode.Items.Count == 1)
			{
				if (memberNode.Items[0] is XamlDomValue xamlDomValue)
				{
					return xamlDomValue.Value as string;
				}
			}
			else if (memberNode.Items.Count > 1)
			{
				throw new CompiledBindingParseException(string.Empty, ResourceUtilities.FormatString(XamlCompilerResources.BindAssignment_InvalidPropertyPathSyntax), bindItem.StartLinePosition);
			}
		}
		return null;
	}

	public XamlType ResolveXmlName(string name)
	{
		return bindItem.ResolveXmlName(name);
	}

	public XamlType ResolveType(Type type)
	{
		return bindItem.SchemaContext.GetXamlType(type);
	}

	public bool CanAssignDirectlyTo(XamlType source, XamlType destination)
	{
		return source.CanAssignDirectlyTo(destination);
	}

	public bool CanInlineConvert(XamlType source, XamlType destination)
	{
		return source.CanInlineConvert(destination);
	}
}
