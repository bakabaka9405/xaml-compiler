using System.Collections.Generic;
using System.Text;

namespace System.Xaml;

internal class XamlMarkupExtensionWriter : XamlWriter
{
	private class Node
	{
		public XamlMember XamlProperty { get; set; }

		public XamlPropertySet Members { get; set; }

		public XamlNodeType NodeType { get; set; }

		public XamlType XamlType { get; set; }
	}

	private abstract class WriterState
	{
		private static char[] specialChars = new char[8] { '\'', '"', ',', '=', '{', '}', '\\', ' ' };

		public virtual void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
		{
			writer.failed = true;
		}

		public virtual void WriteGetObject(XamlMarkupExtensionWriter writer)
		{
			writer.failed = true;
		}

		public virtual void WriteEndObject(XamlMarkupExtensionWriter writer)
		{
			writer.failed = true;
		}

		public virtual void WriteStartMember(XamlMarkupExtensionWriter writer, XamlMember property)
		{
			writer.failed = true;
		}

		public virtual void WriteEndMember(XamlMarkupExtensionWriter writer)
		{
			writer.failed = true;
		}

		public virtual void WriteValue(XamlMarkupExtensionWriter writer, string value)
		{
			writer.failed = true;
		}

		public virtual void WriteNamespace(XamlMarkupExtensionWriter writer, NamespaceDeclaration namespaceDeclaration)
		{
			writer.failed = true;
		}

		protected static bool ContainCharacterToEscape(string s)
		{
			return s.IndexOfAny(specialChars) >= 0;
		}

