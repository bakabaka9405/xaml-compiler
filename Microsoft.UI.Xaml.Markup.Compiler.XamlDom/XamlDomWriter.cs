using System.Collections.Generic;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.XamlDom;

internal class XamlDomWriter : XamlWriter, IXamlLineInfoConsumer
{
	private string _sourceFilePath;

	private Stack<XamlDomNode> writerStack = new Stack<XamlDomNode>();

	private XamlSchemaContext _schemaContext;

	private int _lineNumber;

	private int _linePosition;

	private List<XamlDomNamespace> _namespaces;

	public XamlDomNode RootNode { get; set; }

	public override XamlSchemaContext SchemaContext => _schemaContext;

	bool IXamlLineInfoConsumer.ShouldProvideLineInfo => true;

	private XamlDomNode CurrentStackNode
	{
		get
		{
			if (writerStack.Count > 0)
			{
				return writerStack.Peek();
			}
			return null;
		}
	}

	public XamlDomWriter(XamlSchemaContext schemaContext, string sourceFilePath)
	{
		_schemaContext = schemaContext;
		_sourceFilePath = sourceFilePath;
	}

	public override void WriteGetObject()
	{
		WriteObject(null, isGetObject: true);
	}

	public override void WriteStartObject(XamlType type)
	{
		WriteObject(type, isGetObject: false);
	}

	private void WriteObject(XamlType xamlType, bool isGetObject)
	{
		XamlDomObject xamlDomObject = new XamlDomObject(isGetObject, xamlType, _sourceFilePath);
		xamlDomObject.IsGetObject = isGetObject;
		xamlDomObject.StartLinePosition = _linePosition;
		xamlDomObject.StartLineNumber = _lineNumber;
		if (xamlDomObject.IsGetObject || xamlDomObject.Type.SchemaContext == XamlLanguage.Object.SchemaContext)
		{
			xamlDomObject.SchemaContext = SchemaContext;
		}
		if (_namespaces != null)
		{
			foreach (XamlDomNamespace @namespace in _namespaces)
			{
				xamlDomObject.Namespaces.Add(@namespace);
			}
			_namespaces.Clear();
		}
		if (RootNode == null)
		{
			RootNode = xamlDomObject;
		}
		else
		{
			XamlDomMember xamlDomMember = (XamlDomMember)writerStack.Peek();
			xamlDomMember.Items.Add(xamlDomObject);
			xamlDomObject.Parent = xamlDomMember;
			if (isGetObject)
			{
				xamlDomObject.Type = xamlDomMember.Member.Type;
			}
		}
		writerStack.Push(xamlDomObject);
	}

	public override void WriteEndObject()
	{
		CurrentStackNode.EndLineNumber = _lineNumber;
		CurrentStackNode.EndLinePosition = _linePosition;
		writerStack.Pop();
	}

	public override void WriteStartMember(XamlMember xamlMember)
	{
		XamlDomMember xamlDomMember = new XamlDomMember(xamlMember, _sourceFilePath);
		if (xamlMember.IsDirective)
		{
			xamlDomMember.SchemaContext = SchemaContext;
		}
		if (RootNode != null)
		{
			XamlDomObject xamlDomObject = (XamlDomObject)writerStack.Peek();
			xamlDomObject.MemberNodes.Add(xamlDomMember);
		}
		else
		{
			RootNode = xamlDomMember;
		}
		xamlDomMember.StartLineNumber = _lineNumber;
		xamlDomMember.StartLinePosition = _linePosition;
		writerStack.Push(xamlDomMember);
	}

	public override void WriteEndMember()
	{
		CurrentStackNode.EndLineNumber = _lineNumber;
		CurrentStackNode.EndLinePosition = _linePosition;
		writerStack.Pop();
	}

	public override void WriteValue(object value)
	{
		XamlDomValue xamlDomValue = new XamlDomValue(_sourceFilePath);
		xamlDomValue.Value = value;
		if (RootNode != null)
		{
			XamlDomMember xamlDomMember = (XamlDomMember)writerStack.Peek();
			xamlDomMember.Items.Add(xamlDomValue);
		}
		else
		{
			RootNode = xamlDomValue;
		}
		xamlDomValue.StartLineNumber = _lineNumber;
		xamlDomValue.StartLinePosition = _linePosition;
		xamlDomValue.EndLineNumber = _lineNumber;
		xamlDomValue.EndLinePosition = _linePosition;
	}

	public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
	{
		if (_namespaces == null)
		{
			_namespaces = new List<XamlDomNamespace>();
		}
		XamlDomNamespace xamlDomNamespace = new XamlDomNamespace(namespaceDeclaration, _sourceFilePath);
		xamlDomNamespace.StartLineNumber = _lineNumber;
		xamlDomNamespace.StartLinePosition = _linePosition;
		xamlDomNamespace.EndLineNumber = _lineNumber;
		xamlDomNamespace.EndLinePosition = _linePosition;
		_namespaces.Add(xamlDomNamespace);
	}

	void IXamlLineInfoConsumer.SetLineInfo(int lineNumber, int linePosition)
	{
		_lineNumber = lineNumber;
		_linePosition = linePosition;
	}
}
