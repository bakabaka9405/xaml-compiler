using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Markup.Compiler.MSBuildInterop;

namespace Microsoft.UI.Xaml.Markup.Compiler.Executable;

internal class ConsoleLogger : ILog
{
	public List<MSBuildLogEntry> Entries { get; private set; } = new List<MSBuildLogEntry>();

	public void LogError(string subcategory, string errorCode, string helpKeyword, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message)
	{
		Entries.Add(new MSBuildLogEntry(MSBuildLogEntry.EntryType.Error, subcategory, errorCode, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message));

		string location = FormatLocation(file, lineNumber, columnNumber);
		string prefix = string.IsNullOrEmpty(location) ? string.Empty : $"{location}: ";
		Console.Error.WriteLine($"{prefix}error {errorCode}: {message}");
	}

	public void LogWarning(string subcategory, string warningCode, string helpKeyword, string file, int lineNumber, int columnNumber, int endLineNumber, int endColumnNumber, string message)
	{
		Entries.Add(new MSBuildLogEntry(MSBuildLogEntry.EntryType.Warning, subcategory, warningCode, helpKeyword, file, lineNumber, columnNumber, endLineNumber, endColumnNumber, message));

		string location = FormatLocation(file, lineNumber, columnNumber);
		string prefix = string.IsNullOrEmpty(location) ? string.Empty : $"{location}: ";
		Console.Error.WriteLine($"{prefix}warning {warningCode}: {message}");
	}

	public void LogDiagnosticMessage(string message)
	{
		Entries.Add(new MSBuildLogEntry(MSBuildLogEntry.EntryType.Message, message));
	}

	private static string FormatLocation(string file, int lineNumber, int columnNumber)
	{
		if (string.IsNullOrEmpty(file))
		{
			return string.Empty;
		}
		if (lineNumber > 0 && columnNumber > 0)
		{
			return $"{file}({lineNumber},{columnNumber})";
		}
		return file;
	}
}
