using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.UI.Xaml.Markup.Compiler.CodeGen;

internal static class StringExtensions
{
	internal static string AsNamespaceDeclarationBegin(this string instance)
	{
		return "namespace " + instance.Replace(".", " { namespace ");
	}

	internal static string AsNamespaceDeclarationEnd(this string instance)
	{
		return instance.Where((char c) => c == '.').Aggregate("", (string current, char c) => current + "}");
	}

	internal static string Quotenate(this string value)
	{
		return $"\"{value}\"";
	}

	internal static string ToTitleCase(this string value)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		foreach (char c in value)
		{
			stringBuilder.Append(flag ? char.ToUpper(c) : c);
			flag = char.IsWhiteSpace(c);
		}
		return stringBuilder.ToString();
	}

	internal static string ToLocalCppWinRTTypeName(this string fullName)
	{
		int num = fullName.LastIndexOf("::");
		if (num > 0)
		{
			return fullName.Insert(num, "::implementation");
		}
		return fullName;
	}

	internal static string ToCommaSeparatedValues(this IEnumerable<object> list)
	{
		int num = 0;
		string text = "";
		foreach (object item in list)
		{
			text += ((num++ == 0) ? $"{item}" : $", {item}");
		}
		return text;
	}
}
