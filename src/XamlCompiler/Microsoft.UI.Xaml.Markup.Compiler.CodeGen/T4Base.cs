using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xaml;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal abstract class T4Base<TModel> : T4Base
{
	public TModel Model { get; set; }

	public override void SetModel(XamlProjectInfo projectInfo, XamlSchemaCodeInfo schemaInfo, object model)
	{
		Model = (TModel)model;
		base.ProjectInfo = projectInfo;
		base.SchemaInfo = schemaInfo;
	}
}
internal abstract class T4Base
{
	private static string[] Indents = new string[9] { "", "    ", "        ", "            ", "                ", "                    ", "                        ", "                            ", "                                " };

	private StringBuilder generationEnvironmentField;

	private CompilerErrorCollection errorsField;

	private List<int> indentLengthsField;

	private string currentIndentField = "";

	private bool endsWithNewline;

	public XamlProjectInfo ProjectInfo { get; protected set; }

	public XamlSchemaCodeInfo SchemaInfo { get; protected set; }

	public object[] Arguments { get; set; }

	protected StringBuilder GenerationEnvironment
	{
		get
		{
			if (generationEnvironmentField == null)
			{
				generationEnvironmentField = new StringBuilder();
			}
			return generationEnvironmentField;
		}
		set
		{
			generationEnvironmentField = value;
		}
	}

	public CompilerErrorCollection Errors
	{
		get
		{
			if (errorsField == null)
			{
				errorsField = new CompilerErrorCollection();
			}
			return errorsField;
		}
	}

	private List<int> indentLengths
	{
		get
		{
			if (indentLengthsField == null)
			{
				indentLengthsField = new List<int>();
			}
			return indentLengthsField;
		}
	}

	public T4Base ToStringHelper => this;

	public virtual string TransformText()
	{
		return null;
	}

	protected string IncludeTemplate<TTemplate>() where TTemplate : T4Base, new()
	{
		return IncludeTemplate<TTemplate>(null, Array.Empty<object>());
	}

	protected string IncludeTemplate<TTemplate>(object model, params object[] args) where TTemplate : T4Base, new()
	{
		TTemplate val = new TTemplate();
		if (model != null)
		{
			val.SetModel(ProjectInfo, SchemaInfo, model);
		}
		val.Arguments = args;
		string text = val.TransformText().Trim('\r', '\n');
		if (!string.IsNullOrEmpty(text))
		{
			PushIndent(GetCurrentIndent());
			WriteLine(text);
			PopIndent();
		}
		return string.Empty;
	}

	public abstract void SetModel(XamlProjectInfo projectInfo, XamlSchemaCodeInfo schemaInfo, object model);

	protected Indent GetCurrentIndent()
	{
		if (GenerationEnvironment.Length > 0)
		{
			int num = GenerationEnvironment.Length - 1;
			while (num >= 0 && GenerationEnvironment[num] == ' ')
			{
				num--;
			}
			if (GenerationEnvironment[num] == '\n')
			{
				return (Indent)((GenerationEnvironment.Length - num - 1) / 4);
			}
			if (num == 0)
			{
				return (Indent)((GenerationEnvironment.Length - num) / 4);
			}
		}
		return Indent.None;
	}

	public void Write(string textToAppend)
	{
		if (string.IsNullOrEmpty(textToAppend))
		{
			return;
		}
		if (GenerationEnvironment.Length == 0 || endsWithNewline)
		{
			GenerationEnvironment.Append(currentIndentField);
			endsWithNewline = false;
		}
		if (textToAppend.EndsWith(Environment.NewLine, StringComparison.CurrentCulture))
		{
			endsWithNewline = true;
		}
		if (currentIndentField.Length == 0)
		{
			GenerationEnvironment.Append(textToAppend);
			return;
		}
		textToAppend = textToAppend.Replace(Environment.NewLine, Environment.NewLine + currentIndentField);
		if (endsWithNewline)
		{
			GenerationEnvironment.Append(textToAppend, 0, textToAppend.Length - currentIndentField.Length);
		}
		else
		{
			GenerationEnvironment.Append(textToAppend);
		}
	}

	public void WriteLine(string textToAppend)
	{
		Write(textToAppend);
		GenerationEnvironment.AppendLine();
		endsWithNewline = true;
	}

	public void Write(string format, params object[] args)
	{
		Write(string.Format(CultureInfo.CurrentCulture, format, args));
	}

	public void WriteLine(string format, params object[] args)
	{
		WriteLine(string.Format(CultureInfo.CurrentCulture, format, args));
	}

	public void Error(string message)
	{
		CompilerError compilerError = new CompilerError();
		compilerError.ErrorText = message;
		Errors.Add(compilerError);
	}

	public void Warning(string message)
	{
		CompilerError compilerError = new CompilerError();
		compilerError.ErrorText = message;
		compilerError.IsWarning = true;
		Errors.Add(compilerError);
	}

	public void PushIndent(Indent tabs = Indent.OneTab)
	{
		string text = Indents[(int)tabs];
		currentIndentField += text;
		indentLengths.Add(text.Length);
	}

	public string PopIndent()
	{
		string result = "";
		if (indentLengths.Count > 0)
		{
			int num = indentLengths[indentLengths.Count - 1];
			indentLengths.RemoveAt(indentLengths.Count - 1);
			if (num > 0)
			{
				result = currentIndentField.Substring(currentIndentField.Length - num);
				currentIndentField = currentIndentField.Remove(currentIndentField.Length - num);
			}
		}
		return result;
	}

	public void ClearIndent()
	{
		indentLengths.Clear();
		currentIndentField = "";
	}

	public abstract string ToStringWithCulture(ICodeGenOutput codegenOutput);

	public virtual string ToStringWithCulture(bool value)
	{
		if (!value)
		{
			return "false";
		}
		return "true";
	}

	public abstract string ToStringWithCulture(XamlType type);

	public string ToStringWithCulture(string objectToConvert)
	{
		return objectToConvert.ToString();
	}

	public string ToStringWithCulture(int objectToConvert)
	{
		return objectToConvert.ToString();
	}
}