		protected static string FormatStringInCorrectSyntax(string s)
		{
			StringBuilder stringBuilder = new StringBuilder("\"");
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '\\' || s[i] == '"')
				{
					stringBuilder.Append("\\");
				}
				stringBuilder.Append(s[i]);
			}
			stringBuilder.Append("\"");
			return stringBuilder.ToString();
		}

		protected void WritePrefix(XamlMarkupExtensionWriter writer, string prefix)
		{
			if (prefix != "")
			{
				writer.sb.Append(prefix);
				writer.sb.Append(":");
			}
		}

		public void WriteString(XamlMarkupExtensionWriter writer, string value)
		{
			if (ContainCharacterToEscape(value) || value == string.Empty)
			{
				value = FormatStringInCorrectSyntax(value);
			}
			writer.sb.Append(value);
		}
	}

	private class Start : WriterState
	{
		private static WriterState state = new Start();

		public static WriterState State => state;

		private Start()
		{
		}

		public override void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
		{
			writer.Reset();
			string prefix = writer.LookupPrefix(type);
			writer.sb.Append("{");
			WritePrefix(writer, prefix);
			writer.sb.Append(XamlXmlWriter.GetTypeName(type));
			writer.nodes.Push(new Node
			{
				NodeType = XamlNodeType.StartObject,
				XamlType = type
			});
			writer.currentState = InObjectBeforeMember.State;
		}
	}

	private abstract class InObject : WriterState
	{
		public abstract string Delimiter { get; }

		public override void WriteEndObject(XamlMarkupExtensionWriter writer)
		{
			if (writer.nodes.Count == 0)
			{
				throw new InvalidOperationException(SR.Get("XamlMarkupExtensionWriterInputInvalid"));
			}
			Node node = writer.nodes.Pop();
			if (node.NodeType != XamlNodeType.StartObject)
			{
				throw new InvalidOperationException(SR.Get("XamlMarkupExtensionWriterInputInvalid"));
			}
			writer.sb.Append("}");
			if (writer.nodes.Count == 0)
			{
				writer.currentState = Start.State;
				return;
			}
			Node node2 = writer.nodes.Peek();
			if (node2.NodeType != XamlNodeType.StartMember)
			{
				throw new InvalidOperationException(SR.Get("XamlMarkupExtensionWriterInputInvalid"));
			}
			if (node2.XamlProperty == XamlLanguage.PositionalParameters)
			{
				writer.currentState = InPositionalParametersAfterValue.State;
			}
			else
			{
				writer.currentState = InMemberAfterValueOrEndObject.State;
			}
		}

		protected void UpdateStack(XamlMarkupExtensionWriter writer, XamlMember property)
		{
			if (writer.nodes.Count == 0)
			{
				throw new InvalidOperationException(SR.Get("XamlMarkupExtensionWriterInputInvalid"));
			}
			Node node = writer.nodes.Peek();
			if (node.NodeType != XamlNodeType.StartObject)
			{
				throw new InvalidOperationException(SR.Get("XamlMarkupExtensionWriterInputInvalid"));
			}
			writer.CheckMemberForUniqueness(node, property);
			writer.nodes.Push(new Node
			{
				NodeType = XamlNodeType.StartMember,
				XamlType = node.XamlType,
				XamlProperty = property
			});
		}

		protected void WriteNonPositionalParameterMember(XamlMarkupExtensionWriter writer, XamlMember property)
		{
			if (XamlXmlWriter.IsImplicit(property) || (property.IsDirective && (property.Type.IsCollection || property.Type.IsDictionary)))
			{
				writer.failed = true;
				return;
			}
			if (property.IsDirective)
			{
				writer.sb.Append(Delimiter);
				WritePrefix(writer, writer.LookupPrefix(property));
				writer.sb.Append(property.Name);
			}
			else if (property.IsAttachable)
			{
				writer.sb.Append(Delimiter);
				WritePrefix(writer, writer.LookupPrefix(property));
				string value = property.DeclaringType.Name + "." + property.Name;
				writer.sb.Append(value);
			}
			else
			{
				writer.sb.Append(Delimiter);
				writer.sb.Append(property.Name);
			}
			writer.sb.Append("=");
			writer.currentState = InMember.State;
		}
	}

	private class InObjectBeforeMember : InObject
	{
		private static WriterState state = new InObjectBeforeMember();

		public static WriterState State => state;

		public override string Delimiter => " ";

		private InObjectBeforeMember()
		{
		}

		public override void WriteStartMember(XamlMarkupExtensionWriter writer, XamlMember property)
		{
			UpdateStack(writer, property);
			if (property == XamlLanguage.PositionalParameters)
			{
				writer.currentState = InPositionalParametersBeforeValue.State;
			}
			else
			{
				WriteNonPositionalParameterMember(writer, property);
			}
		}
	}

	private class InObjectAfterMember : InObject
	{
		private static WriterState state = new InObjectAfterMember();

		public static WriterState State => state;

		public override string Delimiter => ", ";

		private InObjectAfterMember()
		{
		}

		public override void WriteStartMember(XamlMarkupExtensionWriter writer, XamlMember property)
		{
			UpdateStack(writer, property);
			WriteNonPositionalParameterMember(writer, property);
		}
	}

	private abstract class InPositionalParameters : WriterState
	{
		public abstract string Delimiter { get; }

		public override void WriteValue(XamlMarkupExtensionWriter writer, string value)
		{
			writer.sb.Append(Delimiter);
			WriteString(writer, value);
			writer.currentState = InPositionalParametersAfterValue.State;
		}

		public override void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
		{
			writer.sb.Append(Delimiter);
			writer.currentState = InMember.State;
			writer.currentState.WriteStartObject(writer, type);
		}
	}

	private class InPositionalParametersBeforeValue : InPositionalParameters
	{
		private static WriterState state = new InPositionalParametersBeforeValue();

		public static WriterState State => state;

		public override string Delimiter => " ";

		private InPositionalParametersBeforeValue()
		{
		}
	}

	private class InPositionalParametersAfterValue : InPositionalParameters
	{
		private static WriterState state = new InPositionalParametersAfterValue();

		public static WriterState State => state;

		public override string Delimiter => ", ";

		private InPositionalParametersAfterValue()
		{
		}

		public override void WriteEndMember(XamlMarkupExtensionWriter writer)
		{
			Node node = writer.nodes.Pop();
			if (node.NodeType != XamlNodeType.StartMember || node.XamlProperty != XamlLanguage.PositionalParameters)
			{
				throw new InvalidOperationException(SR.Get("XamlMarkupExtensionWriterInputInvalid"));
			}
			writer.currentState = InObjectAfterMember.State;
		}
	}

	private class InMember : WriterState
	{
		private static WriterState state = new InMember();

		public static WriterState State => state;

		private InMember()
		{
		}

		public override void WriteValue(XamlMarkupExtensionWriter writer, string value)
		{
			WriteString(writer, value);
			writer.currentState = InMemberAfterValueOrEndObject.State;
		}

		public override void WriteStartObject(XamlMarkupExtensionWriter writer, XamlType type)
		{
			if (!type.IsMarkupExtension)
			{
				writer.failed = true;
				return;
			}
			string prefix = writer.LookupPrefix(type);
			writer.sb.Append("{");
			WritePrefix(writer, prefix);
			writer.sb.Append(XamlXmlWriter.GetTypeName(type));
			writer.nodes.Push(new Node
			{
				NodeType = XamlNodeType.StartObject,
				XamlType = type
			});
			writer.currentState = InObjectBeforeMember.State;
		}
	}

	private class InMemberAfterValueOrEndObject : WriterState
	{
		private static WriterState state = new InMemberAfterValueOrEndObject();

		public static WriterState State => state;

		private InMemberAfterValueOrEndObject()
		{
		}

		public override void WriteEndMember(XamlMarkupExtensionWriter writer)
		{
			if (writer.nodes.Count == 0)
			{
				throw new InvalidOperationException(SR.Get("XamlMarkupExtensionWriterInputInvalid"));
			}
			Node node = writer.nodes.Pop();
			if (node.NodeType != XamlNodeType.StartMember)
			{
				throw new InvalidOperationException(SR.Get("XamlMarkupExtensionWriterInputInvalid"));
			}
			writer.currentState = InObjectAfterMember.State;
		}
	}

	private StringBuilder sb;

	private Stack<Node> nodes;

	private WriterState currentState;

	private XamlXmlWriter xamlXmlWriter;

	private XamlXmlWriterSettings settings;

	private XamlMarkupExtensionWriterSettings meSettings;

	private bool failed;

	public override XamlSchemaContext SchemaContext => xamlXmlWriter.SchemaContext;

	public string MarkupExtensionString
	{
		get
		{
			if (nodes.Count == 0)
			{
				return sb.ToString();
			}
			return null;
		}
	}

	public bool Failed => failed;

	public XamlMarkupExtensionWriter(XamlXmlWriter xamlXmlWriter)
	{
		Initialize(xamlXmlWriter);
	}

	public XamlMarkupExtensionWriter(XamlXmlWriter xamlXmlWriter, XamlMarkupExtensionWriterSettings meSettings)
	{
		this.meSettings = meSettings;
		Initialize(xamlXmlWriter);
	}

	private void Initialize(XamlXmlWriter xamlXmlWriter)
	{
		this.xamlXmlWriter = xamlXmlWriter;
		settings = xamlXmlWriter.Settings;
		meSettings = meSettings ?? new XamlMarkupExtensionWriterSettings();
		currentState = Start.State;
		sb = new StringBuilder();
		nodes = new Stack<Node>();
		failed = false;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	public void Reset()
	{
		currentState = Start.State;
		sb = new StringBuilder();
		nodes.Clear();
		failed = false;
	}

	private string LookupPrefix(XamlType type)
	{
		string chosenNamespace;
		string text = xamlXmlWriter.LookupPrefix(type.GetXamlNamespaces(), out chosenNamespace);
		if (text == null && !meSettings.ContinueWritingWhenPrefixIsNotFound)
		{
			failed = true;
			return string.Empty;
		}
		return text;
	}

	private string LookupPrefix(XamlMember property)
	{
		string chosenNamespace;
		string text = xamlXmlWriter.LookupPrefix(property.GetXamlNamespaces(), out chosenNamespace);
		if (text == null && !meSettings.ContinueWritingWhenPrefixIsNotFound)
		{
			failed = true;
			return string.Empty;
		}
		return text;
	}

	private void CheckMemberForUniqueness(Node objectNode, XamlMember property)
	{
		if (!settings.AssumeValidInput)
		{
			if (objectNode.Members == null)
			{
				objectNode.Members = new XamlPropertySet();
			}
			else if (objectNode.Members.Contains(property))
			{
				throw new InvalidOperationException(SR.Get("XamlMarkupExtensionWriterDuplicateMember", property.Name));
			}
			objectNode.Members.Add(property);
		}
	}

	public override void WriteStartObject(XamlType type)
	{
		currentState.WriteStartObject(this, type);
	}

	public override void WriteGetObject()
	{
		currentState.WriteGetObject(this);
	}

	public override void WriteEndObject()
	{
		currentState.WriteEndObject(this);
	}

	public override void WriteStartMember(XamlMember property)
	{
		currentState.WriteStartMember(this, property);
	}

	public override void WriteEndMember()
	{
		currentState.WriteEndMember(this);
	}

	public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
	{
		currentState.WriteNamespace(this, namespaceDeclaration);
	}

	public override void WriteValue(object value)
	{
		if (!(value is string value2))
		{
			throw new ArgumentException(SR.Get("XamlMarkupExtensionWriterCannotWriteNonstringValue"));
		}
		currentState.WriteValue(this, value2);
	}
}
