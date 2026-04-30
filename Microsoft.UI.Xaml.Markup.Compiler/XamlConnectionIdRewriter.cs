using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler;

internal class XamlConnectionIdRewriter
{
	private string[] xamlLines;

	private List<XamlCompileError> errors = new List<XamlCompileError>();

	private static char[] Whitespace = new char[4] { ' ', '\r', '\n', '\t' };

	private static char[] QuoteCharacters = new char[2] { '"', '\'' };

	public List<XamlCompileError> Errors => errors;

	public string Parse(string xamlText, IXamlClassCodeInfo classCodeInfo, IXamlFileCodeInfo fileCodeInfo)
	{
		if (classCodeInfo == null)
		{
			throw new ArgumentNullException("classCodeInfo");
		}
		if (fileCodeInfo == null)
		{
			throw new ArgumentNullException("fileCodeInfo");
		}
		xamlLines = ReadAllLinesOfString(xamlText);
		return ProcessLines(classCodeInfo, fileCodeInfo);
	}

	public string Edit(string xamlFileName, IXamlClassCodeInfo classCodeInfo, IXamlFileCodeInfo fileCodeInfo)
	{
		if (classCodeInfo == null)
		{
			throw new ArgumentNullException("classCodeInfo");
		}
		if (fileCodeInfo == null)
		{
			throw new ArgumentNullException("fileCodeInfo");
		}
		if (classCodeInfo.IsApplication)
		{
			return ReadAllTextFromFile(xamlFileName);
		}
		xamlLines = ReadAllLinesFromFile(xamlFileName);
		return ProcessLines(classCodeInfo, fileCodeInfo);
	}

	internal virtual string[] ReadAllLinesFromFile(string xamlFileName)
	{
		try
		{
			return File.ReadAllLines(xamlFileName);
		}
		catch (Exception ex)
		{
			Errors.Add(new XamlRewriterErrorFileOpenFailure(xamlFileName, ex.Message));
			return null;
		}
	}

	internal virtual string ReadAllTextFromFile(string xamlFileName)
	{
		try
		{
			return File.ReadAllText(xamlFileName);
		}
		catch (Exception ex)
		{
			Errors.Add(new XamlRewriterErrorFileOpenFailure(xamlFileName, ex.Message));
			return null;
		}
	}

	internal static int ConnectionIdElementComparer(ConnectionIdElement a, ConnectionIdElement b)
	{
		if (a.LineNumberInfo.StartLineNumber < b.LineNumberInfo.StartLineNumber)
		{
			return -1;
		}
		if (a.LineNumberInfo.StartLineNumber > b.LineNumberInfo.StartLineNumber)
		{
			return 1;
		}
		if (a.LineNumberInfo.StartLinePosition < b.LineNumberInfo.StartLinePosition)
		{
			return -1;
		}
		if (a.LineNumberInfo.StartLinePosition > b.LineNumberInfo.StartLinePosition)
		{
			return 1;
		}
		return 0;
	}

	private static string[] ReadAllLinesOfString(string xamlText)
	{
		return xamlText.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.None);
	}

	private static string GetSpaces(int start, int end)
	{
		return new string(' ', end - start + 1);
	}

	private string ProcessLines(IXamlClassCodeInfo classCodeInfo, IXamlFileCodeInfo fileCodeInfo)
	{
		List<ConnectionIdElement> list = fileCodeInfo.ConnectionIdElements.Where((ConnectionIdElement c) => c.HasRewritableAttributes).ToList();
		foreach (ConnectionIdElement item in list)
		{
			foreach (EventAssignment eventAssignment in item.EventAssignments)
			{
				AttributeProcessing(eventAssignment);
			}
			foreach (BindAssignment bindAssignment in item.BindAssignments)
			{
				AttributeProcessing(bindAssignment, allowMultiline: true);
				bindAssignment.ParsePath();
			}
			if (item.HasPhase)
			{
				PhaseAssignment phaseAssignment = item.PhaseAssignment;
				AttributeProcessing(phaseAssignment);
			}
			foreach (BoundEventAssignment boundEventAssignment in item.BoundEventAssignments)
			{
				AttributeProcessing(boundEventAssignment, allowMultiline: true);
				boundEventAssignment.ParsePath();
			}
		}
		foreach (DataTypeAssignment dataTypeAssignment in fileCodeInfo.DataTypeAssignments)
		{
			AttributeProcessing(dataTypeAssignment);
		}
		foreach (StrippableMember strippableMember in fileCodeInfo.StrippableMembers)
		{
			AttributeProcessing(strippableMember);
		}
		foreach (StrippableMember suppressXamlTrimWarningsBindingMember in fileCodeInfo.SuppressXamlTrimWarningsBindingMembers)
		{
			StripSuppressXamlTrimWarningDirectiveInBinding(suppressXamlTrimWarningsBindingMember);
		}
		foreach (StrippableNamespace strippableNamespace in fileCodeInfo.StrippableNamespaces)
		{
			AttributeProcessingNamespace(strippableNamespace);
		}
		foreach (StrippableObject strippableObject in fileCodeInfo.StrippableObjects)
		{
			StripObject(strippableObject);
		}
		if (Errors.Count > 0)
		{
			return null;
		}
		list.Sort(ConnectionIdElementComparer);
		list.Reverse();
		foreach (ConnectionIdElement item2 in list)
		{
			int num = item2.LineNumberInfo.StartLineNumber - 1;
			int startIndex = item2.LineNumberInfo.StartLinePosition - 1;
			string text = xamlLines[num];
			int num2 = text.IndexOfAny(Whitespace, startIndex);
			string text2 = string.Format(CultureInfo.InvariantCulture, " {0}:ConnectionId='{1}'", "x", item2.ConnectionId);
			string text3 = null;
			if (num2 == -1)
			{
				num2 = text.IndexOf(">");
				if (num2 != -1 && text[num2 - 1] == '\\')
				{
					num2--;
				}
			}
			text3 = ((num2 == -1) ? (text + text2) : (text.Substring(0, num2) + text2 + text.Substring(num2)));
			xamlLines[num] = text3;
		}
		StringBuilder stringBuilder = new StringBuilder();
		string[] array = xamlLines;
		foreach (string value in array)
		{
			stringBuilder.AppendLine(value);
		}
		return stringBuilder.ToString();
	}

	private void ReplaceWithSpaces(SourcePos start, SourcePos end)
	{
		for (int i = start.Row - 1; i < end.Row; i++)
		{
			int num = 0;
			int num2 = xamlLines[i].Length - 1;
			string text = xamlLines[i];
			if (i == start.Row - 1)
			{
				num = start.Col - 1;
			}
			if (i == end.Row - 1)
			{
				num2 = end.Col - 1;
			}
			string spaces = GetSpaces(num, num2);
			string text2 = text.Substring(0, num) + spaces + text.Substring(num2 + 1);
			xamlLines[i] = text2;
		}
	}

	private void StripObject(ILineNumberAndErrorInfo element)
	{
		LineNumberInfo lineNumberInfo = element.LineNumberInfo;
		FixedSourceInfo fixedSourceInfo = XamlSourceInfoFixer.GetFixedSourceInfo(element, xamlLines);
		ReplaceWithSpaces(fixedSourceInfo.StartOpeningTag, fixedSourceInfo.EndClosingTag);
	}

	private void AttributeProcessingNamespace(StrippableNamespace element)
	{
		int num = element.LineNumberInfo.StartLineNumber - 1;
		int num2 = element.LineNumberInfo.StartLinePosition - 1;
		string text = xamlLines[num];
		int num3 = -1;
		int num4 = -1;
		char c = '"';
		num3 = text.IndexOfAny(QuoteCharacters, num2);
		if (num3 != -1)
		{
			c = text[num3];
			if (text.Length > num3)
			{
				num4 = text.IndexOf(c, num3 + 1);
			}
		}
		if (num3 == -1 || num4 == -1)
		{
			Errors.Add(new XamlRewriterErrorLineBreak(element.LineNumberInfo.StartLineNumber, element.LineNumberInfo.StartLinePosition));
			return;
		}
		if (element.StripWholeNamespace)
		{
			string spaces = GetSpaces(num2, num4);
			string text2 = text.Substring(0, num2) + spaces + text.Substring(num4 + 1);
			xamlLines[num] = text2;
			return;
		}
		string text3 = text.Substring(num3, num4 - num3 + 1);
		int num5 = text3.IndexOf("TargetPlatform", StringComparison.OrdinalIgnoreCase);
		int num6 = text3.IndexOf(')', num5);
		if (text3[num6 + 1] != ';')
		{
			num5--;
		}
		else
		{
			num6++;
		}
		num5 += num3;
		num6 += num3;
		string spaces2 = GetSpaces(num5, num6);
		string text4 = text.Substring(0, num5) + text.Substring(num6 + 1, num4 - num6) + spaces2 + text.Substring(num4 + 1);
		xamlLines[num] = text4;
	}

	private void AttributeProcessing(ILineNumberAndErrorInfo element, bool allowMultiline = false)
	{
		LineNumberInfo lineNumberInfo = element.LineNumberInfo;
		if (lineNumberInfo.StartLineNumber != lineNumberInfo.EndLineNumber || lineNumberInfo.StartLinePosition != lineNumberInfo.EndLinePosition)
		{
			Errors.Add(element.GetAttributeProcessingError());
			return;
		}
		int num = lineNumberInfo.StartLineNumber - 1;
		int num2 = lineNumberInfo.StartLinePosition - 1;
		string text = xamlLines[num];
		int num3 = -1;
		int num4 = -1;
		char value = 'x';
		num3 = text.IndexOfAny(QuoteCharacters, num2);
		if (num3 != -1)
		{
			value = text[num3];
			if (text.Length > num3)
			{
				num4 = text.IndexOf(value, num3 + 1);
			}
		}
		if (num3 != -1 && num4 != -1)
		{
			string text2 = GetSpaces(num2, num4);
			if (element is BoundLoadAssignment)
			{
				text2 = OverwriteText(text2, "x:Load=\"False\"");
			}
			string text3 = text.Substring(0, num2) + text2 + text.Substring(num4 + 1);
			xamlLines[num] = text3;
		}
		else if (num3 != -1 && allowMultiline)
		{
			string text4 = GetSpaces(num2, text.Length - 1);
			if (element is BoundLoadAssignment)
			{
				text4 = OverwriteText(text4, "x:Load=\"False\"");
			}
			xamlLines[num] = text.Substring(0, num2) + text4;
			for (int i = num + 1; i < xamlLines.Length; i++)
			{
				text = xamlLines[i];
				num4 = text.IndexOf(value);
				if (num4 == -1)
				{
					text4 = GetSpaces(0, text.Length - 1);
					xamlLines[i] = text4;
					continue;
				}
				text4 = GetSpaces(0, num4);
				xamlLines[i] = text4 + text.Substring(num4 + 1);
				break;
			}
		}
		else
		{
			Errors.Add(new XamlRewriterErrorLineBreak(lineNumberInfo.StartLineNumber, lineNumberInfo.StartLinePosition));
		}
	}

	private string OverwriteText(string text, string replacement)
	{
		if (text.Length > replacement.Length)
		{
			return replacement + text.Substring(replacement.Length);
		}
		return replacement;
	}

	private void StripSuppressXamlTrimWarningDirectiveInBinding(ILineNumberAndErrorInfo element)
	{
		LineNumberInfo lineNumberInfo = element.LineNumberInfo;
		if (lineNumberInfo.StartLineNumber != lineNumberInfo.EndLineNumber || lineNumberInfo.StartLinePosition != lineNumberInfo.EndLinePosition)
		{
			Errors.Add(element.GetAttributeProcessingError());
			return;
		}
		int num = lineNumberInfo.StartLineNumber - 1;
		int num2 = lineNumberInfo.StartLinePosition - 1;
		string text = xamlLines[num];
		int num3 = text.IndexOf("x:SuppressXamlTrimWarnings", num2, StringComparison.OrdinalIgnoreCase);
		if (num3 == -1)
		{
			Errors.Add(element.GetAttributeProcessingError());
			return;
		}
		int num4 = text.IndexOf("True", num3 + "x:SuppressXamlTrimWarnings".Length, StringComparison.OrdinalIgnoreCase);
		int length = "True".Length;
		if (num4 == -1)
		{
			num4 = text.IndexOf("False", num3 + "x:SuppressXamlTrimWarnings".Length, StringComparison.OrdinalIgnoreCase);
			length = "False".Length;
		}
		if (num4 == -1)
		{
			Errors.Add(element.GetAttributeProcessingError());
			return;
		}
		int num5 = text.LastIndexOf(',', num3, num3 - num2);
		num2 = ((num5 == -1) ? num3 : num5);
		int num6 = num4 + length;
		string spaces = GetSpaces(num2, num6 - 1);
		text = text.Substring(0, num2) + spaces + text.Substring(num6);
		xamlLines[num] = text;
	}
}
